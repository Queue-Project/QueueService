using Microsoft.EntityFrameworkCore;
using QDomain.Models;

namespace QApplication.Interfaces.Data;

public interface IQueueApplicationDbContext
{ 
   
    DbSet<ComplaintEntity> Complaints { get; set; }
    DbSet<QueueEntity> Queues { get; set; }
  
    DbSet<ReviewEntity> Reviews { get; set; }
   
    

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}