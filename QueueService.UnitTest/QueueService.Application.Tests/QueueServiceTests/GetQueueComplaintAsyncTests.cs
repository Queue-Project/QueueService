using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetQueueComplaintAsyncTests
{
     private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetQueueComplaintAsyncTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Queue_Complaint_Successfully()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        //Act
        var result = await _queueService.GetQueueComplaintAsync(1);


        //Assert
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(complaint.CustomerId);
        result.EmployeeId.ShouldBe(1);
        result.Id.ShouldBe(complaint.Id);
        result.ComplaintText.ShouldBe(complaint.ComplaintText);
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Not_Found()
    {
        //Arrange
        


        //Act
        var result= _queueService.GetQueueComplaintAsync(1);



        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Queue with Id 1 not found");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Complaint_Not_Found()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        //Act
        var result= _queueService.GetQueueComplaintAsync(1);



        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Not found any complaint for this queue");
    }
}