using QApplication.Responses;
using QContracts.QueueEvents.Enums;
using QDomain.Enums;
using QDomain.Models;

namespace QApplication.Interfaces;

public interface IQueueCancellationService
{
    Task<QueueEntity> GetAndValidateQueueForCancellation(int queueId, CancellationToken cancellationToken);
    Task<QueueResponseModel> ProcessCancellation(QueueEntity queue, QueueStatus newStatus, 
        string? cancelReason, UpdatedQueueStatus eventStatus, CancellationToken cancellationToken);
}