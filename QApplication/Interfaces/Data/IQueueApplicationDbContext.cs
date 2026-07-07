using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QDomain.Models;

namespace QApplication.Interfaces.Data;

public interface IQueueApplicationDbContext
{ 
   
    DbSet<ComplaintEntity> Complaints { get; set; }
    DbSet<QueueEntity> Queues { get; set; }
  
    DbSet<ReviewEntity> Reviews { get; set; }


    EntityEntry Entry(object entry);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}