using System.Net;
using System.Security.Claims;
using MagicOnion;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QApplication.UseCases.Queues.Commands.UpdateQueueStatus;
using QContracts.Events;
using QContracts.Events.Enums;
using QDomain.Enums;
using QDomain.Models;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using QUserService.Contracts.Responses.EmployeeResponses;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueTests;

public class UpdateQueueStatusCommandHandlerTests
{
    private readonly Mock<ILogger<UpdateQueueStatusCommandHandler>> _mockLogger;
    private readonly QueueDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPublishQueueUpdatedEvent> _mockPublishQueueUpdatedEvent;
    private readonly UpdateQueueStatusCommandHandler _handler;

    public UpdateQueueStatusCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<UpdateQueueStatusCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockUserService = new Mock<IUserService>();
        _mockPublishQueueUpdatedEvent = new Mock<IPublishQueueUpdatedEvent>();
        _handler = new UpdateQueueStatusCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object,
            _mockAccessor.Object, _mockUserService.Object, _mockPublishQueueUpdatedEvent.Object);
    }

    [Fact]
    public async Task Handler_Should_Update_Queue_Status_Successfully()
    {
        //Assert

        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));

        var blockedResultExpectedResponse = new BlockedCustomerValidationResponse
        {
            IsBlocked = false,
            IsBlockedForever = false,
            BannedUntil = null,
            BlockReason = null,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.IsCustomerBlockedForCompany(It.IsAny<IsCustomerBlockedRequest>()))
            .Returns(UnaryResult.FromResult(blockedResultExpectedResponse));

        var scheduleResultExpectedResponse = new EmployeeAvailabilityResponse
        {
            IsAvailable = true,
            AvailableSlots = new List<TimeSlot>
            {
                new TimeSlot
                {
                    From = DateTimeOffset.UtcNow.Date.AddHours(8),
                    To = DateTimeOffset.UtcNow.Date.AddHours(12)
                }
            },
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.CheckEmployeeAvailability(It.IsAny<EmployeeAvailabilityRequest>()))
            .Returns(UnaryResult.FromResult(scheduleResultExpectedResponse));

        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Confirmed);

        var queueEventExpectedResponse = new QueueEvent
        {
            Email = "test@gmail.com",
            CompanyId = queue.CompanyId,
            QueueId = queue.Id,
            CustomerId = queue.CustomerId,
            EmployeeId = queue.EmployeeId,
            StartTime = queue.StartTime,
            EndTime = queue.EndTime,
            EventType = QueueEventType.Updated,
            CancelReason = queue.CancelReason,
            Status = UpdatedQueueStatus.Confirmed
        };

        _mockPublishQueueUpdatedEvent.Setup(s => s.CreateQueueUpdatedEvent(queue, command.newStatus))
            .Returns(Task.FromResult(queueEventExpectedResponse));
        

        //Act

        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        result.ShouldNotBeNull();
        result.Id.ShouldBe(queue.Id);
        result.CustomerId.ShouldBe(queue.CustomerId);
        result.EmployeeId.ShouldBe(queue.EmployeeId);
        result.BranchId.ShouldBe(queue.BranchId);
        result.ServiceId.ShouldBe(queue.ServiceId);
        result.StartTime.ShouldBe(queue.StartTime);
        result.EndTime.ShouldBe(queue.EndTime.HasValue ? queue.EndTime.Value : new DateTimeOffset());
        result.Status.ShouldBe(command.newStatus);
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Confirmed);

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
        //Assert

        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));
        
        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Confirmed
        );
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe("User is not an employee");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Not_Found()
    {
        //Assert


        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));
        
        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Confirmed);
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Queue with Id {command.QueueId} not found for this employee");
    }
    
      [Fact]
    public async Task Handler_Should_Throw_When_Status_Updated_From_Confirmed_To_Pending()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueueWithStartTime();
        queue.Status = QueueStatus.Confirmed;
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));


        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Pending);
        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("Confirmed queues can only be Completed, DidNotCome, Cancelled!");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Status_Updated_From_Pending_To_Completed()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));


        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Completed);

        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("Pending queue can only be Confirmed or Cancelled!");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Status_Updated_From_Completed_To_Pending()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueueWithStartTime();
        queue.Status = QueueStatus.Completed;
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));


        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Pending);

        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("This queue is already finalized and cannot be updated!");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Status_Updated_Is_Invalid_Status()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));


        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.CancelledByEmployee);

        
        //Act

        var result =  _handler.Handle(command, CancellationToken.None);
        
        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("Invalid status update by employee");
    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Customer_Blocked()
    {
        //Assert

        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        var expectedResponse = new Claim("id", "1");

        _mockAccessor.Setup(s => s.HttpContext!.User.FindFirst("id"))
            .Returns(expectedResponse);

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


        _mockUserService.Setup(s => s.GetCurrentEmployee(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));
        
        var blockedResultExpectedResponse = new BlockedCustomerValidationResponse
        {
            IsBlocked = true,
            IsBlockedForever = false,
            BannedUntil = null,
            BlockReason = "Did not come 3 times",
            ErrorMessage = "You are blocked by this company!"
        };

        _mockUserService.Setup(s => s.IsCustomerBlockedForCompany(It.IsAny<IsCustomerBlockedRequest>()))
            .Returns(UnaryResult.FromResult(blockedResultExpectedResponse));
        
        var command = new UpdateQueueStatusCommand(
            1,
            QueueStatus.Confirmed);
        
        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe($"You are blocked by this company!");
    }
    
  
    
    
    
    
}