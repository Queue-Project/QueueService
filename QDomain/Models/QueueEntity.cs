using QDomain.Enums;

namespace QDomain.Models;

public class QueueEntity
{
    public int Id { get; set; }
    
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }
    

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }
    public string? CancelReason { get; set; }

    public QueueStatus Status { get; set; } = QueueStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsStartingSoonNotified { get; set; } = false;

    public int EmployeeId { get; set; }

    public int CustomerId { get; set; }

    
}