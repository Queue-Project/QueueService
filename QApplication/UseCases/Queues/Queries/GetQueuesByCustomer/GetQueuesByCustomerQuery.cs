using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Queues.Queries.GetQueuesByCustomer;

public record GetQueuesByCustomerQuery(int PageNumber): IRequest<PagedResponse<QueueResponseModel>>;
