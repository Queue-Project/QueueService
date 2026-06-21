using FluentValidation.TestHelper;
using QApplication.UseCases.Queues.Commands.CreateQueue;
using QApplication.Validators.QueueValidators;

namespace QueueService.UnitTest.QueueService.Application.Tests.ValidatorTests.QueueValidatorTests;

public class CreateQueueRequestValidatorTests
{
    private readonly CreateQueueRequestValidator _validator;

    public CreateQueueRequestValidatorTests()
    {
        _validator = new CreateQueueRequestValidator();
    }


    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 1, 1, 1, DateTimeOffset.UtcNow);


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_CompanyId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateQueueCommand(0, 1, 1, 1, DateTimeOffset.UtcNow);


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.CompanyId)
            .WithErrorMessage("CompanyId must be greater than 0");
    }
    
    [Fact]
    public async Task Validator_When_BranchId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 0, 1, 1, DateTimeOffset.UtcNow);


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.BranchId)
            .WithErrorMessage("BranchId must be greater than 0");
    }

    
    [Fact]
    public async Task Validator_When_ServiceId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 1, 1, 0, DateTimeOffset.UtcNow);


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.ServiceId)
            .WithErrorMessage("ServiceId must be greater than 0");
    }
    
    [Fact]
    public async Task Validator_When_EmployeeId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 1, 0, 1, DateTimeOffset.UtcNow);


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.EmployeeId)
            .WithErrorMessage("EmployeeId must be greater than 0");
    }
    
    [Fact]
    public async Task Validator_When_StartTime_Is_Empty_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateQueueCommand(1, 1, 0, 1, new DateTimeOffset());



        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.StartTime)
            .WithErrorMessage("Start time is required");
    }
    
}