using FluentValidation;
using QApplication.Requests.ComplaintRequest;
using QApplication.UseCases.Complaints.Commands.CreateComplaint;

namespace QApplication.Validators.ComplaintValidators;

public class CreateComplaintRequestValidator: AbstractValidator<CreateComplaintCommand>
{
    public CreateComplaintRequestValidator()
    {
        

        RuleFor(x => x.QueueId)
            .GreaterThan(0).WithMessage("QueueId must be greater than 0");
        
        RuleFor(x=>x.ComplaintText)
            .NotEmpty().WithMessage("Complaint text is required.")
            .MinimumLength(10).WithMessage("Complaint text must be at least 10 characters")
            .MaximumLength(500).WithMessage("Complaint text must be at most 500 characters");
    }
}