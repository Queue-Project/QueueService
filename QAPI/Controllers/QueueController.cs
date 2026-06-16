using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QApplication.Responses;
using QApplication.UseCases.Queues.Commands.CancelQueueByCustomer;
using QApplication.UseCases.Queues.Commands.CancelQueueByEmployee;
using QApplication.UseCases.Queues.Commands.CreateQueue;
using QApplication.UseCases.Queues.Commands.UpdateQueueStatus;
using QApplication.UseCases.Queues.Queries.GetAllQueues;
using QApplication.UseCases.Queues.Queries.GetQueueById;
using QApplication.UseCases.Queues.Queries.GetQueuesByCustomer;
using QApplication.UseCases.Queues.Queries.GetQueuesByEmployee;
using QDomain.Enums;

namespace QAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QueueController : ControllerBase
{
    private readonly ILogger<QueueController> _logger;
    private readonly IMediator _mediator;

    public QueueController(ILogger<QueueController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [Authorize(Roles = nameof(UserRoles.CompanyAdmin))]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<QueueResponseModel>>> GetAllAsync([FromQuery]int pageNumber=1)
    {
        _logger.LogInformation("Received request to get all queues. PageNumber: {PageNumber}",
            pageNumber);
        var query = new GetAllQueuesQuery(pageNumber);
        var queues = await _mediator.Send(query);
        return Ok(queues);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<QueueResponseModel>> GetByIdAsync([FromRoute] int id)
    {
        _logger.LogInformation("Received request to get queue by Id: {queueId}", id);
        var query = new GetQueueByIdQuery(id);
        var queue = await _mediator.Send(query);
        _logger.LogInformation("Successfully returned queue with Id: {queueId}", id);
        return Ok(queue);
    }

    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpPost("book")]
    public async Task<IActionResult> PostAsync([FromBody] CreateQueueCommand request)
    {
        _logger.LogInformation("Received request to create queue.");

        var queue = await _mediator.Send(request);
        _logger.LogInformation("Successfully created queue with Id: {queueId}", queue.Id);
        return Ok(queue);
    }

    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpPut("cancel/customer")]
    public async Task<IActionResult> CancelQueueByCustomerAsync([FromBody] CancelQueueByCustomerCommand request)
    {
        _logger.LogInformation("Received request to cancel queue with Id {queueId} by customer.", request.QueueId);
        var cancel = await _mediator.Send(request);
        _logger.LogInformation("Successfully canceled queue with Id: {queueId} by customer.", request.QueueId);
        return Ok(cancel);
    }

    [Authorize(Roles =
        nameof(UserRoles.CompanyAdmin) + "," + nameof(UserRoles.SystemAdmin) + "," + nameof(UserRoles.Employee))]
    [HttpPut("cancel/employee")]
    public async Task<IActionResult> CancelQueueByEmployeeAsync([FromBody] CancelQueueByEmployeeCommand request)
    {
        _logger.LogInformation("Received request to cancel queue with Id {queueId} by employee.", request.QueueId);
        var cancel = await _mediator.Send(request);
        _logger.LogInformation("Successfully canceled queue with Id {queueId} by employee.", request.QueueId);
        return Ok(cancel);
    }

    [Authorize(Roles =
        nameof(UserRoles.CompanyAdmin) + "," + nameof(UserRoles.SystemAdmin) + "," + nameof(UserRoles.Employee))]
    [HttpPut("status/update")]
    public async Task<ActionResult<QueueResponseModel>> UpdateStatusAsync([FromBody] UpdateQueueStatusCommand request)
    {
        _logger.LogInformation("Received request to update queue status {newStatus} with Id: {queueId}",
            request.newStatus, request.QueueId);
        var result = await _mediator.Send(request);
        _logger.LogInformation("Successfully updated queue status with Id: {queueId}", request.QueueId);
        return Ok(result);
    }

    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpGet("history/customer/")]
    public async Task<ActionResult<PagedResponse<QueueResponseModel>>> GetQueuesByCustomerAsync([FromQuery] int pageNumber=1)
    {
        _logger.LogInformation("Received request to get  customer queue history with PageNumber: {pageNumber}", pageNumber);
        var query = new GetQueuesByCustomerQuery(pageNumber);
        var queue = await _mediator.Send(query);
        _logger.LogInformation("Successfully returned queues.");
        return Ok(queue);
    }

    [Authorize(Roles =nameof(UserRoles.Employee))]
    [HttpGet("history/employee/")]
    public async Task<ActionResult<PagedResponse<QueueResponseModel>>> GetQueuesByEmployeeAsync([FromRoute] int pageNumber=1)
    {
        _logger.LogInformation("Received request to get employee queue history with PageNumber: {pageNUmber}", pageNumber);
        var query = new GetQueuesByEmployeeQuery(pageNumber);
        var queue = await _mediator.Send(query);
        _logger.LogInformation("Successfully returned queues.");
        return Ok(queue);
    }
}