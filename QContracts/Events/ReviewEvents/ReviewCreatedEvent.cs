using System.Security.AccessControl;

namespace QContracts.Events.ReviewEvents;

public class ReviewCreatedEvent
{
    public DateTime OccuredAt { get; set; }
    public int ReviewId { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public int QueueId { get; set; }
    public int Grade { get; set; }
    public string ReviewText { get; set; }
    public AuditData AuditData { get; set; }
    
}