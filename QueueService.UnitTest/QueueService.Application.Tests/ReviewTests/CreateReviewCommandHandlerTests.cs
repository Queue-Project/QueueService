using System.Net;
using System.Security.Claims;
using MagicOnion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.UseCases.Reviews.Commands.CreateReview;
using QDomain.Enums;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Contracts.Responses.CustomerResponses;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.ReviewTests;

public class CreateReviewCommandHandlerTests
{
    private readonly Mock<ILogger<CreateReviewCommandHandler>> _mockLogger;
    private readonly QueueDbContext _dbContext;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IUserService> _mockUserService;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateReviewCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockUserService = new Mock<IUserService>();
        _handler = new CreateReviewCommandHandler(_mockLogger.Object, _dbContext, _mockAccessor.Object,
            _mockUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Create_Review()
    {
        //Arrange

        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        var command = new CreateReviewCommand(
            1,
            4,
            "Test Review Text"
        );

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);
        
        
        
        
        
        

        var currentCustomerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };

        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };

        
        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));

        
        

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.QueueId.ShouldBe(command.QueueId);
        result.CustomerId.ShouldBe(1);
        result.ReviewText.ShouldBe(command.ReviewText);
        result.Grade.ShouldBe(command.Grade);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        var command = new CreateReviewCommand(
            1,
            4,
            "Test Review Text"
        );
        
        var expectedResponse = new UnauthorizedAccessException("User not authenticated");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Throws(expectedResponse);
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User not authenticated");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Is_Not_Customer()
    {
        //Arrange

        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        
        var command = new CreateReviewCommand(
            1,
            4,
            "Test Review Text"
        );

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        var currentCustomerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = false,
            ErrorMessage = "User is not a customer"
        };
        
        
        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));
        
        
        

        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe("User is not a customer");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Not_Found()
    {
        //Arrange
        
        
        var command = new CreateReviewCommand(
            1,
            4,
            "Test Review Text"
        );

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        var currentCustomerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        
        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Queue with Id {command.QueueId} not found for this employee");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Customer_By_Id_Not_Found()
    {
        //Arrange
        
        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateReviewCommand(
            1,
            4,
            "Test Review Text"
        );

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        var currentCustomerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = false,
            ErrorMessage = "Customer not found"
        };

        
        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Customer with Id {customerExpectedResponse.Id} not found for adding new review.");
        
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Status_Is_Not_Completed()
    {
        //Arrange
        
        var queue = TestDataSeeder.CreateQueue();
        queue.Status = QueueStatus.Confirmed;
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateReviewCommand(
            1,
            4,
            "Test Review Text"
        );

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        var currentCustomerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };

        
        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        
        exception.Message.ShouldBe("You can leave review only if status is completed");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Review_Is_Double()
    {
        //Arrange
        
        var review = TestDataSeeder.CreateReview();
        await _dbContext.Reviews.AddAsync(review, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateReviewCommand(
            1,
            4,
            "Test Review Text"
        );

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        var currentCustomerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };

        
        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        
        exception.Message.ShouldBe("You have already left a review for this queue!");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Grade_Is_Not_Between_1_And_5()
    {
        //Arrange
        
        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var command = new CreateReviewCommand(
            1,
            6,
            "Test Review Text"
        );

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        var currentCustomerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        var customerExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };

        
        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));
        
        //Act
        var result =  _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        
        exception.Message.ShouldBe("Grade should be between 1 and 5!");
    }
}