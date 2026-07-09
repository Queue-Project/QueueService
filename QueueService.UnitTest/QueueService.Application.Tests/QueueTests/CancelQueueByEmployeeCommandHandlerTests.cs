using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Interfaces;
using QApplication.Responses;
using QApplication.UseCases.Queues.Commands.CancelQueueByEmployee;
using QContracts.Events.Enums;
using QDomain.Enums;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueTests;

public class CancelQueueByEmployeeCommandHandlerTests
{
     private readonly Mock<ILogger<CancelQueueByEmployeeCommandHandler>> _mockLogger;
    private readonly Mock<IQueueCancellationService> _mockQueueCancellationService;
    private readonly QueueDbContext _dbContext;
    private readonly CancelQueueByEmployeeCommandHandler _handler;

    public CancelQueueByEmployeeCommandHandlerTests()
    {
        _mockLogger = new Mock<ILogger<CancelQueueByEmployeeCommandHandler>>();
        _mockQueueCancellationService = new Mock<IQueueCancellationService>();
        _dbContext = TestDbContextFactory.Create();
        _handler = new CancelQueueByEmployeeCommandHandler(_mockLogger.Object, _mockQueueCancellationService.Object);
    }

    [Fact]
    public async Task Handler_Should_Cancel_Successfully()
    {
        //Arrange
        var queue = TestDataSeeder.CreateQueueWithStartTime();
        await _dbContext.Queues.AddAsync(queue, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _mockQueueCancellationService.Setup(s => s.GetAndValidateQueueForCancellation(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(queue);

        var expectedResponse = new QueueResponseModel
        {
            Id = 1,
            CompanyId = 1,
            BranchId = 1,
            CustomerId = 1,
            EmployeeId = 1,
            ServiceId = 1,
            StartTime = queue.StartTime,
            EndTime = queue.EndTime,
            Status = QueueStatus.CancelledByEmployee
        };

        _mockQueueCancellationService.Setup(s => s.ProcessCancellation(queue, QueueStatus.CancelledByEmployee,
                "Can not come", UpdatedQueueStatus.CanceledByEmployee, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var command = new CancelQueueByEmployeeCommand(1, "Can not come");
        
        //Act
        var result = await _handler.Handle(command, CancellationToken.None);

        //Assert
        
        result.Id.ShouldBe(command.QueueId);
        result.CompanyId.ShouldBe(expectedResponse.CompanyId);
        result.EmployeeId.ShouldBe(expectedResponse.EmployeeId);
        result.BranchId.ShouldBe(expectedResponse.BranchId);
        result.CustomerId.ShouldBe(expectedResponse.CustomerId);
        result.ServiceId.ShouldBe(expectedResponse.ServiceId);
        result.StartTime.ShouldBe(expectedResponse.StartTime);
        result.EndTime.ShouldBe(expectedResponse.EndTime);
        result.Status.ShouldBe(expectedResponse.Status);
    }
    
}