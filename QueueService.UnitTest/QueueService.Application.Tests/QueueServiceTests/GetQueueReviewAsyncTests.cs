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

public class GetQueueReviewAsyncTests
{
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetQueueReviewAsyncTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Queue_Review_Successfully()
    {
        //Arrange
        var review = TestDataSeeder.CreateReview();
        await _dbContext.Reviews.AddAsync(review, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        //Act
        var result = await _queueService.GetQueueReviewAsync(1);


        //Assert
        result.ShouldNotBeNull();
        result.CustomerId.ShouldBe(review.CustomerId);
        result.EmployeeId.ShouldBe(1);
        result.Id.ShouldBe(review.Id);
        result.Grade.ShouldBe(review.Grade);
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Not_Found()
    {
        //Arrange
        


        //Act
        var result= _queueService.GetQueueReviewAsync(1);



        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Queue with Id 1 not found");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Review_Not_Found()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        //Act
        var result= _queueService.GetQueueReviewAsync(1);



        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Not found review for this request");
    }
}