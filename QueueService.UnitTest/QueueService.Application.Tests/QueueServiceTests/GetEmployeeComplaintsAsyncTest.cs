using System.Net;
using MagicOnion;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Contracts.Responses.EmployeeResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetEmployeeComplaintsAsyncTest
{
    
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetEmployeeComplaintsAsyncTest()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }
    
     [Fact]
    public async Task Handler_Should_Return_Employee_Complaints_Successfully()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        

        var employeeExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            Position = "Test Position",
            IsValid = true,
            ErrorMessage =null
        };


        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));


        //Act
        var result = await _queueService.GetEmployeeComplaintsAsync(1);


        //Assert
        result.ShouldNotBeNull();
        var firstComplaint = result.FirstOrDefault();
        firstComplaint!.Id.ShouldBe(firstComplaint.Id);
        firstComplaint.CustomerId.ShouldBe(complaint.CustomerId);
        firstComplaint.QueueId.ShouldBe(complaint.QueueId);
        firstComplaint.ComplaintText.ShouldBe(complaint.ComplaintText);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Employee_Not_Found()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        

        var employeeExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            Position = "Test Position",
            IsValid = false,
            ErrorMessage = $"Employee with Id 1 not found"
        };


        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));
        

        //Act
        var result =  _queueService.GetEmployeeComplaintsAsync( 1);


        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(employeeExpectedResponse.ErrorMessage);


    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Employee_Complaint_Is_Empty()
    {
        //Arrange
        
        

        var employeeExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            Position = "Test Position",
            IsValid = true,
            ErrorMessage =null
        };


        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));
        

        //Act
        var result = await  _queueService.GetEmployeeReviewsAsync( 1);


        //Assert
        result.ShouldBeEmpty();


    }
}