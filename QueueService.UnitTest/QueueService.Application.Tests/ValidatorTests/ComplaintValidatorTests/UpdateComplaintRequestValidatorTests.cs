using FluentValidation.TestHelper;
using QApplication.Requests.ComplaintRequest;
using QApplication.Validators.ComplaintValidators;
using QDomain.Enums;

namespace QueueService.UnitTest.QueueService.Application.Tests.ValidatorTests.ComplaintValidatorTests;

public class UpdateComplaintRequestValidatorTests
{
    private readonly UpdateComplaintRequestValidator _validator;

    public UpdateComplaintRequestValidatorTests()
    {
        _validator = new UpdateComplaintRequestValidator();
    }


    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        
        //Arrange
        var command = new UpdateComplaintStatusRequest
        {
            ComplaintStatus = ComplaintStatus.Reviewed,
            ResponseText = "Test Response Text"
        };

        
        //Act
        var result = _validator.TestValidate(command);
        
        
        //Assert
        
        result.ShouldNotHaveAnyValidationErrors();


    }

    

    
    [Fact]
    public async Task Validator_When_Response_Is_Shorter_Than_10_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateComplaintStatusRequest
        {
            ComplaintStatus = ComplaintStatus.Reviewed,
            ResponseText = "Test"
        };

        
        //Act
        var result = _validator.TestValidate(command);
        
        
        //Assert

        result.ShouldHaveValidationErrorFor(s => s.ResponseText)
            .WithErrorMessage("Response text must be at least 10 characters");
    }
}