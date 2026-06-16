using QDomain.Enums;

namespace QApplication.Responses;

public class AddQueueResponseModel
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public int EmployeeId { get; set; }
    public int CustomerId { get; set; }
    public int ServiceId { get; set; }
    
    public DateTimeOffset StartTime { get; set; }
    public QueueStatus Status { get; set; }
}