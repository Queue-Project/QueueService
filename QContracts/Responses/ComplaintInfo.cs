using MessagePack;
using QContracts.Enums;

namespace QContracts.Responses;

[MessagePackObject]
public class ComplaintInfo
{
    [Key(0)] public int Id { get; set; }
    [Key(1)] public int QueueId { get; set; }  
    [Key(2)] public int EmployeeId { get; set; }
    [Key(3)] public int CustomerId { get; set; }
    [Key(4)] public string ComplaintText { get; set; }
    [Key(5)] public string? ResponseText { get; set; }
    [Key(6)] public CurrentComplaintStatus Status { get; set; }
    [Key(7)] public DateTime CreatedAt { get; set; }
}