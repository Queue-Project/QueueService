using QContracts.QueueEvents.Enums;

namespace QContracts.Events;

public class QueueEvent
{
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public string Email { get; set; }
    public int CompanyId { get; set; }
    public int QueueId { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public QueueEventType EventType { get; set; }
    public UpdatedQueueStatus? Status { get; set; } 
    public string? CancelReason { get; set; }   
}