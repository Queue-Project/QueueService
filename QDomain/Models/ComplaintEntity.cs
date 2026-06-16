using QDomain.Enums;

namespace QDomain.Models;

public class ComplaintEntity
{
    public int Id { get; set; }
    
    public string ComplaintText { get; set; }
    public string? ResponseText { get; set; }
    public ComplaintStatus ComplaintStatus { get; set; } = ComplaintStatus.Pending;

    public DateTime CreatdAt { get; set; }= DateTime.UtcNow;
    public int CustomerId { get; set; }
    
    public int QueueId { get; set; }
    public QueueEntity Queue { get; set; }
    
}