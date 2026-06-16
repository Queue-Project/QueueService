namespace QContracts.Events.BlockedCustomerEvent;

public class BlockedCustomerDeletedEvent
{
    public DateTimeOffset OccuredAt { get; set; }
    public int CompanyId { get; set; }
    public int CustomerId { get; set; }
    public int BlockedCustomerId { get; set; }
    public string? Reason { get; set; }
    public DateTime BannedUntil { get; set; }
    public bool DoesBanForever { get; set; }
}