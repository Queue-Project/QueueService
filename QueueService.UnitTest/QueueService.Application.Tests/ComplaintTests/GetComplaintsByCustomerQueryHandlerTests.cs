using System.Net;
using System.Security.Claims;
using MagicOnion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.UseCases.Complaints.Queries.GetAllCustomerComplints;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.ComplaintTests;

public class GetComplaintsByCustomerQueryHandlerTests
{
    private readonly Mock<ILogger<GetComplaintsByCustomerQueryHandler>> _mockLogger;
    private readonly QueueDbContext _dbContext;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IUserService> _mockUserService;
    private readonly GetComplaintsByCustomerQueryHandler _handler;

    public GetComplaintsByCustomerQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetComplaintsByCustomerQueryHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockUserService = new Mock<IUserService>();
        _handler = new GetComplaintsByCustomerQueryHandler(_mockLogger.Object, _dbContext, _mockAccessor.Object,
            _mockUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_AllComplaints_When_User_Is_Customer_Successfully()
    {
        //Arrange
        var complaints = TestDataSeeder.CreateComplaints();
        await _dbContext.Complaints.AddRangeAsync(complaints, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);


        var customerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };


        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));

        var query = new GetComplaintsByCustomerQuery(1);


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
        var query = new GetComplaintsByCustomerQuery(1);

        var expectedResponse = new UnauthorizedAccessException("User not authenticated");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Throws(expectedResponse);

        //Act
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User not authenticated");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Is_Not_Customer()
    {
        //Arrange


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


        var query = new GetComplaintsByCustomerQuery(1);


        //Act
        var result = _handler.Handle(query, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe("User is not a customer");
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Review_List_Is_Empty()
    {
        //Arrange


        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);


        var customerExpectedResponse = new CurrentCustomerResponse
        {
            CustomerId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            IsValid = true,
            ErrorMessage = null
        };


        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(customerExpectedResponse));

        var query = new GetComplaintsByCustomerQuery(1);


        //Act
        var result = _handler.Handle(query, CancellationToken.None);


        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("ComplaintEntity");
    }
}