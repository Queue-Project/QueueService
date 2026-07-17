using MessagePack;
using QContracts.Events.Enums;

namespace QContracts.Responses;

[MessagePackObject]
public class QueueTrackingDataResponse
{
    [Key(0)] public int QueueId { get; set; }

    [Key(1)] public int EmployeeId { get; set; }
    
    [Key(2)] public int CompanyServiceId { get; set; }

    [Key(3)] public DateTimeOffset StartTime { get; set; }

    [Key(4)] public UpdatedQueueStatus Status { get; set; }

    [Key(5)] public List<QueueTrackingItemResponse> QueuesAhead { get; set; } = [];
}