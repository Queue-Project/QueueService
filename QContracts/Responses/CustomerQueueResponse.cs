using MessagePack;
using QContracts.Events.Enums;

namespace QContracts.Responses;

[MessagePackObject]
public class CustomerQueueResponse
{
    [Key(0)] public int QueueId { get; set; }

    [Key(1)] public int CompanyId { get; set; }

    [Key(2)] public int BranchId { get; set; }

    [Key(3)] public int ServiceId { get; set; }

    [Key(4)] public int EmployeeId { get; set; }

    [Key(5)] public DateTimeOffset StartTime { get; set; }

    [Key(6)] public UpdatedQueueStatus Status { get; set; }
}