namespace QDomain.Models;

public class ReviewEntity   
{
    public int Id { get; set; }
    
    public int Grade { get; set; }
    public string? ReviewText { get; set; }
    
    public int QueueId { get; set; }
    public QueueEntity Queue { get; set; }

    public int CustomerId { get; set; }

    public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
    
}