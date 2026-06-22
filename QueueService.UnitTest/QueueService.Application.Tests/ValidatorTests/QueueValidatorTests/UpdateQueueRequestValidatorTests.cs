using FluentValidation.TestHelper;
using QApplication.UseCases.Queues.Commands.UpdateQueueStatus;
using QApplication.Validators.QueueValidators;
using QDomain.Enums;

namespace QueueService.UnitTest.QueueService.Application.Tests.ValidatorTests.QueueValidatorTests;

public class UpdateQueueRequestValidatorTests
{
    private readonly UpdateQueueRequestValidator _validator;

    public UpdateQueueRequestValidatorTests()
    {
        _validator = new UpdateQueueRequestValidator();
    }


    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new UpdateQueueStatusCommand(1, QueueStatus.Confirmed, DateTimeOffset.UtcNow.AddMinutes(20));


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_QueueId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new UpdateQueueStatusCommand(0, QueueStatus.Confirmed, DateTimeOffset.UtcNow.AddMinutes(20));

        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.QueueId)
            .WithErrorMessage("QueueId must be greater than 0");
    }

    
}