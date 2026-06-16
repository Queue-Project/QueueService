namespace QContracts.CashingEvents;

public class CompanyCacheResetEvent
{
    public DateTimeOffset OccuredAt { get; set; }
    public int CompanyId { get; set; }
    
}