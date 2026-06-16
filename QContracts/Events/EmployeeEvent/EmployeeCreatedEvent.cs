namespace QContracts.Events.EmployeeEvent;

public class EmployeeCreatedEvent
{
    public DateTimeOffset OccurredAt { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int EmployeeId { get; set; }
    public int? ServiceId { get; set; }
    public string  FirstName { get; set; }
    public string  LastName { get; set; }
    public string  Position { get; set; }
    public string PhoneNumber { get; set; }
}