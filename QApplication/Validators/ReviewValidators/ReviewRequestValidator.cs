using FluentValidation;
using QApplication.UseCases.Reviews.Commands.CreateReview;

namespace QApplication.Validators.ReviewValidators;

public class ReviewRequestValidator: AbstractValidator<CreateReviewCommand>
{
    public ReviewRequestValidator()
    {
        RuleFor(x => x.QueueId)
            .GreaterThan(0).WithMessage("QueueId must be greater than 0");

        RuleFor(x => x.Grade)
            .InclusiveBetween(0, 5).WithMessage("Grade must be between 1 to 5");

        RuleFor(x => x.ReviewText)
            .MinimumLength(2).WithMessage("Review text must be at least 2 characters")
            .MaximumLength(500).WithMessage("Review text must be at most 500 characters");
        
    }
}