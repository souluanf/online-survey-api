using FluentAssertions;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Enums;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Tests.Entities;

public class SurveyTests
{
    [Fact]
    public void Constructor_WithValidTitle_ShouldCreateSurvey()
    {
        
        var title = "Election Poll 2024";
        var description = "Public opinion poll";

        
        var survey = new Survey(title, description);

        
        survey.Title.Should().Be(title);
        survey.Description.Should().Be(description);
        survey.Status.Should().Be(SurveyStatus.Draft);
        survey.Id.Should().NotBeEmpty();
        survey.Questions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTitle_ShouldThrowDomainException(string? invalidTitle)
    {
        
        var act = () => new Survey(invalidTitle!);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Survey title cannot be empty.");
    }

    [Fact]
    public void Constructor_WithTitleExceeding200Characters_ShouldThrowDomainException()
    {
        
        var longTitle = new string('a', 201);

        
        var act = () => new Survey(longTitle);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Survey title cannot exceed 200 characters.");
    }

    [Fact]
    public void AddQuestion_ToDraftSurvey_ShouldAddQuestion()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "What is your favorite color?", 1);
        question.AddOption(new Option(question.Id, "Red", 1));
        question.AddOption(new Option(question.Id, "Blue", 2));

        
        survey.AddQuestion(question);

        
        survey.Questions.Should().ContainSingle();
        survey.Questions.First().Should().Be(question);
    }

    [Fact]
    public void Activate_WithValidQuestions_ShouldChangeStatusToActive()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);

        
        survey.Activate();

        
        survey.Status.Should().Be(SurveyStatus.Active);
        survey.StartDate.Should().NotBeNull();
        survey.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Activate_WithoutQuestions_ShouldThrowDomainException()
    {
        
        var survey = new Survey("Test Survey");

        
        var act = () => survey.Activate();

        
        act.Should().Throw<DomainException>()
            .WithMessage("Survey must have at least one question to be activated.");
    }

    [Fact]
    public void Close_ActiveSurvey_ShouldChangeStatusToClosed()
    {
        
        var survey = CreateActiveSurvey();

        
        survey.Close();

        
        survey.Status.Should().Be(SurveyStatus.Closed);
        survey.EndDate.Should().NotBeNull();
        survey.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Close_DraftSurvey_ShouldThrowDomainException()
    {
        
        var survey = new Survey("Test Survey");

        
        var act = () => survey.Close();

        
        act.Should().Throw<DomainException>()
            .WithMessage("Only active surveys can be closed.");
    }

    [Fact]
    public void SetDescription_WithValidDescription_ShouldUpdateDescription()
    {
        
        var survey = new Survey("Test Survey");
        var description = "This is a test description";

        
        survey.SetDescription(description);

        
        survey.Description.Should().Be(description);
        survey.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetDescription_WithNull_ShouldSetToNull()
    {
        
        var survey = new Survey("Test Survey", "Initial description");

        
        survey.SetDescription(null);

        
        survey.Description.Should().BeNull();
    }

    [Fact]
    public void SetDescription_WithDescriptionExceeding1000Characters_ShouldThrowDomainException()
    {
        
        var survey = new Survey("Test Survey");
        var longDescription = new string('a', 1001);

        
        var act = () => survey.SetDescription(longDescription);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Survey description cannot exceed 1000 characters.");
    }

    [Fact]
    public void SetTitle_WithValidTitle_ShouldUpdateTitle()
    {
        
        var survey = new Survey("Original Title");
        var newTitle = "Updated Title";

        
        survey.SetTitle(newTitle);

        
        survey.Title.Should().Be(newTitle);
        survey.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_WithQuestionHavingLessThanTwoOptions_ShouldThrowDomainException()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Only one option", 1));
        survey.AddQuestion(question);

        
        var act = () => survey.Activate();

        
        act.Should().Throw<DomainException>()
            .WithMessage("All questions must have at least 2 options.");
    }

    [Fact]
    public void Activate_AlreadyActiveSurvey_ShouldThrowDomainException()
    {
        
        var survey = CreateActiveSurvey();

        
        var act = () => survey.Activate();

        
        act.Should().Throw<DomainException>()
            .WithMessage("Only draft surveys can be activated.");
    }

    [Fact]
    public void Activate_WithCustomDates_ShouldSetDates()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);

        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);

        
        survey.Activate(startDate, endDate);

        
        survey.StartDate.Should().Be(startDate);
        survey.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void AddQuestion_ToActiveSurvey_ShouldThrowDomainException()
    {
        
        var survey = CreateActiveSurvey();
        var question = new Question(survey.Id, "Another question?", 2);

        
        var act = () => survey.AddQuestion(question);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot add questions to a non-draft survey.");
    }

    [Fact]
    public void AddQuestion_Exceeding50Questions_ShouldThrowDomainException()
    {
        
        var survey = new Survey("Test Survey");
        for (int i = 1; i <= 50; i++)
        {
            var question = new Question(survey.Id, $"Question {i}?", i);
            question.AddOption(new Option(question.Id, "Option 1", 1));
            question.AddOption(new Option(question.Id, "Option 2", 2));
            survey.AddQuestion(question);
        }

        var extraQuestion = new Question(survey.Id, "Question 51?", 51);

        
        var act = () => survey.AddQuestion(extraQuestion);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Survey cannot have more than 50 questions.");
    }

    [Fact]
    public void RemoveQuestion_WithValidId_ShouldRemoveQuestion()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);

        
        survey.RemoveQuestion(question.Id);

        
        survey.Questions.Should().BeEmpty();
    }

    [Fact]
    public void RemoveQuestion_WithInvalidId_ShouldThrowDomainException()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        survey.AddQuestion(question);

        
        var act = () => survey.RemoveQuestion(Guid.NewGuid());

        
        act.Should().Throw<DomainException>()
            .WithMessage("Question not found.");
    }

    [Fact]
    public void RemoveQuestion_FromActiveSurvey_ShouldThrowDomainException()
    {
        
        var survey = CreateActiveSurvey();
        var questionId = survey.Questions.First().Id;

        
        var act = () => survey.RemoveQuestion(questionId);

        
        act.Should().Throw<DomainException>()
            .WithMessage("Cannot remove questions from a non-draft survey.");
    }

    [Fact]
    public void IsOpen_WhenActiveAndNoEndDate_ShouldReturnTrue()
    {
        
        var survey = CreateActiveSurvey();

         
        survey.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void IsOpen_WhenActiveAndEndDateInFuture_ShouldReturnTrue()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate(null, DateTime.UtcNow.AddDays(30));

         
        survey.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void IsOpen_WhenClosed_ShouldReturnFalse()
    {
        
        var survey = CreateActiveSurvey();
        survey.Close();

         
        survey.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void IsOpen_WhenDraft_ShouldReturnFalse()
    {
        
        var survey = new Survey("Test Survey");

         
        survey.IsOpen.Should().BeFalse();
    }

    private static Survey CreateActiveSurvey()
    {
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate();
        return survey;
    }
}
