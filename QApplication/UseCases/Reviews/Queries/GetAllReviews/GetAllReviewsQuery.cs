using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Reviews.Queries.GetAllReviews;

public record GetAllReviewsQuery(int PageNumber): IRequest<PagedResponse<ReviewResponseModel>>;