using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Caching;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Queues.Queries.GetQueuesByEmployee;

public class GetQueuesByEmployeeQueryHandler: IRequestHandler<GetQueuesByEmployeeQuery, PagedResponse<QueueResponseModel>>
{
    private const int PageSize=15;
    private readonly ILogger<GetQueuesByEmployeeQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public GetQueuesByEmployeeQueryHandler(ILogger<GetQueuesByEmployeeQueryHandler> logger, IQueueApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, ICacheService cacheService, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _cacheService = cacheService;
        _userService = userService;
    }

    public async Task<PagedResponse<QueueResponseModel>> Handle(GetQueuesByEmployeeQuery request, CancellationToken cancellationToken)
    {
        
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
        var employeeId = currentEmployee.EmployeeId;
        
        _logger.LogInformation("Getting all customer's queue. PageNumber: {pageNumber}, PageSize: {pageSize}",
            request.PageNumber, PageSize);

        var hashKey = CacheKeys.EmployeeQueuesHashKey(employeeId);
        var filed = CacheKeys.EmployeeQueuesField(request.PageNumber);

        var cached = await _cacheService.HashGetAsync<PagedResponse<QueueResponseModel>>(hashKey, filed);

        if (cached is not null)
        {
            return cached;
        }


        var query = _dbContext.Queues.Where(s => s.EmployeeId == employeeId);
        

        var totalCount = await query.CountAsync(cancellationToken);
        var queues = await query
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Skip((request.PageNumber - 1) * PageSize)
            .Take(PageSize).ToListAsync(cancellationToken);

        var response = queues.Select(queue => new QueueResponseModel()
        {
            Id = queue.Id,
            CompanyId = queue.CompanyId,
            BranchId = queue.BranchId,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            ServiceId = queue.ServiceId,
            StartTime = queue.StartTime,
            EndTime = queue.EndTime ?? queue.StartTime.AddMinutes(30),
            Status = queue.Status
        }).ToList();

        _logger.LogInformation("Successfully fetched {QueueCount} queues for EmployeeId: {EmployeeId}", response.Count,
            employeeId);

        var pagedResponse = new PagedResponse<QueueResponseModel>
        {
            Items = response,
            PageNumber = request.PageNumber,
            PageSize = PageSize,
            TotalCount = totalCount
        };
        
        return pagedResponse;
    }
}