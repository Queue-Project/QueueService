using MessagePack;

namespace QContracts.Responses;

[MessagePackObject]
public class BlockedCustomerInfo
{
    [Key(1)] public int BlockedId { get; set; }
    [Key(2)] public int CustomerId { get; set; }
    [Key(3)] public int CompanyId { get; set; }
    [Key(4)] public string? Reason { get; set; }
    [Key(5)] public DateTime BannedUntil { get; set; }
    [Key(6)] public bool DoesBanForever { get; set; }
    [Key(7)] public DateTime CreatedAt { get; set; }
}