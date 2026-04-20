using FluentAssertions;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Tests.Entities;

public class OptionTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateOption()
    {
        
        var questionId = Guid.NewGuid();
        var text = "Option A";
        var order = 1;

        
        var option = new Option(questionId, text, order);

        
        option.QuestionId.Should().Be(questionId);
        option.Text.Should().Be(text);
        option.Order.Should().Be(order);
        option.Id.Should().NotBeEmpty();
        option.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidText_ShouldThrowDomainException(string? invalidText)
    {
        
        var questionId = Guid.NewGuid();

        
        var act = () => new Option(questionId, invalidText!, 1);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot be empty.");
    }

    [Fact]
    public void Constructor_WithTextExceeding200Characters_ShouldThrowDomainException()
    {
        
        var questionId = Guid.NewGuid();
        var longText = new string('a', 201);

        
        var act = () => new Option(questionId, longText, 1);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot exceed 200 characters.");
    }

    [Fact]
    public void SetText_WithValidText_ShouldUpdateText()
    {
        
        var option = new Option(Guid.NewGuid(), "Original text", 1);
        var newText = "Updated text";

        
        option.SetText(newText);

        
        option.Text.Should().Be(newText);
        option.UpdatedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void SetText_WithInvalidText_ShouldThrowDomainException(string? invalidText)
    {
        
        var option = new Option(Guid.NewGuid(), "Valid text", 1);

        
        var act = () => option.SetText(invalidText!);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot be empty.");
    }

    [Fact]
    public void SetText_WithTextExceeding200Characters_ShouldThrowDomainException()
    {
        
        var option = new Option(Guid.NewGuid(), "Valid text", 1);
        var longText = new string('b', 201);

        
        var act = () => option.SetText(longText);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Option text cannot exceed 200 characters.");
    }

    [Fact]
    public void SetOrder_WithValidOrder_ShouldUpdateOrder()
    {
        
        var option = new Option(Guid.NewGuid(), "Test option", 1);

        
        option.SetOrder(5);

        
        option.Order.Should().Be(5);
        option.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetOrder_WithZero_ShouldUpdateOrder()
    {
        
        var option = new Option(Guid.NewGuid(), "Test option", 1);

        
        option.SetOrder(0);

        
        option.Order.Should().Be(0);
    }

    [Fact]
    public void SetOrder_WithNegativeOrder_ShouldThrowDomainException()
    {
        
        var option = new Option(Guid.NewGuid(), "Test option", 1);

        
        var act = () => option.SetOrder(-1);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Option order cannot be negative.");
    }

    [Fact]
    public void SetText_WithExactly200Characters_ShouldSucceed()
    {
        
        var option = new Option(Guid.NewGuid(), "Test", 1);
        var text200Chars = new string('a', 200);

        
        option.SetText(text200Chars);

        
        option.Text.Should().Be(text200Chars);
    }
}
