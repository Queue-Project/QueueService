using QContracts.Events;
using QDomain.Enums;
using QDomain.Models;

namespace QApplication.Interfaces;

public interface IPublishQueueUpdatedEvent
{
    Task<QueueEvent> CreateQueueUpdatedEvent(QueueEntity dbQueue, QueueStatus newStatus);
}