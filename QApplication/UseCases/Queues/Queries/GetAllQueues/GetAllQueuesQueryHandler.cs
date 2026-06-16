using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Caching;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Queues.Queries.GetAllQueues;

public class GetAllQueuesQueryHandler: IRequestHandler<GetAllQueuesQuery, PagedResponse<QueueResponseModel>>
{
    private const int PageSize = 15;
    private readonly ILogger<GetAllQueuesQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;
    public GetAllQueuesQueryHandler(ILogger<GetAllQueuesQueryHandler> logger, IQueueApplicationDbContext dbContext, ICacheService cache, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _cache = cache;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<PagedResponse<QueueResponseModel>> Handle(GetAllQueuesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all queues. PageNumber: {pageNumber}, PageSize: {pageSize}", request.PageNumber, PageSize);

        var userIdClaim = _contextAccessor.HttpContext!.User.FindFirst("id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        
        var currentEmployee = await _userService.GetCurrentEmployee(new CurrentUserRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId
        });
        var companyId = currentEmployee.CompanyId;
        
        var hashKey = CacheKeys.AllQueuesHashKey;
        var filed = CacheKeys.AllQueuesField(request.PageNumber );

        var cached = await _cache.HashGetAsync<PagedResponse<QueueResponseModel>>(hashKey, filed);

        if (cached is not null)
        {
            return cached;
        }
        
        var totalCount = await _dbContext.Queues
            .Where(s=>s.CompanyId== companyId)
            .CountAsync(cancellationToken);

        var dbQueues =await  _dbContext.Queues
            .Where(s=>s.CompanyId== companyId)
            .OrderBy(s => s.Id)
            .Skip((request.PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);
                
                
        var response = dbQueues.Select(queue => new QueueResponseModel()
        {
            Id = queue.Id,
            CompanyId = queue.CompanyId,
            BranchId = queue.BranchId,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            ServiceId = queue.ServiceId,
            StartTime = queue.StartTime,
            Status = queue.Status
        }).ToList();

                
                
        _logger.LogInformation("Fetched {queueCount} queues.", response.Count);

        var pagedResponse =new PagedResponse<QueueResponseModel>
        {
            Items = response,
            PageNumber = request.PageNumber,
            PageSize = PageSize,
            TotalCount = totalCount
        };
        
        await _cache.HashSetAsync(hashKey, filed, pagedResponse, TimeSpan.FromMinutes(10));


        return pagedResponse;
        
    }
}