using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Exceptions;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QDomain.Models;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Reviews.Queries.GetReviewById;

public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, ReviewResponseModel>
{
    private readonly ILogger<GetReviewByIdQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public GetReviewByIdQueryHandler(ILogger<GetReviewByIdQueryHandler> logger, IQueueApplicationDbContext dbContext,
        IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<ReviewResponseModel> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting review by Id {id}", request.Id);

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

        var dbReview = await _dbContext.Reviews
            .Where(s => isUserEmployee.IsEmployee
                ? s.Queue.EmployeeId == employeeId
                : s.CustomerId == customerId)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (dbReview == null)
        {
            _logger.LogWarning("Review with Id {id} not found.", request.Id);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(ReviewEntity));
        }

        var response = new ReviewResponseModel()
        {
            Id = dbReview.Id,
            CustomerId = dbReview.CustomerId,
            QueueId = dbReview.QueueId,
            EmployeeId = dbReview.Queue.EmployeeId,
            Grade = dbReview.Grade,
            ReviewText = dbReview.ReviewText
        };

        _logger.LogInformation("Review with Id {id} fetched successfully.", request.Id);
        return response;
    }
}