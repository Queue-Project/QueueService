using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QAPI.Controllers;
using QApplication.Responses;
using QApplication.UseCases.Complaints.Commands.CreateComplaint;
using QDomain.Enums;
using Shouldly;

namespace QueueService.UnitTest.QueueService.API.Tests.ComplaintControllerTests;

public class CreateComplaintEndpointTests
{
      private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<ComplaintController>> _mockLogger;
    private readonly ComplaintController _complaintController;

    public CreateComplaintEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ComplaintController>>();
        _complaintController = new ComplaintController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_CreatedComplaint_WithOkStatusCode()
    {
        // Arrange
        var createComplaintCommand = new CreateComplaintCommand(1,  "Test Complaint Text");

        var expectedResponse = new ComplaintResponseModel()
        {
            Id = 1,
            CustomerId = 1,
            EmployeeId = 1,
            QueueId = 1,
            ComplaintText = "Test Complaint Text",
            ComplaintStatus = ComplaintStatus.Pending
        };

        _mockMediator
            .Setup(m => m.Send(createComplaintCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _complaintController.AddComplaintAsync(createComplaintCommand);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<ComplaintResponseModel>();

        returnValue.Id.ShouldBe(expectedResponse.Id);
        returnValue.CustomerId.ShouldBe(expectedResponse.CustomerId);
        returnValue.EmployeeId.ShouldBe(expectedResponse.EmployeeId);
        returnValue.ComplaintText.ShouldBe(expectedResponse.ComplaintText);
        returnValue.QueueId.ShouldBe(expectedResponse.QueueId);
        returnValue.ComplaintStatus.ShouldBe(expectedResponse.ComplaintStatus);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_InvalidCommand()
    {
        // Arrange
        var createComplaintCommand = new CreateComplaintCommand(1,  "");



        _mockMediator
            .Setup(m => m.Send(createComplaintCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _complaintController.AddComplaintAsync(createComplaintCommand);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }
}