using MessagePack;

namespace QContracts.Responses;

[MessagePackObject]
public class QueueTrackingItemResponse
{
    [Key(0)] public int QueueId { get; set; }

    [Key(1)] public int CompanyServiceId { get; set; }

    [Key(2)] public DateTimeOffset StartTime { get; set; }
}