using System.Net;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QContracts.Events;
using QContracts.Events.Enums;
using QDomain.Enums;
using QDomain.Models;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;

namespace QApplication.Services;

public class PublishQueueUpdatedEvent: IPublishQueueUpdatedEvent
{
    private readonly IUserService _userService;

    public PublishQueueUpdatedEvent(IUserService userService)
    {
        _userService = userService;
    }
    public async Task<QueueEvent> CreateQueueUpdatedEvent(QueueEntity dbQueue, QueueStatus newStatus)
    {
        var user = await _userService.GetUserByCustomerId(new GetUserByCustomerIdRequest
        {
            RequestId = Guid.NewGuid(),
            CustomerId = dbQueue.CustomerId
        });

        if (!user.IsValid)
        {
            throw new HttpStatusCodeException(HttpStatusCode.NotFound,
                $"Customer with Id {dbQueue.CustomerId} not found");
        }
        
        var userEmail = user.EmailAddress;
        
        return new QueueEvent
        {
            Email = userEmail,
            CompanyId = dbQueue.CompanyId,
            QueueId = dbQueue.Id,
            CustomerId = dbQueue.CustomerId,
            EmployeeId = dbQueue.EmployeeId,
            StartTime = dbQueue.StartTime,
            EndTime = dbQueue.EndTime,
            EventType = QueueEventType.Updated,
            CancelReason = dbQueue.CancelReason,
            Status = newStatus == QueueStatus.Confirmed
                ? UpdatedQueueStatus.Confirmed
                : UpdatedQueueStatus.Completed
        };
    }
}