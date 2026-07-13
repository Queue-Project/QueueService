using MessagePack;

namespace QContracts.Requests;

[MessagePackObject]
public class EmployeeQueuesByDateRequest
{
    [Key(1)] public int EmployeeId { get; set; }
    [Key(2)] public DateOnly Date { get; set; }
}