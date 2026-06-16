using FluentValidation;
using QApplication.UseCases.Queues.Commands.CreateQueue;

namespace QApplication.Validators.QueueValidators;

public class CreateQueueRequestValidator: AbstractValidator<CreateQueueCommand>
{
    public CreateQueueRequestValidator()
    {

        RuleFor(x => x.EmployeeId)
            .GreaterThan(0).WithMessage("EmployeeId must be greater than 0");

        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("ServiceId must be greater than 0");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Start time is required")
            .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(30))
            .WithMessage("Start time cannot be more than 30 days in advance.");
    }
}