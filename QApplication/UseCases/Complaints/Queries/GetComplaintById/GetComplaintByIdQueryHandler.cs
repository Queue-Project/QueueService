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

namespace QApplication.UseCases.Complaints.Queries.GetComplaintById;

public class GetComplaintByIdQueryHandler: IRequestHandler<GetComplaintByIdQuery, ComplaintResponseModel>
{
    private readonly ILogger<GetComplaintByIdQueryHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public GetComplaintByIdQueryHandler(ILogger<GetComplaintByIdQueryHandler> logger, IQueueApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<ComplaintResponseModel> Handle(GetComplaintByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting complaint by Id {id}", request.Id);

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
        
        var dbComplaint = await _dbContext.Complaints
            .Where(s=>isUserEmployee.IsEmployee
            ? s.Queue.EmployeeId== employeeId
            : s.CustomerId== customerId)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (dbComplaint == null)
        {
            _logger.LogWarning("Complaint with Id {id} not found.", request.Id);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(ComplaintEntity));
        }

        var response = new ComplaintResponseModel
        {
            Id = dbComplaint.Id,
            CustomerId = dbComplaint.CustomerId,
            QueueId = dbComplaint.QueueId,
            EmployeeId = dbComplaint.Queue.EmployeeId,
            ComplaintText = dbComplaint.ComplaintText,
            ResponseText = dbComplaint.ResponseText,
            ComplaintStatus = dbComplaint.ComplaintStatus
        };

        _logger.LogInformation("Complaint by Id {id} fetched successfully.", request.Id);
        return response;
    }
}