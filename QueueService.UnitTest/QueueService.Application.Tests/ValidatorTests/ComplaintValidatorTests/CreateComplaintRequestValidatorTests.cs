using FluentValidation.TestHelper;
using QApplication.UseCases.Complaints.Commands.CreateComplaint;
using QApplication.Validators.ComplaintValidators;

namespace QueueService.UnitTest.QueueService.Application.Tests.ValidatorTests.ComplaintValidatorTests;

public class CreateComplaintRequestValidatorTests
{
    private readonly CreateComplaintRequestValidator _validator;

    public CreateComplaintRequestValidatorTests()
    {
        _validator = new CreateComplaintRequestValidator();
    }


    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        
        //Arrange
        var command = new CreateComplaintCommand(1, "Test Complaint Text");

        
        //Act
        var result = _validator.TestValidate(command);
        
        
        //Assert
        
        result.ShouldNotHaveAnyValidationErrors();


    }


    [Fact]
    public async Task Validator_When_QueueId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateComplaintCommand(0, "Test Complaint Text");

        
        //Act
        var result = _validator.TestValidate(command);
        
        
        //Assert

        result.ShouldHaveValidationErrorFor(s => s.QueueId)
            .WithErrorMessage("QueueId must be greater than 0");
    }
    
    [Fact]
    public async Task Validator_When_ComplaintText_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateComplaintCommand(1, "");

        
        //Act
        var result = _validator.TestValidate(command);
        
        
        //Assert

        result.ShouldHaveValidationErrorFor(s => s.ComplaintText)
            .WithErrorMessage("Complaint text is required.");
    }
    
    [Fact]
    public async Task Validator_When_ComplaintText_Is_Shorter_Than_10_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateComplaintCommand(1, "test");

        
        //Act
        var result = _validator.TestValidate(command);
        
        
        //Assert

        result.ShouldHaveValidationErrorFor(s => s.ComplaintText)
            .WithErrorMessage("Complaint text must be at least 10 characters");
    }
    
    
}