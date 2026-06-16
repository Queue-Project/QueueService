using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Reviews.Queries.GetAllCustomerReviews;

public record GetReviewsByCustomerQuery(int PageNumber) : IRequest<PagedResponse<ReviewResponseModel>>;
