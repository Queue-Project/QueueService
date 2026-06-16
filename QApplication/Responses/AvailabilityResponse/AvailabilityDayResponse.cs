namespace QApplication.Responses.AvailabilityResponse;

public class AvailabilityDayResponse
{
    public DateTimeOffset Date { get; set; }
    public List<TimelineBlockResponse> Timeline { get; set; } = [];

}