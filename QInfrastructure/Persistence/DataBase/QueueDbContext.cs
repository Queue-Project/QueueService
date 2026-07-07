using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using QApplication.Interfaces.Data;
using QDomain.Models;

namespace QInfrastructure.Persistence.DataBase;

public class QueueDbContext: DbContext, IQueueApplicationDbContext
{
    public QueueDbContext(DbContextOptions<QueueDbContext> options) : base(options)
    {
        
    }
    
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TableConfiguration.QueueTableConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    
    public DbSet<ComplaintEntity> Complaints { get; set; }
    public DbSet<QueueEntity> Queues { get; set; }
   
    public DbSet<ReviewEntity> Reviews { get; set; }
 
    public new EntityEntry Entry(object entity)
        => base.Entry(entity);
    
}