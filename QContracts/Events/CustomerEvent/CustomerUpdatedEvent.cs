namespace QContracts.Events.CustomerEvent;

public class CustomerUpdatedEvent
{
    public DateTimeOffset OccuredAt { get; set; }
    public int CustomerId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
}