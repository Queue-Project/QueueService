using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Reviews.Queries.GetReviewById;

public record GetReviewByIdQuery(int Id): IRequest<ReviewResponseModel>;