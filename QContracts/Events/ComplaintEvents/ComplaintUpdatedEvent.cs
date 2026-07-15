using QContracts.Enums;

namespace QContracts.Events.ComplaintEvents;

public class ComplaintUpdatedEvent
{
    public DateTime OccuredAt { get; set; }
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public int ServiceId { get; set; }
    public int ComplaintId { get; set; }
    public int CustomerId { get; set; }
    public int EmployeeId { get; set; }
    public int QueueId { get; set; }
    public string ComplaintText { get; set; }
    public string ResponseText { get; set; }
    public CurrentComplaintStatus CurrentComplaintStatus { get; set; }
    public AuditData? AuditData { get; set; }
}