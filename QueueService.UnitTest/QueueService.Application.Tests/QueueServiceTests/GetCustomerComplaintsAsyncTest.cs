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

public class GetCustomerComplaintsAsyncTest
{
      
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetCustomerComplaintsAsyncTest()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }
    
     [Fact]
    public async Task Handler_Should_Return_Customer_Complaint_Successfully()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
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
        var result = await _queueService.GetCustomerComplaintsAsync(1);


        //Assert
        result.ShouldNotBeNull();
        var firstComplaint = result.FirstOrDefault();
        firstComplaint!.Id.ShouldBe(firstComplaint.Id);
        firstComplaint.CustomerId.ShouldBe(complaint.CustomerId);
        firstComplaint.QueueId.ShouldBe(complaint.QueueId);
        firstComplaint.ComplaintText.ShouldBe(complaint.ComplaintText);

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
        var result =  _queueService.GetCustomerComplaintsAsync( 1);


        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(customerExpectedResponse.ErrorMessage);


    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Customer_Complaint_Is_Empty()
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
        var result = await  _queueService.GetCustomerComplaintsAsync( 1);


        //Assert
        result.ShouldBeEmpty();


    }
}