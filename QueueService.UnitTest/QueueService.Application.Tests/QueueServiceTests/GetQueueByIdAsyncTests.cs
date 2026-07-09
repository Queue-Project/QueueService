using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetQueueByIdAsyncTests
{
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetQueueByIdAsyncTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Queue_Info_Successfully()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        var expectedCustomerName = "Test Customer Name";
        var expectedEmployeeName = "Test Employee Name";

        _mockPersonName.Setup(s => s.GetCustomerNameAsync(queue.CustomerId))
            .ReturnsAsync(expectedCustomerName);

        _mockPersonName.Setup(s => s.GetEmployeeNameAsync(queue.EmployeeId))
            .ReturnsAsync(expectedEmployeeName);


        //Act
        var result = await _queueService.GetQueueByIdAsync(1);


        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(result.Id);
        result.CustomerId.ShouldBe(queue.CustomerId);
        result.CompanyId.ShouldBe(queue.CompanyId);
        result.EmployeeId.ShouldBe(queue.EmployeeId);
        result.BranchId.ShouldBe(queue.BranchId);
        result.ServiceId.ShouldBe(queue.ServiceId);
        result.StartTime.ShouldBe(queue.StartTime);
        result.EndTime.ShouldBe(queue.EndTime);
        result.CustomerName.ShouldBe(expectedCustomerName);
        result.EmployeeName.ShouldBe(expectedEmployeeName);
    }

    [Fact]
    public async Task Handler_Should_Throw_When_Queue_Not_Found()
    {
        //Arrange


        var expectedCustomerName = "Test Customer Name";
        var expectedEmployeeName = "Test Employee Name";

        _mockPersonName.Setup(s => s.GetCustomerNameAsync(1))
            .ReturnsAsync(expectedCustomerName);

        _mockPersonName.Setup(s => s.GetEmployeeNameAsync(1))
            .ReturnsAsync(expectedEmployeeName);


        //Act
        var result =  _queueService.GetQueueByIdAsync(1);


        //Assert
        var exception = await result.ResponseAsync.ShouldThrowAsync<HttpStatusCodeException>();
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe($"Queue with Id 1 not found");
    }
}