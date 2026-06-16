using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Queues.Commands.CancelQueueByCustomer;

public record CancelQueueByCustomerCommand(int QueueId, string? CancelReason) : IRequest<QueueResponseModel>;
