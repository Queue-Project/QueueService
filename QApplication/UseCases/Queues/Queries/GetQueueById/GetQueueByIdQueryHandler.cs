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

namespace QApplication.UseCases.Queues.Queries.GetQueueById;

public class GetQueueByIdQueryHandler: IRequestHandler<GetQueueByIdQuery, QueueResponseModel>
{
    private readonly ILogger<GetQueueByIdQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly ICacheService _cache;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public GetQueueByIdQueryHandler(ILogger<GetQueueByIdQueryHandler> logger, IQueueApplicationDbContext dbContext, ICacheService cache, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _cache = cache;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<QueueResponseModel> Handle(GetQueueByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting queue by Id {id}", request.Id);

        var userIdClaim = _contextAccessor.HttpContext!.User.FindFirst("id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var isUserEmployee = await _userService.IsCurrentUserEmployee(new CurrentUserRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId
        });
        
        

        int employeeId = 0;
        int customerId = 0;
        if (isUserEmployee.IsEmployee)
        {
            var currentEmployee = await _userService.GetCurrentEmployee(new CurrentUserRequest
            {
                RequestId = Guid.NewGuid(),
                UserId = userId
            });
            employeeId = currentEmployee.EmployeeId;
        }
        else
        {
            var currentCustomer = await _userService.GetCurrentCustomer(new CurrentUserRequest
            {
                RequestId = Guid.NewGuid(),
                UserId = userId
            });
            customerId = currentCustomer.CustomerId;
        }
        
        
        var keyCache = CacheKeys.QueueId(request.Id);
        var queue = await _cache.GetOrCreateAsync(keyCache, async () =>
        {
            _logger.LogInformation($"Cache miss for QueueId: {request.Id}");
            var dbQueue = await _dbContext.Queues
                .Where(s=>isUserEmployee.IsEmployee
                ? s.EmployeeId== employeeId
                : s.CustomerId== customerId)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
            if (dbQueue == null)
            {
                _logger.LogWarning("Queue with Id {id} not found", request.Id);
                throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(QueueEntity));
            }

            var response = new QueueResponseModel()
            {
                Id = dbQueue.Id,
                CompanyId = dbQueue.CompanyId,
                BranchId = dbQueue.BranchId,
                CustomerId = dbQueue.CustomerId,
                EmployeeId = dbQueue.EmployeeId,
                ServiceId = dbQueue.ServiceId,
                StartTime = dbQueue.StartTime,
                EndTime = dbQueue.EndTime,
                Status = dbQueue.Status
            };
            

            _logger.LogInformation("Queue with Id {id} fetched successfully", request.Id);
            return response;

        }, absoluteExpiration: TimeSpan.FromMinutes(10), slidingExpiration: TimeSpan.FromMinutes(5));

        if (queue==null)
        {
            _logger.LogInformation("Data is null. Can not return!");
            throw new Exception("Retrieved data is null");
        }

        return queue;
    }
}