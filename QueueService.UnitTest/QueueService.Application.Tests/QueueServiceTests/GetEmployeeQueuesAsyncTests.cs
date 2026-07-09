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

public class GetEmployeeQueuesAsyncTests
{
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetEmployeeQueuesAsyncTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }
    
     [Fact]
    public async Task Handler_Should_Return_Employee_Queues_Successfully()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var expectedCustomerName = "Test Customer Name";
        var expectedEmployeeName = "Test Employee Name";

        _mockPersonName.Setup(s => s.GetCustomerNameAsync(queue.CustomerId))
            .ReturnsAsync(expectedCustomerName);

        _mockPersonName.Setup(s => s.GetEmployeeNameAsync(queue.EmployeeId))
            .ReturnsAsync(expectedEmployeeName);

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
        var result = await _queueService.GetEmployeeQueuesAsync(1);


        //Assert
        result.ShouldNotBeNull();
        var firstQueue = result.FirstOrDefault();
        firstQueue!.Id.ShouldBe(firstQueue.Id);
        firstQueue.CustomerId.ShouldBe(queue.CustomerId);
        firstQueue.CompanyId.ShouldBe(queue.CompanyId);
        firstQueue.BranchId.ShouldBe(queue.BranchId);
        firstQueue.ServiceId.ShouldBe(queue.ServiceId);
        firstQueue.StartTime.ShouldBe(queue.StartTime);
        firstQueue.EndTime.ShouldBe(queue.EndTime);
       

    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Employee_Not_Found()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueue();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        
        var expectedCustomerName = "Test Firstname Test Lastname";
        var expectedEmployeeName = "Test Firstname Test Lastname";

        _mockPersonName.Setup(s => s.GetCustomerNameAsync(queue.CustomerId))
            .ReturnsAsync(expectedCustomerName);

        _mockPersonName.Setup(s => s.GetEmployeeNameAsync(queue.EmployeeId))
            .ReturnsAsync(expectedEmployeeName);

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
            ErrorMessage = $"Employee with Id {queue.EmployeeId} not found"
        };


        _mockUserService.Setup(s => s.GetEmployeeById(It.IsAny<EmployeeByIdRequest>()))
            .Returns(UnaryResult.FromResult(employeeExpectedResponse));
        

        //Act
        var result =  _queueService.GetEmployeeQueuesAsync( 1);


        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe(employeeExpectedResponse.ErrorMessage);


    }
    
    [Fact]
    public async Task Handler_Should_Throw_When_Employee_Queues_Is_Empty()
    {
        //Arrange
        
        
        var expectedCustomerName = "Test Firstname Test Lastname";
        var expectedEmployeeName = "Test Firstname Test Lastname";


        _mockPersonName.Setup(s => s.GetCustomerNameAsync(1))
            .ReturnsAsync(expectedCustomerName);

        _mockPersonName.Setup(s => s.GetEmployeeNameAsync(1))
            .ReturnsAsync(expectedEmployeeName);

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
        var result = await  _queueService.GetEmployeeQueuesAsync( 1);


        //Assert
        result.ShouldBeEmpty();


    }
}