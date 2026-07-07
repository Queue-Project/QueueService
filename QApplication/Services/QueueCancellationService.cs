using System.Net;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QApplication.Exceptions;
using QApplication.Helpers;
using QApplication.Interfaces;
using QApplication.Interfaces.Data;
using QApplication.Responses;
using QContracts.Events;
using QContracts.Events.Enums;
using QDomain.Enums;
using QDomain.Models;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.Services;

public class QueueCancellationService : IQueueCancellationService
{
    private readonly ILogger<QueueCancellationService> _logger;
    private readonly IQueueApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserService _userService;

    public QueueCancellationService(ILogger<QueueCancellationService> logger, IQueueApplicationDbContext dbContext,
        IPublishEndpoint publishEndpoint, IUserService userService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _userService = userService;
    }


    public async Task<QueueEntity> GetAndValidateQueueForCancellation(int queueId, CancellationToken cancellationToken)
    {
        var dbQueue = await _dbContext.Queues.FirstOrDefaultAsync(s => s.Id == queueId, cancellationToken);
        if (dbQueue == null)
        {
            _logger.LogWarning("Queue with Id {QueueId} not found for customer cancellation", queueId);
            throw new HttpStatusCodeException(HttpStatusCode.NotFound, nameof(QueueEntity));
        }

        return dbQueue;
    }

    public async Task<QueueResponseModel> ProcessCancellation(QueueEntity queue, QueueStatus newStatus,
        string? cancelReason, UpdatedQueueStatus eventStatus,
        CancellationToken cancellationToken)
    {
        var userCustomer = await _userService.GetUserByCustomerId(new GetUserByCustomerIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = queue.CustomerId
        });

        if (!userCustomer.IsValid)
        {
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"Customer with Id {queue.CustomerId} not found");
        }


        var userEmployee = await _userService.GetUserByEmployeeId(new GetUserByEmployeeIdRequest
        {
            RequestId = Guid.NewGuid(),
            EmployeeId = queue.EmployeeId
        });

        if (!userEmployee.IsValid)
        {
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"Employee with Id {queue.EmployeeId} not found");
        }


        queue.Status = newStatus;
        queue.CancelReason = cancelReason;
        _logger.LogDebug("Saving cancellation changes to db");

        await _dbContext.SaveChangesAsync(cancellationToken);


        var entry = _dbContext.Entry(queue);
        var changes = AuditHelper.GetChanges(entry);

        var userEmail = "Unknown";
        int userId = 0;
        if (eventStatus == UpdatedQueueStatus.CanceledByCustomer)
        {
            userId = queue.CustomerId;
            userEmail = userCustomer.EmailAddress;
        }
        else if (eventStatus == UpdatedQueueStatus.CanceledByEmployee)
        {
            userId = queue.EmployeeId;
            userEmail = userEmployee.EmailAddress;
        }


        await _publishEndpoint.Publish(new QueueEvent
        {
            Email = userEmail,
            CompanyId = queue.CompanyId,
            QueueId = queue.Id,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            StartTime = queue.StartTime,
            EndTime = queue.EndTime,
            EventType = QueueEventType.Updated,
            Status = eventStatus,
            CancelReason = cancelReason,
            AuditData = new AuditData
            {
                PerformedByUserId = userId,
                PerformedByUserName = userEmail,
                Changes = changes
            }
        }, cancellationToken);

        return new QueueResponseModel
        {
            Id = queue.Id,
            CompanyId = queue.CompanyId,
            BranchId = queue.BranchId,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            ServiceId = queue.ServiceId,
            StartTime = queue.StartTime,
            Status = queue.Status
        };
    }
}