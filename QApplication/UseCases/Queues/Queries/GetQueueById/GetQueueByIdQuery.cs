using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Queues.Queries.GetQueueById;

public record GetQueueByIdQuery(int Id): IRequest<QueueResponseModel>;