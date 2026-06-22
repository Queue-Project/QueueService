using System.Net;
using System.Security.Claims;
using MagicOnion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.UseCases.Reviews.Queries.GetReviewById;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.ReviewTests;

public class GetReviewByIdQueryHandlerTests
{
    private readonly Mock<ILogger<GetReviewByIdQueryHandler>> _mockLogger;
    private readonly QueueDbContext _dbContext;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IUserService> _mockUserService;
    private readonly GetReviewByIdQueryHandler _handler;

    public GetReviewByIdQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetReviewByIdQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockUserService = new Mock<IUserService>();
        _handler = new GetReviewByIdQueryHandler(_mockLogger.Object, _dbContext, _mockAccessor.Object,
            _mockUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Review_When_User_Is_Customer_Successfully()
    {
        //Arrange
        var review = TestDataSeeder.CreateReview();
        await _dbContext.Reviews.AddAsync(review, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);


        var isEmployeeExpectedResponse = new IsEmployeeResponse
        {
            IsEmployee = false
        };
        
        
        var customerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        _mockUserService.Setup(s => s.IsCurrentUserEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(isEmployeeExpectedResponse));

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));

        var query = new GetReviewByIdQuery(1);


        //Act
        var result = await _handler.Handle(query, CancellationToken.None);


        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(result.Id);
        result.CustomerId.ShouldBe(review.CustomerId);
        result.QueueId.ShouldBe(review.QueueId);
        result.Grade.ShouldBe(review.Grade);
        result.ReviewText.ShouldBe(review.ReviewText);
    }
    
    [Fact]
    public async Task Handler_Should_Return_Review_When_User_Is_Employee_Successfully()
    {
        //Arrange
        var review = TestDataSeeder.CreateReview();
        await _dbContext.Reviews.AddAsync(review, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);


        var isEmployeeExpectedResponse = new IsEmployeeResponse
        {
            IsEmployee = true
        };
        
        
        var employeeExpectedResponse = new CurrentEmployeeResponse
        {
            EmployeeId = 1,
            CompanyId = 1,
            BranchId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        _mockUserService.Setup(s => s.IsCurrentUserEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(isEmployeeExpectedResponse));

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var query = new GetReviewByIdQuery(1);


        //Act
        var result = await _handler.Handle(query, CancellationToken.None);


        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(result.Id);
        result.CustomerId.ShouldBe(review.CustomerId);
        result.QueueId.ShouldBe(review.QueueId);
        result.Grade.ShouldBe(review.Grade);
        result.ReviewText.ShouldBe(review.ReviewText);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        var query = new GetReviewByIdQuery(1);
        
        var expectedResponse = new UnauthorizedAccessException("User not authenticated");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Throws(expectedResponse);
        
        //Act
        var result =  _handler.Handle(query, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User not authenticated");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Review_Not_Found()
    {
        //Arrange

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);


        var isEmployeeExpectedResponse = new IsEmployeeResponse
        {
            IsEmployee = true
        };
        
        
        var employeeExpectedResponse = new CurrentEmployeeResponse
        {
            EmployeeId = 1,
            CompanyId = 1,
            BranchId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };
        
        _mockUserService.Setup(s => s.IsCurrentUserEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(isEmployeeExpectedResponse));

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var query = new GetReviewByIdQuery(1);


        //Act
        var result =  _handler.Handle(query, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("ReviewEntity");
    }

}