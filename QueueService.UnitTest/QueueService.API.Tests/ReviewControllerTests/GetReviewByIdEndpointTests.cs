using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QAPI.Controllers;
using QApplication.Exceptions;
using QApplication.Responses;
using QApplication.UseCases.Reviews.Queries.GetReviewById;
using Shouldly;

namespace QueueService.UnitTest.QueueService.API.Tests.ReviewControllerTests;

public class GetReviewByIdEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<ReviewController>> _mockLogger;
    private readonly ReviewController _reviewController;

    public GetReviewByIdEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ReviewController>>();
        _reviewController = new ReviewController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_ReviewById_WithOkStatusCode()
    {
        var query = new GetReviewByIdQuery(1);
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
            .Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reviewController.GetByIdAsync(1);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<ReviewResponseModel>();
        returnedValue.Id.ShouldBe(expectedResponse.Id);
        returnedValue.CustomerId.ShouldBe(expectedResponse.CustomerId);
        returnedValue.EmployeeId.ShouldBe(expectedResponse.EmployeeId);
        returnedValue.Grade.ShouldBe(expectedResponse.Grade);
        returnedValue.QueueId.ShouldBe(expectedResponse.QueueId);
        returnedValue.ReviewText.ShouldBe(expectedResponse.ReviewText);
        

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_CustomerDoesNotExist()
    {
        var query = new GetReviewByIdQuery(1);
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Review not found");
    
        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _reviewController.GetByIdAsync(1);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Review not found");

    }
}