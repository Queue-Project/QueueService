using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Interfaces;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetCompanyReviewsAsync
{
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetCompanyReviewsAsync()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Company_Reviews_Successfully()
    {
        //Arrange
        var review = TestDataSeeder.CreateReview();
        await _dbContext.Reviews.AddAsync(review, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        //Act
        var result = await _queueService.GetCompanyReviewsAsync(1);


        //Assert
        result.ShouldNotBeNull();
        var firstReview = result.FirstOrDefault();
        firstReview!.Id.ShouldBe(firstReview.Id);
        firstReview.CustomerId.ShouldBe(review.CustomerId);
        firstReview.QueueId.ShouldBe(review.QueueId);
        firstReview.Grade.ShouldBe(review.Grade);
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Company_Reviews_Is_Empty()
    {
        //Arrange


        //Act
        var result = await _queueService.GetCompanyReviewsAsync(1);


        //Assert
        result.ShouldBeEmpty();
    }
}