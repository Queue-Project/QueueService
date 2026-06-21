using System.Collections.Frozen;
using MassTransit;
using Microsoft.Extensions.Logging;
using QApplication.Caching;
using QApplication.Responses.AvailabilityResponse;
using QContracts.Events;
using QContracts.Events.Enums;
using QInfrastructure.Extensions;
using QNotificationService.Contracts.NotificationEvents;

namespace QInfrastructure.Consumers.QueueConsumers;

public class QueueEventConsumer : IConsumer<QueueEvent>
{
    private readonly ILogger<QueueEventConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICacheService _cacheService;

    private static readonly FrozenDictionary<UpdatedQueueStatus, string> StatusMessage =
        new Dictionary<UpdatedQueueStatus, string>()
        {
            [UpdatedQueueStatus.CanceledByCustomer] =
                "Your queue with Employee {0} was canceled by you. Reason: {1}.. ",
            [UpdatedQueueStatus.CanceledByEmployee] =
                "Your queue with Employee {0} was canceled by employee. Reason: {1}.. ",
            [UpdatedQueueStatus.CanceledByAdmin] = "Your queue with Employee {0} was canceled by admin. Reason: {1}.. ",
            [UpdatedQueueStatus.Completed] = "Your queue with Employee {0} is now completed. ",
            [UpdatedQueueStatus.Confirmed] = "Your queue with Employee {0} has been confirmed for {1}. "
        }.ToFrozenDictionary();

    public QueueEventConsumer(ILogger<QueueEventConsumer> logger, IPublishEndpoint publishEndpoint,
        ICacheService cacheService)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _cacheService = cacheService;
    }

    public async Task Consume(ConsumeContext<QueueEvent> context)
    {
        var evt = context.Message;
        switch (evt.EventType)
        {
            case QueueEventType.Created:
                await HandleCreatedEvent(evt);
                break;

            case QueueEventType.StartingSoon:

                await HandleStartingSoonEvent(evt);

                break;

            case QueueEventType.Updated:
                await HandleUpdatedEvent(evt);
                break;
        }
    }

    private async Task HandleCreatedEvent(QueueEvent evt)
    {
        _logger.LogInformation("Processing cache reset for QueueId {QueueId}", evt.QueueId);

        
        var date = evt.StartTime.Date;

        await _cacheService.AddQueueToSchedule(evt.EmployeeId, date, evt.QueueId,
            new TimeIntervalResponse
            {
                Start = evt.StartTime,
                End = evt.EndTime ?? evt.StartTime.AddMinutes(30)
            });

        
        
        var cacheRest =
            _cacheService.ResetCacheAsync(evt.QueueId, evt.CustomerId, evt.EmployeeId);

        

        _logger.LogInformation("Publishing notification event for QueueId {QueueId}", evt.QueueId);

        var notification = _publishEndpoint.Publish(new SendNotificationEvent()
        {
            UserId = evt.CustomerId,
            Email = evt.Email,
            Message =
                $"You have successfully booked a queue with Employee {evt.EmployeeId} at {evt.StartTime}. "
        });

        await Task.WhenAll(cacheRest, notification);

        _logger.LogInformation("Cache reset processed for QueueId {QueueId}", evt.QueueId);

        _logger.LogInformation("Published notification event for QueueId {QueueId}", evt.QueueId);
    }

    private async Task HandleStartingSoonEvent(QueueEvent evt)
    {
        _logger.LogInformation("Publishing notification event for QueueId {QueueId}", evt.QueueId);

        await _publishEndpoint.Publish(new SendNotificationEvent
        {
            UserId = evt.CustomerId,
            Email = evt.Email,
            Message = $"Reminder: your queue with Employee {evt.EmployeeId} starts in 5 minutes."
        });

        _logger.LogInformation("Published notification event for QueueId {QueueId}", evt.QueueId);
    }

    private async Task HandleUpdatedEvent(QueueEvent evt)
    {
        _logger.LogInformation("Processing cache reset for QueueId {QueueId}", evt.QueueId);

        
        var date = evt.StartTime.Date;
        if (evt.Status== UpdatedQueueStatus.CanceledByCustomer
            || evt.Status== UpdatedQueueStatus.CanceledByEmployee
            || evt.Status == UpdatedQueueStatus.CanceledByAdmin)
        {
            await _cacheService.RemoveQueueFromSchedule(evt.EmployeeId, date, evt.QueueId);
        }

        if (evt.Status== UpdatedQueueStatus.Confirmed)
        {
            await _cacheService.RemoveQueueFromSchedule(evt.EmployeeId, date, evt.QueueId);
            await _cacheService.AddQueueToSchedule(evt.EmployeeId, date,evt.QueueId ,new TimeIntervalResponse
            {
                Start = evt.StartTime,
                End = evt.EndTime ?? evt.StartTime.AddMinutes(30)
            });
        }
        
        
        
        var cacheReset = _cacheService.ResetCacheAsync(evt.QueueId, evt.CustomerId, evt.EmployeeId);
        

        
        
        Task? notificationTask = null;

        if (evt.Status.HasValue && StatusMessage.TryGetValue(evt.Status.Value, out var template))
        {
            var message = string.Format(
                template,
                evt.EmployeeId,
                evt.CancelReason ?? evt.StartTime.ToString("yyyy-MM-dd HH:mm"));

            notificationTask = _publishEndpoint.Publish(new SendNotificationEvent
            {
                UserId = evt.CustomerId,
                Email = evt.Email,
                Message = message
            });
        }

        if (notificationTask != null)
        {
            await Task.WhenAll(cacheReset, notificationTask);
            _logger.LogInformation("Published notification for status {Status}", evt.Status);
        }
        else
        {
            await cacheReset;
            _logger.LogDebug("No notification sent for status {Status}", evt.Status);
        }

        _logger.LogInformation("Successfully processed Updated event for QueueId {QueueId}", evt.QueueId);
    }
    
   
}