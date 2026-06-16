using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Queues.Commands.CancelQueueByEmployee;

public record CancelQueueByEmployeeCommand(int QueueId, string? CancelReason): IRequest<QueueResponseModel>;