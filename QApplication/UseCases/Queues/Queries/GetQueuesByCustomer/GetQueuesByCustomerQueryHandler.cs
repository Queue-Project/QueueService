using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Caching;
using QApplication.Exceptions;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QDomain.Models;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;
using StackExchange.Redis;

namespace QApplication.UseCases.Queues.Queries.GetQueuesByCustomer;

public class GetQueuesByCustomerQueryHandler : IRequestHandler<GetQueuesByCustomerQuery, PagedResponse<QueueResponseModel>>
{
    private const int PageSize = 15;
    private readonly ILogger<GetQueuesByCustomerQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;
   

    public GetQueuesByCustomerQueryHandler(ILogger<GetQueuesByCustomerQueryHandler> logger,
        IQueueApplicationDbContext dbContext, ICacheService cache, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _cache = cache;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<PagedResponse<QueueResponseModel>> Handle(GetQueuesByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        
        
        
        var userIdClaim = _contextAccessor.HttpContext!.User.FindFirst("id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var currentCustomer = await _userService.GetCurrentCustomer(new CurrentUserRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId
        });
        
        if (!currentCustomer.IsValid)
        {
            _logger.LogWarning("User is not a customer");
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, $"User is not a customer");
        }
        var customerId = currentCustomer.CustomerId;
        

        _logger.LogInformation("Getting all customer's queue. PageNumber: {pageNumber}, PageSize: {pageSize}",
            request.PageNumber, PageSize);

       

        var hashKey = CacheKeys.CustomerQueuesHashKey(customerId);
        var filed = CacheKeys.CustomerQueuesField(request.PageNumber);

        var cached = await _cache.HashGetAsync<PagedResponse<QueueResponseModel>>(hashKey, filed);

        if (cached is not null)
        {
            return cached;
        }
        
        var query = _dbContext.Queues
            .Where(s => s.CustomerId == customerId);
        
        if (!query.Any())
        {
            _logger.LogWarning("No queues found for CustomerId: {customerId}", customerId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(QueueEntity));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var queues = await query
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Skip((request.PageNumber - 1) * 15)
            .Take(15).ToListAsync(cancellationToken);


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
        
                
        _logger.LogInformation("Successfully fetched {QueueCount} queues for CustomerId: {CustomerId}",
            response.Count,
            customerId);
        
        
        _logger.LogInformation("Fetched {companyCount} companies.", response.Count);
        var pagedResponse=new PagedResponse<QueueResponseModel>
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