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

namespace QApplication.UseCases.Complaints.Commands.CreateComplaint;

public class CreateComplaintCommandHandler: IRequestHandler<CreateComplaintCommand, ComplaintResponseModel>
{
    private readonly ILogger<CreateComplaintCommandHandler> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IUserService _userService;

    public CreateComplaintCommandHandler(ILogger<CreateComplaintCommandHandler> logger, IQueueApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _contextAccessor = contextAccessor;
        _userService = userService;
    }

    public async Task<ComplaintResponseModel> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding new complaint to this queue Id {queueId}", request.QueueId);

        
        var userIdClaim = _contextAccessor.HttpContext!.User.FindFirst("id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }
        
        var currentCustomer = await _userService.GetCurrentCustomer(new CurrentUserRequest
        {
            UserId = userId,
            RequestId = Guid.NewGuid()
        });

        if (!currentCustomer.IsValid)
        {
            _logger.LogWarning("Customer not found for UserId: {UserId}", userId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, currentCustomer.ErrorMessage ?? "Customer not found");
        }
        
        _logger.LogDebug("Current customer retrieved: CustomerId {CustomerId}", currentCustomer.CustomerId);
        

        var queueId = await _dbContext.Queues
            .Where(s=>s.CustomerId== currentCustomer.CustomerId)
            .FirstOrDefaultAsync(s => s.Id == request.QueueId, cancellationToken);
        if (queueId == null)
        {
            _logger.LogWarning("Queue with Id {queueId} not found for adding new complaint,", request.QueueId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(QueueEntity));
        }


        if (queueId.Status != QueueStatus.Completed && queueId.Status != QueueStatus.CanceledByAdmin &&
            queueId.Status != QueueStatus.CancelledByEmployee)
        {
            _logger.LogError("Invalid queue status while adding new complaint for this queue Id {queueId}",
                request.QueueId);
            throw new Exception("You can leave complaint when status is Completed or CanceledByAdmin/ByEmployee");
        }

        var complaints = await _dbContext.Complaints.Where(s => s.QueueId == request.QueueId)
            .ToListAsync(cancellationToken);
        var isDouble = complaints.Any(s => s.CustomerId == currentCustomer.CustomerId);
        if (isDouble)
        {
            _logger.LogError("Overlapping complaint for this queue Id {queueId}", request.QueueId);
            throw new Exception("You have already left a complaint for this queue!");
        }

        var complaint = new ComplaintEntity
        {
            CustomerId = currentCustomer.CustomerId,
            QueueId = request.QueueId,
            ComplaintText = request.ComplaintText,
            ComplaintStatus = ComplaintStatus.Pending,
            CreatdAt = DateTime.UtcNow
        };

        await _dbContext.Complaints.AddAsync(complaint, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Complaint added successfully with Id {id}.", complaint.Id);
        var response = new ComplaintResponseModel
        {
            Id = complaint.Id,
            CustomerId = complaint.CustomerId,
            QueueId = complaint.QueueId,
            EmployeeId = complaint.Queue.EmployeeId,
            ComplaintText = complaint.ComplaintText,
            ComplaintStatus = complaint.ComplaintStatus
        };

        return response;
    }
}