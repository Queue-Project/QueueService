using QDomain.Enums;

namespace QApplication.Responses;

public class ComplaintResponseModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int QueueId { get; set; }
    public int EmployeeId { get; set; }
    public string ComplaintText { get; set; }
    public string? ResponseText { get; set; }
    public ComplaintStatus ComplaintStatus { get; set; }
}