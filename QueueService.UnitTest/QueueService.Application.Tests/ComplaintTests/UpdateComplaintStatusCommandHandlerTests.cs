using System.Net;
using System.Security.Claims;
using MagicOnion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.UseCases.Complaints.Commands.UpdateComplaintStatus;
using QDomain.Enums;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.ComplaintTests;

public class UpdateComplaintStatusCommandHandlerTests
{
    private readonly Mock<ILogger<UpdateComplaintStatusCommandHandler>> _mockLogger;
    private readonly QueueDbContext _dbContext;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IUserService> _mockUserService;
    private readonly UpdateComplaintStatusCommandHandler _handler;

    public UpdateComplaintStatusCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<UpdateComplaintStatusCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockUserService = new Mock<IUserService>();
        _handler = new UpdateComplaintStatusCommandHandler(_mockLogger.Object, _dbContext, _mockAccessor.Object,
            _mockUserService.Object);
    }

    [Fact]
    public async Task Handler_Should_Update_Complaint_Status_Successfully()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        complaint.ComplaintStatus = ComplaintStatus.Pending;
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new Claim("id", "1");


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

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var command = new UpdateComplaintStatusCommand(1, ComplaintStatus.Reviewed, "Test Reason Text");
        
        //Act

        var result = await _handler.Handle(command, CancellationToken.None);
        
        //Assert
        
        result.Id.ShouldBe(command.Id);
        result.CustomerId.ShouldBe(complaint.CustomerId);
        result.EmployeeId.ShouldBe(employeeExpectedResponse.EmployeeId);
        result.QueueId.ShouldBe(complaint.QueueId);
        result.ComplaintStatus.ShouldBe(command.ComplaintStatus);
        result.ResponseText.ShouldBe(command.ResponseText);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
       
        var command = new UpdateComplaintStatusCommand(1, ComplaintStatus.Reviewed, "Test Reason Text");


        var expectedResponse = new UnauthorizedAccessException("User not authenticated");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Throws(expectedResponse);

        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User not authenticated");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Is_Not_Employee()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        complaint.ComplaintStatus = ComplaintStatus.Pending;
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new Claim("id", "1");


        var employeeExpectedResponse = new CurrentEmployeeResponse
        {
            EmployeeId = 1,
            CompanyId = 1,
            BranchId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            IsValid = false,
            ErrorMessage = "User is not an employee"
        };

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var command = new UpdateComplaintStatusCommand(1, ComplaintStatus.Reviewed, "Test Reason Text");
        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe("User is not an employee");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Complaint_Not_Found()
    {
        //Arrange


        var expectedResponse = new Claim("id", "1");


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

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var command = new UpdateComplaintStatusCommand(1, ComplaintStatus.Reviewed, "Test Reason Text");
        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("ComplaintEntity");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Status_Updated_From_Pending_To_Resolved()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        complaint.ComplaintStatus = ComplaintStatus.Pending;
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new Claim("id", "1");


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

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var command = new UpdateComplaintStatusCommand(1, ComplaintStatus.Resolved, "Test Reason Text");
        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("Resolved can be when status is Reviewed");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Status_Updated_From_Reviewed_To_Pending()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        complaint.ComplaintStatus = ComplaintStatus.Reviewed;
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new Claim("id", "1");


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

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var command = new UpdateComplaintStatusCommand(1, ComplaintStatus.Pending, "Test Reason Text");
        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("Pending status can only be Reviewed");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Status_Updated_From_Resolved_To_Pending()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        complaint.ComplaintStatus = ComplaintStatus.Resolved;
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new Claim("id", "1");


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

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var command = new UpdateComplaintStatusCommand(1, ComplaintStatus.Pending, "Test Reason Text");
        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("This complaint is already finalized and cannot be updated!");
    }
}