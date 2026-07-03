using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QInfrastructure.Persistence.DataBase;

public class QueueContextFactory:IDesignTimeDbContextFactory<QueueDbContext>
{
    public QueueDbContext CreateDbContext(string[] args)
    {
        var optionBuilder = new DbContextOptionsBuilder<QueueDbContext>();
        
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../QueueService.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionBuilder.UseNpgsql(connectionString);
        return new QueueDbContext(optionBuilder.Options);
    }
}