using MessagePack;

namespace QContracts.Responses;

[MessagePackObject]
public class CustomerInfo
{
    [Key(1)] public int CustomerId { get; set; }
    [Key(2)] public string FirstName { get; set; }
    [Key(3)] public string LastName { get; set; }
    [Key(4)] public DateTime CreatedAt { get; set; }
}