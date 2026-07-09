using System.Net;
using System.Security.Claims;
using BranchService.Contracts.Interfaces;
using BranchService.Contracts.Requests;
using BranchService.Contracts.Responses;
using MagicOnion;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.UseCases.Queues.Commands.CreateQueue;
using QContracts.Events;
using QDomain.Enums;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using QUserService.Contracts.Requests.CustomerRequests;
using QUserService.Contracts.Requests.EmployeeRequests;
using QUserService.Contracts.Requests.UserRequests;
using QUserService.Contracts.Responses.BlockedCustomersResponses;
using QUserService.Contracts.Responses.CustomerResponses;
using QUserService.Contracts.Responses.EmployeeResponses;
using QUserService.Contracts.Responses.UserResponses;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueTests;

public class CreateQueueCommandHandlerTests
{
    private readonly Mock<ILogger<CreateQueueCommandHandler>> _mockLogger;
    private readonly QueueDbContext _dbContext;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
    private readonly Mock<IHttpContextAccessor> _mockAccessor;
    private readonly Mock<IBranchService> _mockBranchService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly CreateQueueCommandHandler _handler;

    public CreateQueueCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CreateQueueCommandHandler>>();
        _dbContext = TestDbContextFactory.Create();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _mockAccessor = new Mock<IHttpContextAccessor>();
        _mockBranchService = new Mock<IBranchService>();
        _mockUserService = new Mock<IUserService>();
        _handler = new CreateQueueCommandHandler(_mockLogger.Object, _dbContext, _mockPublishEndpoint.Object,
            _mockAccessor.Object, _mockBranchService.Object, _mockUserService.Object);
    }


    [Fact]
    public async Task Handler_Should_Create_Queue_Successfully()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));


        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));


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

        var customerResultExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2006, 06, 06),
            Gender = "Male",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            PostalCode = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));

        var employeeResultExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeResultExpectedResponse));


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

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.CompanyId.ShouldBe(command.CompanyId);
        result.BranchId.ShouldBe(command.BranchId);
        result.EmployeeId.ShouldBe(command.EmployeeId);
        result.ServiceId.ShouldBe(command.ServiceId);
        result.StartTime.ShouldBe(command.StartTime);
        result.Status.ShouldBe(QueueStatus.Pending);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Not_Found()
    {
        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


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
    public async Task Handler_Should_Throw_When_User_Is_Not_Customer()
    {
        //Arrange


        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


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
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe("User is not a customer");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_User_Is_Not_Found_For_Customer_Id()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = false,
            ErrorMessage = "User not found for this customer"
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe("User not found for this customer");
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Creation_Validation_Is_In_Valid()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = false,
            ErrorMessage = "Queue is out of working hours",
            IsWithinWorkingHours = false,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe(queueCreationValidationExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Company_Not_Found()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = false,
            ErrorMessage = "Company not found",
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(companyResultExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Branch_Not_Found()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = false,
            ErrorMessage = "Branch not found",
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(branchResultExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Company_Service_Not_Found()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));

        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = false,
            ErrorMessage = "CompanyService not found",
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(companyServiceResultExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Time_Slot_Is_Not_Available()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));

        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));

        var scheduleResultExpectedResponse = new EmployeeAvailabilityResponse
        {
            IsAvailable = false,
            AvailableSlots = new List<TimeSlot>(),
            ErrorMessage = "The selected time slot is not available. Please choose a different time."
        };

        _mockUserService.Setup(s => s.CheckEmployeeAvailability(It.IsAny<EmployeeAvailabilityRequest>()))
            .Returns(UnaryResult.FromResult(scheduleResultExpectedResponse));

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        exception.Message.ShouldBe(scheduleResultExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Customer_Not_Found()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));

        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));

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

        var customerResultExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2006, 06, 06),
            Gender = "Male",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            PostalCode = null,
            IsValid = false,
            ErrorMessage =
                $"Customer with Id {currentCustomerExpectedResponse.CustomerId} not found for adding new queue"
        };

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(customerResultExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Employee_Not_Found()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));

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

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));

        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));

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

        var customerResultExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2006, 06, 06),
            Gender = "Male",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            PostalCode = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));


        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));


        var employeeResultExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            IsValid = false,
            ErrorMessage = $"Employee with Id {command.EmployeeId} not found"
        };

        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeResultExpectedResponse));


        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(employeeResultExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Customer_Blocked()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));

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

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));

        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));

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

        var customerResultExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2006, 06, 06),
            Gender = "Male",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            PostalCode = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));


        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));


        var employeeResultExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeResultExpectedResponse));

        var blockedResultExpectedResponse = new BlockedCustomerValidationResponse
        {
            IsBlocked = true,
            IsBlockedForever = false,
            BannedUntil = null,
            BlockReason = "Did not come three times",
            ErrorMessage = "You are blocked by this company!"
        };

        _mockUserService.Setup(s => s.IsCustomerBlockedForCompany(It.IsAny<IsCustomerBlockedRequest>()))
            .Returns(UnaryResult.FromResult(blockedResultExpectedResponse));

        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe(blockedResultExpectedResponse.ErrorMessage);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Is_Double()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 1, 1, 1, new DateTimeOffset(new DateTime(2026,06,22, 9,20,00)));

        var queue = TestDataSeeder.CreateQueue();
        queue.StartTime = new DateTimeOffset(new DateTime(2026,06,22, 9,20,00));
        await _dbContext.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


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

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));

        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));

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

        var customerResultExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2006, 06, 06),
            Gender = "Male",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            PostalCode = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));


        var employeeResultExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeResultExpectedResponse));

        //Act
        var result = _handler.Handle(command, CancellationToken.None);

        //Assert

        var exception = await result.ShouldThrowAsync<Exception>();
        exception.Message.ShouldBe("This slot is already booked!");
    }

    [Fact]
    public async Task Handler_Should_Publish_Event()
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
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCurrentCustomer(It.IsAny<CurrentUserRequest>()))
            .Returns(UnaryResult.FromResult(currentCustomerExpectedResponse));

        var currentUserEmailExpectedResponse = new UserEmailResponse
        {
            UserId = 1,
            EmailAddress = "test@gmail.com",
            CustomerId = 1,
            EmployeeId = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetUserEmailByCustomerId(It.IsAny<GetUserEmailByCustomerIdRequest>()))
            .Returns(UnaryResult.FromResult(currentUserEmailExpectedResponse));

        var queueCreationValidationExpectedResponse = new QueueCreationValidationResponse
        {
            RequestId = Guid.NewGuid(),
            IsValid = true,
            ErrorMessage = null,
            IsWithinWorkingHours = true,
            WorkingHoursMessage = null,
            IsWithinBreakTime = false,
            BreakTimeMessage = null,
            MaxTicketsPerDay = 100
        };

        _mockBranchService.Setup(s => s.ValidateQueueCreationAsync(It.IsAny<QueueCreationValidationRequest>()))
            .Returns(UnaryResult.FromResult(queueCreationValidationExpectedResponse));

        var companyResultExpectedResponse = new CompanyResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyName = "Test Company Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyId(It.IsAny<CompanyRequest>()))
            .Returns(UnaryResult.FromResult(companyResultExpectedResponse));

        var branchResultExpectedResponse = new BranchResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            BranchId = 1,
            IsValid = true,
            ErrorMessage = null,
            BranchName = "Test Branch Name"
        };


        _mockBranchService.Setup(s => s.CheckBranchId(It.IsAny<BranchRequest>()))
            .Returns(UnaryResult.FromResult(branchResultExpectedResponse));


        var companyServiceResultExpectedResponse = new CompanyServiceResponse
        {
            RequestId = Guid.NewGuid(),
            CompanyId = 1,
            CompanyServiceId = 1,
            IsValid = true,
            ErrorMessage = null,
            CompanyServiceName = "Test CompanyService Name"
        };

        _mockBranchService.Setup(s => s.CheckCompanyServiceId(It.IsAny<CompanyServiceRequest>()))
            .Returns(UnaryResult.FromResult(companyServiceResultExpectedResponse));


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

        var customerResultExpectedResponse = new CustomerResponse
        {
            Id = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2006, 06, 06),
            Gender = "Male",
            Country = "Test Country",
            City = "Test City",
            Address = "Test Address",
            PostalCode = null,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetCustomerById(It.IsAny<CustomerByIdRequest>()))
            .Returns(UnaryResult.FromResult(customerResultExpectedResponse));

        var employeeResultExpectedResponse = new EmployeeResponse
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            ServiceId = 1,
            FirstName = "Test Firstname",
            LastName = "Test Lastname",
            Position = "Test Position",
            PhoneNumber = "+992923324252",
            CreatedAt = DateTime.UtcNow,
            IsValid = true,
            ErrorMessage = null
        };

        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeResultExpectedResponse));


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

        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow.Date.AddHours(9).AddMinutes(20));

        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert

        _mockPublishEndpoint.Verify(s=>s.Publish(It.IsAny<QueueEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}