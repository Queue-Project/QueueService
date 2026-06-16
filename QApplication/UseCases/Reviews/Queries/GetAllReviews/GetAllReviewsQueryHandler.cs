using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Reviews.Queries.GetAllReviews;

public class GetAllReviewsQueryHandler: IRequestHandler<GetAllReviewsQuery, PagedResponse<ReviewResponseModel>>
{
    private const int PageSize = 15;
    private readonly ILogger<GetAllReviewsQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public GetAllReviewsQueryHandler(ILogger<GetAllReviewsQueryHandler> logger, IQueueApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<PagedResponse<ReviewResponseModel>> Handle(GetAllReviewsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all reviews. PageNumber: {pageNumber}, PageSize: {pageSize}", request.PageNumber,
            PageSize);

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

        var totalCount = await _dbContext.Reviews
            .Where(s=>isUserEmployee.IsEmployee
            ? s.Queue.EmployeeId== employeeId
            : s.CustomerId== customerId)
            .CountAsync(cancellationToken);

        var dbReviews =await  _dbContext.Reviews
            .Where(s=>isUserEmployee.IsEmployee
            ? s.Queue.EmployeeId== employeeId
            : s.CustomerId== customerId)
            .OrderBy(s => s.Id)
            .Skip((request.PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);

        
        var response = dbReviews.Select(reviews => new ReviewResponseModel()
        {
            Id = reviews.Id,
            CustomerId = reviews.CustomerId,
            QueueId = reviews.QueueId,
            EmployeeId = reviews.Queue.EmployeeId,
            Grade = reviews.Grade,
            ReviewText = reviews.ReviewText
        }).ToList();
        
        _logger.LogInformation("Fetched {reviewCount} reviews.", response.Count);

        return new PagedResponse<ReviewResponseModel>
        {
            Items = response,
            PageNumber = request.PageNumber,
            PageSize = PageSize,
            TotalCount = totalCount
        };
    }
}