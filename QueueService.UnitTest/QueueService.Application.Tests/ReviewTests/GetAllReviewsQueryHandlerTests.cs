using System.Security.Claims;
using MagicOnion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.UseCases.Reviews.Queries.GetAllReviews;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.ReviewTests;

public class GetAllReviewsQueryHandlerTests
{
    private readonly Mock<ILogger<GetAllReviewsQueryHandler>> _mockLogger;
    private readonly QueueDbContext _dbContext;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IUserService> _mockUserService;
    private readonly GetAllReviewsQueryHandler _handler;

    public GetAllReviewsQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetAllReviewsQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockUserService = new Mock<IUserService>();
        _handler = new GetAllReviewsQueryHandler(_mockLogger.Object, _dbContext, _mockAccessor.Object,
            _mockUserService.Object);
    }
    
    [Fact]
    public async Task Handler_Should_Return_AllReviews_When_User_Is_Customer_Successfully()
    {
        //Arrange
        var review = TestDataSeeder.CreateReviews();
        await _dbContext.Reviews.AddRangeAsync(review, CancellationToken.None);
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

        var query = new GetAllReviewsQuery(1);


        //Act
        var result = await _handler.Handle(query, CancellationToken.None);


        //Assert
        result.ShouldNotBeNull();
        result.HasNextPage.ShouldBe(false);
        result.TotalPages.ShouldBe(1);
        result.TotalCount.ShouldBe(2);

        var firstReview = result.Items.FirstOrDefault();
        firstReview!.Id.ShouldBe(1);
    }
    
    [Fact]
    public async Task Handler_Should_Return_AllReviews_When_User_Is_Employee_Successfully()
    {
        //Arrange
        var review = TestDataSeeder.CreateReviews();
        await _dbContext.Reviews.AddRangeAsync(review, CancellationToken.None);
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

        var query = new GetAllReviewsQuery(1);


        //Act
        var result = await _handler.Handle(query, CancellationToken.None);


        //Assert
        result.ShouldNotBeNull();
        result.HasNextPage.ShouldBe(false);
        result.TotalPages.ShouldBe(1);
        result.TotalCount.ShouldBe(2);

        var firstReview = result.Items.FirstOrDefault();
        firstReview!.Id.ShouldBe(1);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        var query = new GetAllReviewsQuery(1);
        
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
    public async Task Should_Return_ReviewEmptyList()
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

        var query = new GetAllReviewsQuery(1);


        //Act
        var result = await _handler.Handle(query, CancellationToken.None);
        
        //Assert
        
        result.TotalCount.ShouldBe(0);
        result.HasNextPage.ShouldBe(false);
        var companyList = result.Items;
        companyList.ShouldBeEmpty();
        
    }
}