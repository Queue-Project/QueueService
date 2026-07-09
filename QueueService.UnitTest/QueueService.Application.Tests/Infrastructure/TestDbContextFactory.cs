using Microsoft.EntityFrameworkCore;
using QInfrastructure.Persistence.DataBase;

namespace QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;

public static class TestDbContextFactory
{
    public static QueueDbContext Create()
    {
        var options = new DbContextOptionsBuilder<QueueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new QueueDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}