using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Reviews.Commands.CreateReview;

public record CreateReviewCommand(int QueueId, int Grade, string? ReviewText): IRequest<ReviewResponseModel>;