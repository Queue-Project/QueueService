using System.Net;
using MagicOnion;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Responses.CustomerResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetCustomerReviewsAsyncTests
{
    
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetCustomerReviewsAsyncTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }
    
     [Fact]
    public async Task Handler_Should_Return_Customer_Reviews_Successfully()
    {
        //Arrange
        var review = TestDataSeeder.CreateReview();
        await _dbContext.Reviews.AddAsync(review, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        

        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };


        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));


        //Act
        var result = await _queueService.GetCustomerReviewsAsync(1);


        //Assert
        result.ShouldNotBeNull();
        var firstReview = result.FirstOrDefault();
        firstReview!.Id.ShouldBe(firstReview.Id);
        firstReview.CustomerId.ShouldBe(review.CustomerId);
        firstReview.QueueId.ShouldBe(review.QueueId);
        firstReview.Grade.ShouldBe(review.Grade);

    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Customer_Not_Found()
    {
        //Arrange
        var review = TestDataSeeder.CreateReview();
        await _dbContext.Reviews.AddAsync(review, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = false,
            ErrorMessage = $"Customer with Id 1 not found"
        };


        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));
        

        //Act
        var result =  _queueService.GetCustomerReviewsAsync( 1);


        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(customerExpectedResponse.ErrorMessage);


    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Customer_Reviews_Is_Empty()
    {
        //Arrange
        
        
        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };


        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));
        

        //Act
        var result = await  _queueService.GetCustomerReviewsAsync( 1);


        //Assert
        result.ShouldBeEmpty();


    }
}