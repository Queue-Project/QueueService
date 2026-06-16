using System.Net;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Exceptions;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QDomain.Enums;
using QDomain.Models;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.UseCases.Complaints.Commands.UpdateComplaintStatus;

public class UpdateComplaintStatusCommandHandler: IRequestHandler<UpdateComplaintStatusCommand, ComplaintResponseModel>
{
    private readonly ILogger<UpdateComplaintStatusCommandHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public UpdateComplaintStatusCommandHandler(ILogger<UpdateComplaintStatusCommandHandler> logger, IQueueApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<ComplaintResponseModel> Handle(UpdateComplaintStatusCommand request, CancellationToken cancellationToken)
    {
         _logger.LogInformation("Updating complaint status with Id {id}", request.Id);

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
         
         var dbComplaint = await _dbContext.Complaints
             .Where(s=>s.Queue.CompanyId== companyId || s.Queue.EmployeeId== currentEmployee.EmployeeId)
             .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (dbComplaint == null)
        {
            _logger.LogWarning("Complaint with Id {id} not found for updating status.", request.Id);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(ComplaintEntity));
        }

        switch (request.ComplaintStatus)
        {
            case ComplaintStatus.Pending:
                if (dbComplaint.ComplaintStatus != ComplaintStatus.Resolved)
                {
                    _logger.LogError("Invalid Pending status updating for this complaint Id {id}", request.Id);
                    throw new Exception("Pending status can only be Reviewed");
                }

                if (dbComplaint.ComplaintStatus == ComplaintStatus.Resolved)
                {
                    _logger.LogError("Trying to update already finalized status for this complaint Id {id}", request.Id);
                    throw new Exception("This complaint is already finalized and cannot be updated!");
                }

                break;
            case ComplaintStatus.Reviewed:
                if (dbComplaint.ComplaintStatus != ComplaintStatus.Pending)
                {
                    _logger.LogError("Invalid Reviewed status updating for this complaint Id {id}", request.Id);
                    throw new Exception("Reviewed status can only be Resolved");
                }

                break;
            case ComplaintStatus.Resolved:
                if (dbComplaint.ComplaintStatus != ComplaintStatus.Reviewed)
                {
                    _logger.LogError("Invalid Resolved status updating for this complaint Id {id}", request.Id);
                    throw new Exception("Resolved can be when status is Reviewed");
                }

                break;
            default:
                _logger.LogError("Invalid status choice.");
                throw new Exception("There is not this kind of status");
        }

        dbComplaint.ComplaintStatus = request.ComplaintStatus;
        dbComplaint.ResponseText = request.ResponseText;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Complaint status with Id {id} updated successfully.", request.Id);
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

        return response;
    }
}