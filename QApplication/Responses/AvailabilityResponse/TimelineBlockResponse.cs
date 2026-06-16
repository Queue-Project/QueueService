using QDomain.Enums;

namespace QApplication.Responses.AvailabilityResponse;

public class TimelineBlockResponse
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public SlotType Type { get; set; }

}