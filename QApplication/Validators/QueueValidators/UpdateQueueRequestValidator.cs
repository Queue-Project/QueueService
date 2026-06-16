using FluentValidation;
using QApplication.UseCases.Queues.Commands.UpdateQueueStatus;
using QDomain.Enums;

namespace QApplication.Validators.QueueValidators;

public class UpdateQueueRequestValidator: AbstractValidator<UpdateQueueStatusCommand>
{
    public UpdateQueueRequestValidator()
    {
        RuleFor(x => x.QueueId)
            .GreaterThan(0).WithMessage("QueueId must be greater than 0");

        RuleFor(x => x.newStatus)
            .IsInEnum().WithMessage("Invalid queue status");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("End time is required when completing a queue.")
            .When(x => x.newStatus == QueueStatus.Confirmed);
    }
}