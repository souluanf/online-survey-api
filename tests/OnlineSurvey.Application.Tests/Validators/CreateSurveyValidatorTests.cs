using FluentAssertions;
using FluentValidation.TestHelper;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Validators;

namespace OnlineSurvey.Application.Tests.Validators;

public class CreateSurveyValidatorTests
{
    private readonly CreateSurveyValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldPass()
    {
        
        var request = CreateValidRequest();

        
        var result = _validator.TestValidate(request);

        
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldFail()
    {
        
        var request = CreateValidRequest() with { Title = "" };

        
        var result = _validator.TestValidate(request);

        
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldFail()
    {
        
        var request = CreateValidRequest() with { Title = new string('a', 201) };

        
        var result = _validator.TestValidate(request);

        
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_WithNoQuestions_ShouldFail()
    {
        
        var request = CreateValidRequest() with { Questions = [] };

        
        var result = _validator.TestValidate(request);

        
        result.ShouldHaveValidationErrorFor(x => x.Questions)
            .WithErrorMessage("At least one question is required.");
    }

    [Fact]
    public void Validate_WithQuestionWithOneOption_ShouldFail()
    {
        
        var request = new CreateSurveyRequest(
            "Test Survey",
            null,
            [
                new CreateQuestionRequest(
                    "Test Question?",
                    1,
                    true,
                    [new CreateOptionRequest("Only Option", 1)]
                )
            ]
        );

        
        var result = _validator.TestValidate(request);

        
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage("At least 2 options are required.");
    }

    private static CreateSurveyRequest CreateValidRequest() => new(
        "Valid Survey Title",
        "Valid description",
        [
            new CreateQuestionRequest(
                "Valid question?",
                1,
                true,
                [
                    new CreateOptionRequest("Option 1", 1),
                    new CreateOptionRequest("Option 2", 2)
                ]
            )
        ]
    );
}
