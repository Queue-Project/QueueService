using MediatR;
using Microsoft.Extensions.Logging;
using QApplication.Interfaces;
using QApplication.Responses;
using QContracts.QueueEvents;
using QContracts.QueueEvents.Enums;
using QDomain.Enums;

namespace QApplication.UseCases.Queues.Commands.CancelQueueByEmployee;

public class CancelQueueByEmployeeCommandHandler : IRequestHandler<CancelQueueByEmployeeCommand, QueueResponseModel>
{
    private readonly ILogger<CancelQueueByEmployeeCommandHandler> _logger;
    private readonly IQueueCancellationService _cancellationService;


    public CancelQueueByEmployeeCommandHandler(ILogger<CancelQueueByEmployeeCommandHandler> logger,
        IQueueCancellationService cancellationService)
    {
        _logger = logger;
        _cancellationService = cancellationService;
    }

    public async Task<QueueResponseModel> Handle(CancelQueueByEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling queue Id {id} by employee", request.QueueId);

        var dbQueue = await _cancellationService.GetAndValidateQueueForCancellation(request.QueueId, cancellationToken);


        var response = await _cancellationService.ProcessCancellation(dbQueue, QueueStatus.CancelledByEmployee,
            request.CancelReason,
            UpdatedQueueStatus.CanceledByEmployee, cancellationToken);
        
        _logger.LogInformation("Successfully cancelled queue Id {id} by employee", request.QueueId);
        return response;
    }
}