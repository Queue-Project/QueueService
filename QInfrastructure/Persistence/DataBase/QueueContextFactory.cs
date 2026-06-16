using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QInfrastructure.Persistence.DataBase;

public class QueueContextFactory:IDesignTimeDbContextFactory<QueueDbContext>
{
    public QueueDbContext CreateDbContext(string[] args)
    {
        var optionBuilder = new DbContextOptionsBuilder<QueueDbContext>();
        optionBuilder.UseNpgsql("Host=localhost;Port=5432;Database=QueueService2;Username=postgres;Password=b.sh.3242");
        return new QueueDbContext(optionBuilder.Options);
    }
}