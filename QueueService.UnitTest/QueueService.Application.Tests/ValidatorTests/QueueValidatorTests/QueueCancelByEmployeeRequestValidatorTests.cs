using FluentValidation.TestHelper;
using QApplication.UseCases.Queues.Commands.CancelQueueByEmployee;
using QApplication.Validators.QueueValidators;

namespace QueueService.UnitTest.QueueService.Application.Tests.ValidatorTests.QueueValidatorTests;

public class QueueCancelByEmployeeRequestValidatorTests
{
    private readonly QueueCancelByEmployeeRequestValidator _validator;

    public QueueCancelByEmployeeRequestValidatorTests()
    {
        _validator = new QueueCancelByEmployeeRequestValidator();
    }


    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CancelQueueByEmployeeCommand(1, "Test Cancel Reason Test Cancel Reason");


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_QueueId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CancelQueueByEmployeeCommand(0, "Test Cancel Reason Test Cancel Reason");

        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.QueueId)
            .WithErrorMessage("QueueId must be greater than 0.");
    }
    
    [Fact]
    public async Task Validator_When_CancelReason_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CancelQueueByEmployeeCommand(1, "");

        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.CancelReason)
            .WithErrorMessage("Cancel reason is required when cancelling a queue.");
    }
    
    [Fact]
    public async Task Validator_When_CancelReason_Is_Shorter_Than_20_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CancelQueueByEmployeeCommand(1, "Test");

        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.CancelReason)
            .WithErrorMessage("Cancel reason must be at least 20 characters");
    }
}