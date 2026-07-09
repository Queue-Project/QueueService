using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QAPI.Controllers;
using QApplication.Exceptions;
using QApplication.Responses;
using QApplication.UseCases.Complaints.Queries.GetComplaintById;
using QDomain.Enums;
using Shouldly;

namespace QueueService.UnitTest.QueueService.API.Tests.ComplaintControllerTests;

public class GetComplaintByIdEndpointTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<ComplaintController>> _mockLogger;
    private readonly ComplaintController _complaintController;

    public GetComplaintByIdEndpointTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<ComplaintController>>();
        _complaintController = new ComplaintController(_mockLogger.Object, _mockMediator.Object);
    }
    
    [Fact]
    public async Task Should_Return_ComplaintById_WithOkStatusCode()
    {
        var query = new GetComplaintByIdQuery(1);
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
            .Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _complaintController.GetComplaintByIdAsync(1);

        // Assert
        result.ShouldNotBeNull();
        
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(200);
        
        var returnedValue = okResult.Value.ShouldBeOfType<ComplaintResponseModel>();
        returnedValue.Id.ShouldBe(expectedResponse.Id);
        returnedValue.CustomerId.ShouldBe(expectedResponse.CustomerId);
        returnedValue.EmployeeId.ShouldBe(expectedResponse.EmployeeId);
        returnedValue.ComplaintText.ShouldBe(expectedResponse.ComplaintText);
        returnedValue.QueueId.ShouldBe(expectedResponse.QueueId);
        returnedValue.ComplaintStatus.ShouldBe(expectedResponse.ComplaintStatus);
        

    }
    
    [Fact]
    public async Task Should_Return_NotFound_When_CustomerDoesNotExist()
    {
        var query = new GetComplaintByIdQuery(1);
        var expectedException = new HttpStatusCodeException(HttpStatusCode.NotFound, "Complaint not found");
    
        _mockMediator
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var result =  _complaintController.GetComplaintByIdAsync(1);

        //Assert
        var exception = await result.ShouldThrowAsync<HttpStatusCodeException>();
        
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.Message.ShouldBe("Complaint not found");

    }
}