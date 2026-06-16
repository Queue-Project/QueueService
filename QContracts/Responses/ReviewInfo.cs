using MessagePack;

namespace QContracts.Responses;

[MessagePackObject]
public class ReviewInfo
{
    [Key(0)] public int Id { get; set; }
    [Key(1)] public int QueueId { get; set; }  
    [Key(2)] public int EmployeeId { get; set; }
    [Key(3)] public int CustomerId { get; set; }
    [Key(4)] public int Grade { get; set; }
    [Key(5)] public string? ReviewText { get; set; }
    [Key(6)] public DateTime CreatedAt { get; set; }
}