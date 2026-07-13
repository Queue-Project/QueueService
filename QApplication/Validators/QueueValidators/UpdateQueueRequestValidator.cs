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
        
    }
}