using FluentValidation.TestHelper;
using QApplication.UseCases.Reviews.Commands.CreateReview;
using QApplication.Validators.ReviewValidators;

namespace QueueService.UnitTest.QueueService.Application.Tests.ValidatorTests.ReviewValidatorTests;

public class ReviewRequestValidatorTests
{
    private readonly ReviewRequestValidator _validator;

    public ReviewRequestValidatorTests()
    {
        _validator = new ReviewRequestValidator();
    }
    
    [Fact]
    public async Task Validator_When_Commands_Valid_Should_Not_HaveValidationError()
    {
        //Arrange
        var command = new CreateReviewCommand(1, 4, "Good");


        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldNotHaveAnyValidationErrors();
    }


    [Fact]
    public async Task Validator_When_QueueId_Is_Invalid_Number_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateReviewCommand(0, 4, "Good");

        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.QueueId)
            .WithErrorMessage("QueueId must be greater than 0");
    }
    
    [Fact]
    public async Task Validator_When_Grade_Is_Inclusive_Between_1_And_5_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateReviewCommand(1, 6, "Good");

        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.Grade)
            .WithErrorMessage("Grade must be between 1 to 5");
    }
    
    [Fact]
    public async Task Validator_When_ReviewText_Is_Shorter_Than_2_ShouldHaveValidationError()
    {
        //Arrange
        var command = new CreateReviewCommand(1, 5, "A");

        //Act
        var result = _validator.TestValidate(command);


        //Assert

        result.ShouldHaveValidationErrorFor(s => s.ReviewText)
            .WithErrorMessage("Review text must be at least 2 characters");
    }
    
    
}