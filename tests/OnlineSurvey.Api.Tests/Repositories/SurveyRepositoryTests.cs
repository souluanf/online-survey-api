using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Enums;
using OnlineSurvey.Infrastructure.Data;
using OnlineSurvey.Infrastructure.Repositories;

namespace OnlineSurvey.Api.Tests.Repositories;

public class SurveyRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly SurveyRepository _repository;

    public SurveyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new SurveyRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnSurvey()
    {
        
        var survey = new Survey("Test Survey", "Description");
        await _repository.AddAsync(survey);
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetByIdAsync(survey.Id);

        
        result.Should().NotBeNull();
        result!.Id.Should().Be(survey.Id);
        result.Title.Should().Be("Test Survey");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllSurveys()
    {
        
        await _repository.AddAsync(new Survey("Survey 1"));
        await _repository.AddAsync(new Survey("Survey 2"));
        await _repository.AddAsync(new Survey("Survey 3"));
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetAllAsync();

        
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddAsync_ShouldAddSurvey()
    {
        
        var survey = new Survey("New Survey");

        
        await _repository.AddAsync(survey);
        await _context.SaveChangesAsync();

        
        var result = await _context.Surveys.FindAsync(survey.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSurvey()
    {
        
        var survey = new Survey("Original Title");
        await _repository.AddAsync(survey);
        await _context.SaveChangesAsync();

        
        survey.SetTitle("Updated Title");
        await _repository.UpdateAsync(survey);
        await _context.SaveChangesAsync();

        
        var result = await _context.Surveys.FindAsync(survey.Id);
        result!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveSurvey()
    {
        
        var survey = new Survey("To Delete");
        await _repository.AddAsync(survey);
        await _context.SaveChangesAsync();

        
        await _repository.DeleteAsync(survey);
        await _context.SaveChangesAsync();

        
        var result = await _context.Surveys.FindAsync(survey.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithQuestionsAsync_ShouldIncludeQuestions()
    {
        
        var survey = new Survey("Survey with Questions");
        var question1 = new Question(survey.Id, "Question 1?", 1);
        var question2 = new Question(survey.Id, "Question 2?", 2);
        question1.AddOption(new Option(question1.Id, "Option 1", 1));
        question1.AddOption(new Option(question1.Id, "Option 2", 2));
        question2.AddOption(new Option(question2.Id, "Option A", 1));
        question2.AddOption(new Option(question2.Id, "Option B", 2));
        survey.AddQuestion(question1);
        survey.AddQuestion(question2);

        await _repository.AddAsync(survey);
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetByIdWithQuestionsAsync(survey.Id);

        
        result.Should().NotBeNull();
        result!.Questions.Should().HaveCount(2);
        result.Questions.First().Order.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdWithQuestionsAndOptionsAsync_ShouldIncludeQuestionsAndOptions()
    {
        
        var survey = new Survey("Survey with Questions and Options");
        var question = new Question(survey.Id, "Question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);

        await _repository.AddAsync(survey);
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetByIdWithQuestionsAndOptionsAsync(survey.Id);

        
        result.Should().NotBeNull();
        result!.Questions.Should().HaveCount(1);
        result.Questions.First().Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnSurveysWithMatchingStatus()
    {
        
        var draftSurvey = new Survey("Draft Survey");
        var activeSurvey = CreateActiveSurvey("Active Survey");

        await _repository.AddAsync(draftSurvey);
        await _repository.AddAsync(activeSurvey);
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetByStatusAsync(SurveyStatus.Active);

        
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Active Survey");
    }

    [Fact]
    public async Task GetActiveSurveysAsync_ShouldReturnOnlyActiveSurveys()
    {
        
        var draftSurvey = new Survey("Draft Survey");
        var activeSurvey = CreateActiveSurvey("Active Survey");

        await _repository.AddAsync(draftSurvey);
        await _repository.AddAsync(activeSurvey);
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetActiveSurveysAsync();

        
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(SurveyStatus.Active);
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldReturnPaginatedResults()
    {
        
        for (int i = 1; i <= 15; i++)
        {
            await _repository.AddAsync(new Survey($"Survey {i}"));
        }
        await _context.SaveChangesAsync();

        
        var (surveys, totalCount) = await _repository.GetPaginatedAsync(page: 1, pageSize: 10);

        
        surveys.Should().HaveCount(10);
        totalCount.Should().Be(15);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithStatusFilter_ShouldFilterByStatus()
    {
        
        await _repository.AddAsync(new Survey("Draft 1"));
        await _repository.AddAsync(new Survey("Draft 2"));
        await _repository.AddAsync(CreateActiveSurvey("Active 1"));
        await _context.SaveChangesAsync();

        
        var (surveys, totalCount) = await _repository.GetPaginatedAsync(
            page: 1,
            pageSize: 10,
            status: SurveyStatus.Draft);

        
        surveys.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPaginatedAsync_SecondPage_ShouldSkipFirstPageItems()
    {
        
        for (int i = 1; i <= 15; i++)
        {
            await _repository.AddAsync(new Survey($"Survey {i}"));
        }
        await _context.SaveChangesAsync();

        
        var (surveys, totalCount) = await _repository.GetPaginatedAsync(page: 2, pageSize: 10);

        
        surveys.Should().HaveCount(5);
        totalCount.Should().Be(15);
    }

    [Fact]
    public async Task GetByIdWithQuestionsAsync_WhenNotExists_ShouldReturnNull()
    {
        
        var result = await _repository.GetByIdWithQuestionsAsync(Guid.NewGuid());

        
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdWithQuestionsAndOptionsAsync_WhenNotExists_ShouldReturnNull()
    {
        
        var result = await _repository.GetByIdWithQuestionsAndOptionsAsync(Guid.NewGuid());

        
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSurveysAsync_ShouldExcludeExpiredSurveys()
    {
        var survey = new Survey("Expired Survey");
        var question = new Question(survey.Id, "Test?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-1));

        await _repository.AddAsync(survey);
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetActiveSurveysAsync();

        
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByStatusAsync_WithDraftStatus_ShouldReturnOnlyDrafts()
    {
        
        await _repository.AddAsync(new Survey("Draft 1"));
        await _repository.AddAsync(CreateActiveSurvey("Active 1"));
        await _context.SaveChangesAsync();

        
        var result = await _repository.GetByStatusAsync(SurveyStatus.Draft);

        
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(SurveyStatus.Draft);
    }

    [Fact]
    public async Task GetPaginatedAsync_WithNoSurveys_ShouldReturnEmpty()
    {
        
        var (surveys, totalCount) = await _repository.GetPaginatedAsync(page: 1, pageSize: 10);

        
        surveys.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldOrderByCreatedAtDescending()
    {
        
        var survey1 = new Survey("First Created");
        await _repository.AddAsync(survey1);
        await _context.SaveChangesAsync();

        await Task.Delay(10); // Ensure different timestamps

        var survey2 = new Survey("Second Created");
        await _repository.AddAsync(survey2);
        await _context.SaveChangesAsync();

        
        var (surveys, _) = await _repository.GetPaginatedAsync(page: 1, pageSize: 10);

        
        surveys.First().Title.Should().Be("Second Created");
    }

    private static Survey CreateActiveSurvey(string title)
    {
        var survey = new Survey(title);
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate();
        return survey;
    }
}
