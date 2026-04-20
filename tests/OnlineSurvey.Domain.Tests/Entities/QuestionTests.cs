using FluentAssertions;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Tests.Entities;

public class QuestionTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateQuestion()
    {
        
        var surveyId = Guid.NewGuid();
        var text = "What is your favorite color?";
        var order = 1;

        
        var question = new Question(surveyId, text, order);

        
        question.SurveyId.Should().Be(surveyId);
        question.Text.Should().Be(text);
        question.Order.Should().Be(order);
        question.IsRequired.Should().BeTrue();
        question.Options.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidText_ShouldThrowDomainException(string? invalidText)
    {
        
        var act = () => new Question(Guid.NewGuid(), invalidText!, 1);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Question text cannot be empty.");
    }

    [Fact]
    public void AddOption_WithValidOption_ShouldAddToCollection()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        var option = new Option(question.Id, "Option 1", 1);

        
        question.AddOption(option);

        
        question.Options.Should().ContainSingle();
        question.Options.First().Should().Be(option);
    }

    [Fact]
    public void AddOption_WithDuplicateText_ShouldThrowDomainException()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));

        
        var act = () => question.AddOption(new Option(question.Id, "Option 1", 2));

        
        act.Should().Throw<DomainException>()
            .WithMessage("Duplicate option text is not allowed.");
    }

    [Fact]
    public void AddOption_ExceedingMaxOptions_ShouldThrowDomainException()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        for (int i = 1; i <= 10; i++)
        {
            question.AddOption(new Option(question.Id, $"Option {i}", i));
        }

        
        var act = () => question.AddOption(new Option(question.Id, "Option 11", 11));

        
        act.Should().Throw<DomainException>()
            .WithMessage("Question cannot have more than 10 options.");
    }

    [Fact]
    public void Constructor_WithTextExceeding500Characters_ShouldThrowDomainException()
    {
        
        var longText = new string('a', 501);

        
        var act = () => new Question(Guid.NewGuid(), longText, 1);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Question text cannot exceed 500 characters.");
    }

    [Fact]
    public void Constructor_WithIsRequiredFalse_ShouldCreateOptionalQuestion()
    {
        var question = new Question(Guid.NewGuid(), "Optional question?", 1, false);

        
        question.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void SetText_WithValidText_ShouldUpdateText()
    {
        
        var question = new Question(Guid.NewGuid(), "Original text?", 1);
        var newText = "Updated text?";

        
        question.SetText(newText);

        
        question.Text.Should().Be(newText);
        question.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetText_WithTextExceeding500Characters_ShouldThrowDomainException()
    {
        
        var question = new Question(Guid.NewGuid(), "Original text?", 1);
        var longText = new string('a', 501);

        
        var act = () => question.SetText(longText);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Question text cannot exceed 500 characters.");
    }

    [Fact]
    public void SetOrder_WithValidOrder_ShouldUpdateOrder()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);

        
        question.SetOrder(5);

        
        question.Order.Should().Be(5);
        question.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetOrder_WithZero_ShouldUpdateOrder()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);

        
        question.SetOrder(0);

        
        question.Order.Should().Be(0);
    }

    [Fact]
    public void SetOrder_WithNegativeOrder_ShouldThrowDomainException()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);

        
        var act = () => question.SetOrder(-1);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Question order cannot be negative.");
    }

    [Fact]
    public void RemoveOption_WithValidId_ShouldRemoveOption()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        var option = new Option(question.Id, "Option 1", 1);
        question.AddOption(option);

        
        question.RemoveOption(option.Id);

        
        question.Options.Should().BeEmpty();
    }

    [Fact]
    public void RemoveOption_WithInvalidId_ShouldThrowDomainException()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));

        
        var act = () => question.RemoveOption(Guid.NewGuid());

        
        act.Should().Throw<DomainException>()
            .WithMessage("Option not found.");
    }

    [Fact]
    public void AddOption_WithDuplicateTextCaseInsensitive_ShouldThrowDomainException()
    {
        
        var question = new Question(Guid.NewGuid(), "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));

        
        var act = () => question.AddOption(new Option(question.Id, "OPTION 1", 2));

        
        act.Should().Throw<DomainException>()
            .WithMessage("Duplicate option text is not allowed.");
    }
}
