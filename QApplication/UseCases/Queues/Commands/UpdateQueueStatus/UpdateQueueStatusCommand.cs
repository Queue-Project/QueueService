using MediatR;
using QApplication.Responses;
using QDomain.Enums;

namespace QApplication.UseCases.Queues.Commands.UpdateQueueStatus;

public record UpdateQueueStatusCommand(int QueueId, QueueStatus newStatus): IRequest<UpdateQueueStatusResponseModel>;