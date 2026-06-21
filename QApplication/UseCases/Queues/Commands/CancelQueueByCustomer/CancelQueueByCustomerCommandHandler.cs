using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using QApplication.Exceptions;
using QApplication.Interfaces;
using QApplication.Responses;
using QContracts.Events.Enums;
using QDomain.Enums;
using QDomain.Models;


namespace QApplication.UseCases.Queues.Commands.CancelQueueByCustomer;

public class CancelQueueByCustomerCommandHandler : IRequestHandler<CancelQueueByCustomerCommand, QueueResponseModel>
{
    private readonly ILogger<CancelQueueByCustomerCommandHandler> _logger;

    private readonly IQueueCancellationService _cancellationService;

    public CancelQueueByCustomerCommandHandler(ILogger<CancelQueueByCustomerCommandHandler> logger,
        IQueueCancellationService cancellationService)
    {
        _logger = logger;
        _cancellationService = cancellationService;
    }

    public async Task<QueueResponseModel> Handle(CancelQueueByCustomerCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling queue Id {id} by customer", request.QueueId);
        var dbQueue = await _cancellationService.GetAndValidateQueueForCancellation(request.QueueId, cancellationToken);


        _logger.LogDebug("Current queue status: {Status}", dbQueue.Status);
        if (dbQueue.Status != QueueStatus.Pending && dbQueue.Status != QueueStatus.Confirmed)
        {
            _logger.LogWarning("Invalid queue status {Status} for customer cancellation", dbQueue.Status);
            throw new HttpStatusCodeException(HttpStatusCode.BadRequest, nameof(QueueEntity));
        }

        _logger.LogDebug("Time until queue start: {minutes} minutes",
            (dbQueue.StartTime - DateTimeOffset.Now).TotalMinutes);
        if ((dbQueue.StartTime - DateTime.Now).TotalMinutes < 10)
        {
            _logger.LogWarning("Cancellation is than 10 minutes before start time");
            throw new Exception("Cannot cancel less than 10 minutes before start time");
        }


        var response = await _cancellationService.ProcessCancellation(dbQueue, QueueStatus.CancelledByCustomer,
            request.CancelReason,
            UpdatedQueueStatus.CanceledByCustomer, cancellationToken);

        _logger.LogInformation("Successfully cancelled queue Id {id} by customer", request.QueueId);
        return response;
    }
}