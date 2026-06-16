using MessagePack;
using QContracts.Enums;

namespace QContracts.Responses;

[MessagePackObject]
public class QueueInfo
{
    [Key(0)] public int Id { get; set; }

    [Key(1)] public int CompanyId { get; set; }

    [Key(2)] public int BranchId { get; set; }

    [Key(3)] public int ServiceId { get; set; }
    [Key(6)] public int EmployeeId { get; set; }
    
    [Key(4)] public int CustomerId { get; set; }
    [Key(7)] public string EmployeeName { get; set; }

    [Key(5)] public string CustomerName { get; set; }
    

    [Key(8)] public DateTimeOffset StartTime { get; set; }

    [Key(9)] public DateTimeOffset? EndTime { get; set; }

    [Key(10)] public CurrentQueueStatus CurrentQueueStatus { get; set; }

    [Key(11)] public string? CancelReason { get; set; }

    [Key(12)] public DateTime CreatedAt { get; set; }

}