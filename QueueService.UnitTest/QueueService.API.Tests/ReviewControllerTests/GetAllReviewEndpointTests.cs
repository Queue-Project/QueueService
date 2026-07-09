using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QAPI.Controllers;
using QApplication.Responses;
using QApplication.UseCases.Reviews.Queries.GetAllReviews;
using Shouldly;

namespace QueueService.UnitTest.QueueService.API.Tests.ReviewControllerTests;

public class GetAllReviewEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<ReviewController>> _mockLogger;
    private readonly ReviewController _reviewController;

    public GetAllReviewEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ReviewController>>();
        _reviewController = new ReviewController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_AllCustomers_WithOkStatusCode()
    {
        var pageNumber = 1;
        var query = new GetAllReviewsQuery(pageNumber);

        var expectedResponse = new PagedResponse<ReviewResponseModel>
        {
            Items = [
                new ReviewResponseModel
                {
                    Id = 1,
                    CustomerId = 1,
                    EmployeeId = 1,
                    Grade = 5,
                    QueueId = 1,
                    ReviewText = "Test Review Text"
                },
                new ReviewResponseModel
                {
                    Id = 2,
                    CustomerId = 1,
                    EmployeeId = 1,
                    Grade = 5,
                    QueueId = 2,
                    ReviewText = "Test Review Text3"
                }
            ],
            PageNumber = 1,
            PageSize = 15
        };

        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _reviewController.GetAllAsync(pageNumber);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<PagedResponse<ReviewResponseModel>>();
        returnedValue.Items.Count.ShouldBe(2);
        returnedValue.HasNextPage.ShouldBe(false);
        

    }
}
