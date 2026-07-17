using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QContracts.Events.Enums;
using QDomain.Enums;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetQueueTrackingDataTests
{
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetQueueTrackingDataTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Return_Queue_Tracking_Data_When_Queue_Is_Exists_Successfully()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueue();
        queue.Status = QueueStatus.Pending;
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

       



        //Act
        var result = await _queueService.GetQueueTrackingData(queue.Id);


        //Assert
        result.ShouldNotBeNull();
        result.EmployeeId.ShouldBe(1);
        result.QueueId.ShouldBe(1);
        result.Status.ShouldBe(UpdatedQueueStatus.Pending);
        result.CompanyServiceId.ShouldBe(1);
        
        
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Is_Not_Exists()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueue();
        queue.Status = QueueStatus.Completed;
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        


        //Act
        var result =  _queueService.GetQueueTrackingData(queue.Id);


        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Queue with Id {queue.Id} not found");
       
        
    }
}