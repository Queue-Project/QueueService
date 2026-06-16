using FluentValidation;
using QApplication.Requests.ComplaintRequest;

namespace QApplication.Validators.ComplaintValidators;

public class UpdateComplaintRequestValidator: AbstractValidator<UpdateComplaintStatusRequest>
{
    public UpdateComplaintRequestValidator()
    {
        RuleFor(x => x.ComplaintStatus)
            .IsInEnum().WithMessage("Invalid complaint status.");
        
        RuleFor(x=>x.ResponseText)
            .MinimumLength(10).WithMessage("Response text must be at least 10 characters")
            .MaximumLength(500).WithMessage("Response text must be at most 500 characters");
    }
}