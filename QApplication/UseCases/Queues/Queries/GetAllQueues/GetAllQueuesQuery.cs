using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Queues.Queries.GetAllQueues;

public record GetAllQueuesQuery(int PageNumber): IRequest<PagedResponse<QueueResponseModel>>;