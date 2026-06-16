using QDomain.Enums;

namespace QApplication.Requests.ComplaintRequest;

public class UpdateComplaintStatusRequest
{
    public ComplaintStatus ComplaintStatus { get; set; }
    public string? ResponseText { get; set; }
}