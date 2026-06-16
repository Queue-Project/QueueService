using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QApplication.Requests.ComplaintRequest;
using QApplication.Responses;
using QApplication.UseCases.Complaints.Commands.CreateComplaint;
using QApplication.UseCases.Complaints.Commands.UpdateComplaintStatus;
using QApplication.UseCases.Complaints.Queries.GetAllComplaints;
using QApplication.UseCases.Complaints.Queries.GetAllCustomerComplints;
using QApplication.UseCases.Complaints.Queries.GetComplaintById;
using QDomain.Enums;

namespace QAPI.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ComplaintController: ControllerBase
{
    private readonly ILogger<ComplaintController> _logger;
    private readonly IMediator _mediator;

    public ComplaintController( ILogger<ComplaintController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }
    
    [Authorize(Roles = nameof(UserRoles.Employee)+","+ nameof(UserRoles.Customer))]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComplaintResponseModel>>> GetAllComplaintsAsync([FromQuery]int pageNumber=1)
    {
        _logger.LogInformation("Received request to get all complaints. PageNumber: {PageNumber}, PageSize: 15", pageNumber);
        var query = new GetAllComplaintsQuery(pageNumber);
        var complaints = await _mediator.Send(query);
        return Ok(complaints);
    }

    [Authorize(Roles = nameof(UserRoles.Employee)+","+ nameof(UserRoles.Customer))]
    [HttpGet("{id}")]
    public async Task<ActionResult<ComplaintResponseModel>> GetComplaintByIdAsync([FromRoute]int id)
    {
        _logger.LogInformation("Received request to get complaint with Id: {complaintId}", id);
        var query = new GetComplaintByIdQuery(id);
        var complaint = await _mediator.Send(query);
        _logger.LogInformation("Successfully returned complaint with Id: {complaintId}", id);
        return Ok(complaint);
    }

    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpPost("create-complaint")]
    public async Task<IActionResult> AddComplaintAsync([FromBody]CreateComplaintCommand request)
    {
        _logger.LogInformation("Received request to create complaint to queueId: {id}", request.QueueId);
        var complaint = await _mediator.Send(request);
        _logger.LogInformation("Successfully created complaint with Id: {complaintId}", complaint.Id);
        return Ok(complaint);
    }

    [Authorize(Roles = nameof(UserRoles.Employee))]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComplaintStatusAsync([FromRoute] int id, [FromBody] UpdateComplaintStatusRequest request)
    {
        _logger.LogInformation("Received complaint to update with Id: {complaintId}", id);
        var command = new UpdateComplaintStatusCommand(id, request.ComplaintStatus, request.ResponseText);
        var complaint = await _mediator.Send(command);
        _logger.LogInformation("Successfully updated complaint with Id: {complaintId}", id);
        return Ok(complaint);
    }
    
    [Authorize(Roles = nameof(UserRoles.Customer))]
    [HttpGet("complaint-history/customer/")]
    public async Task<ActionResult<PagedResponse<ComplaintResponseModel>>> GetComplaintsByCustomerAsync([FromQuery] int pageNumber=1)
    {
        _logger.LogInformation("Received request to get customer complaint history with PageNumber: {pageNumber}", pageNumber);
        var query = new GetComplaintsByCustomerQuery(pageNumber);
        var queue = await _mediator.Send(query);
        _logger.LogInformation("Successfully returned complaints.");
        return Ok(queue);
    }
}