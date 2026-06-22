using Microsoft.Extensions.Logging;
using Moq;
using QApplication.Interfaces;
using QInfrastructure.Persistence.DataBase;
using QueueService.UnitTest.QueueService.Application.Tests.Infrastructure;
using QUserService.Contracts.Interfaces;
using Shouldly;

namespace QueueService.UnitTest.QueueService.Application.Tests.QueueServiceTests;

public class GetCompanyComplaintsAsyncTests
{
    private readonly QueueDbContext _dbContext;
    private readonly Mock<ILogger<QApplication.Services.QueueService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IPersonNameProvider> _mockPersonName;
    private readonly QApplication.Services.QueueService _queueService;

    public GetCompanyComplaintsAsyncTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<QApplication.Services.QueueService>>();
        _mockUserService = new Mock<IUserService>();
        _mockPersonName = new Mock<IPersonNameProvider>();
        _queueService = new QApplication.Services.QueueService(_dbContext, _mockLogger.Object, _mockUserService.Object,
            _mockPersonName.Object);
    }

    [Fact]
    public async Task Handler_Should_Return_Company_Complaints_Successfully()
    {
        //Arrange
        var complaint = TestDataSeeder.CreateComplaint();
        await _dbContext.Complaints.AddAsync(complaint, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);


        //Act
        var result = await _queueService.GetCompanyComplaintsAsync(1);


        //Assert
        result.ShouldNotBeNull();
        var firstComplaint = result.FirstOrDefault();
        firstComplaint!.Id.ShouldBe(firstComplaint.Id);
        firstComplaint.CustomerId.ShouldBe(complaint.CustomerId);
        firstComplaint.QueueId.ShouldBe(complaint.QueueId);
        firstComplaint.ComplaintText.ShouldBe(complaint.ComplaintText);
    }


    [Fact]
    public async Task Handler_Should_Throw_When_Company_Complaints_Is_Empty()
    {
        //Arrange


        //Act
        var result = await _queueService.GetCompanyComplaintsAsync(1);


        //Assert
        result.ShouldBeEmpty();
    }
}