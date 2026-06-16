using MessagePack;

namespace QContracts.Responses;

[MessagePackObject]
public class EmployeeInfo
{
    [Key(1)] public int CompanyId { get; set; }
    [Key(2)] public int? BranchId { get; set; }
    [Key(3)] public int? CompanyServiceId { get; set; }
    [Key(4)] public int EmployeeId { get; set; }
    [Key(5)] public string FirstName { get; set; }
    [Key(6)] public string LastName { get; set; }
    [Key(7)] public string Position { get; set; }
    [Key(8)] public DateTime CreatedAt { get; set; }
}