using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Interfaces;
using QContracts.Events.Enums;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetCustomerActiveQueuesTests
{
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetCustomerActiveQueuesTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Return_Customer_Active_Queues_Successfully()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueues();
        await _dbContext.Queues.AddRangeAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

       



        //Act
        var result = await _queueService.GetCustomerActiveQueues(1);


        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        var firstQueue = result.FirstOrDefault();
        firstQueue!.CompanyId.ShouldBe(1);
        firstQueue.BranchId.ShouldBe(1);
        firstQueue.EmployeeId.ShouldBe(1);
        firstQueue.ServiceId.ShouldBe(1);
        firstQueue.QueueId.ShouldBe(1);
        firstQueue.Status.ShouldBe(UpdatedQueueStatus.Pending);
        
    }


    [Fact]
    public async Task Handler_Should_Return_Empty_List_When_Active_Queues_Is_Empty()
    {
        //Arrange

        


        //Act
        var result = await _queueService.GetCustomerActiveQueues(1);


        //Assert
        result.ShouldBeEmpty();
    }
}