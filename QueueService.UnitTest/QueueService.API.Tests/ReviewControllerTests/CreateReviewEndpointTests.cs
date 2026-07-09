using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QAPI.Controllers;
using QApplication.Responses;
using QApplication.UseCases.Reviews.Commands.CreateReview;
using Shouldly;

namespace QueueService.UnitTest.QueueService.API.Tests.ReviewControllerTests;

public class CreateReviewEndpointTests
{
     private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<ReviewController>> _mockLogger;
    private readonly ReviewController _reviewController;

    public CreateReviewEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ReviewController>>();
        _reviewController = new ReviewController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task Should_Return_CreatedReview_WithOkStatusCode()
    {
        // Arrange
        var createReviewCommand = new CreateReviewCommand(1, 5, "Test, Review Text");

        var expectedResponse = new ReviewResponseModel
        {
            Id = 1,
            CustomerId = 1,
            EmployeeId = 1,
            Grade = 5,
            QueueId = 1,
            ReviewText = "Test Review Text"
        };

        _mockMediator
            .Setup(m => m.Send(createReviewCommand, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reviewController.PostAsync(createReviewCommand);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<ReviewResponseModel>();

        returnValue.Id.ShouldBe(expectedResponse.Id);
        returnValue.CustomerId.ShouldBe(expectedResponse.CustomerId);
        returnValue.EmployeeId.ShouldBe(expectedResponse.EmployeeId);
        returnValue.Grade.ShouldBe(expectedResponse.Grade);
        returnValue.QueueId.ShouldBe(expectedResponse.QueueId);
        returnValue.ReviewText.ShouldBe(expectedResponse.ReviewText);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_InvalidCommand()
    {
        // Arrange
        var createReviewCommand = new CreateReviewCommand(1, 5, "");


        _mockMediator
            .Setup(m => m.Send(createReviewCommand, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));


        //Act
        var result = _reviewController.PostAsync(createReviewCommand);

        //Assert
        var exception = result.ShouldThrow<FluentValidation.ValidationException>();
        exception.Message.ShouldBe("Validation failed");
    }
}