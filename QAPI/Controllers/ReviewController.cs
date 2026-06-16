using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QApplication.Responses;
using QApplication.UseCases.Reviews.Commands.CreateReview;
using QApplication.UseCases.Reviews.Queries.GetAllCustomerReviews;
using QApplication.UseCases.Reviews.Queries.GetAllReviews;
using QApplication.UseCases.Reviews.Queries.GetReviewById;
using QDomain.Enums;

namespace QAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly ILogger<ReviewController> _logger;
    private readonly IMediator _mediator;

    public ReviewController(ILogger<ReviewController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [Authorize(Roles = nameof(UserRoles.Customer) + "," + nameof(UserRoles.Employee))]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewResponseModel>>> GetAllAsync([FromQuery] int pageNumber = 1)
    {
        _logger.LogInformation("Received request to get all reviews. PageNumber: {PageNumber}, PageSize: 15",
            pageNumber);
        var query = new GetAllReviewsQuery(pageNumber);
        var reviews = await _mediator.Send(query);
        return Ok(reviews);
    }

    [Authorize(Roles = nameof(UserRoles.Customer) + "," + nameof(UserRoles.Employee))]
    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewResponseModel>> GetByIdAsync([FromRoute] int id)
    {
        _logger.LogInformation("Received request to get review with Id: {reviewId}", id);
        var query = new GetReviewByIdQuery(id);
        var review = await _mediator.Send(query);
        _logger.LogInformation("Successfully returned review with Id: {reviewId}", id);
        return Ok(review);
    }

    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpPost("create-review")]
    public async Task<IActionResult> PostAsync([FromBody] CreateReviewCommand request)
    {
        _logger.LogInformation("Received request to create review to queue with Id: {queueId}", request.QueueId);
        var review = await _mediator.Send(request);
        _logger.LogInformation("Successfully created review with Id: {reviewId}", review.Id);
        return Ok(review);
    }
    
    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpGet("review-history/customer/")]
    public async Task<ActionResult<PagedResponse<ReviewResponseModel>>> GetReviewsByCustomer([FromQuery] int pageNumber=1)
    {
        _logger.LogInformation("Received request to get customer review history with PageNumber: {pageNumber}", pageNumber);
        var query = new GetReviewsByCustomerQuery(pageNumber);
        var queue = await _mediator.Send(query);
        _logger.LogInformation("Successfully returned reviews.");
        return Ok(queue);
    }
}