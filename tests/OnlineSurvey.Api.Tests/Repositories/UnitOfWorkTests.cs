using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Infrastructure.Data;
using OnlineSurvey.Infrastructure.Repositories;

namespace OnlineSurvey.Api.Tests.Repositories;

public class UnitOfWorkTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Surveys_ShouldReturnSurveyRepository()
    {
        
        var repository = _unitOfWork.Surveys;

        
        repository.Should().NotBeNull();
        repository.Should().BeOfType<SurveyRepository>();
    }

    [Fact]
    public void Surveys_CalledMultipleTimes_ShouldReturnSameInstance()
    {
        
        var repository1 = _unitOfWork.Surveys;
        var repository2 = _unitOfWork.Surveys;

        
        repository1.Should().BeSameAs(repository2);
    }

    [Fact]
    public void Responses_ShouldReturnResponseRepository()
    {
        
        var repository = _unitOfWork.Responses;

        
        repository.Should().NotBeNull();
        repository.Should().BeOfType<ResponseRepository>();
    }

    [Fact]
    public void Responses_CalledMultipleTimes_ShouldReturnSameInstance()
    {
        
        var repository1 = _unitOfWork.Responses;
        var repository2 = _unitOfWork.Responses;

        
        repository1.Should().BeSameAs(repository2);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        
        var survey = new Survey("Test Survey");
        await _unitOfWork.Surveys.AddAsync(survey);

        
        var result = await _unitOfWork.SaveChangesAsync();

        
        result.Should().BeGreaterThan(0);
        var savedSurvey = await _context.Surveys.FindAsync(survey.Id);
        savedSurvey.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WithNoChanges_ShouldReturnZero()
    {
        
        var result = await _unitOfWork.SaveChangesAsync();

        
        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_WithoutTransaction_ShouldNotThrow()
    {
        
        var act = async () => await _unitOfWork.CommitAsync();

        
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RollbackAsync_WithoutTransaction_ShouldNotThrow()
    {
        
        var act = async () => await _unitOfWork.RollbackAsync();

        
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        var unitOfWork = new UnitOfWork(context);

        
        var act = () => unitOfWork.Dispose();

        
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        var unitOfWork = new UnitOfWork(context);

        
        var act = () =>
        {
            unitOfWork.Dispose();
            unitOfWork.Dispose();
        };

        
        act.Should().NotThrow();
    }

    [Fact]
    public async Task FullWorkflow_CreateSurveyAndResponse_ShouldWork()
    {
        var survey = new Survey("Integration Test Survey");
        var question = new Question(survey.Id, "Test question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate();

        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        // Create Response
        var response = new Response(survey.Id, "participant1", "127.0.0.1");
        response.AddAnswer(new Answer(response.Id, question.Id, question.Options.First().Id));

        await _unitOfWork.Responses.AddAsync(response);
        await _unitOfWork.SaveChangesAsync();

        
        var savedSurvey = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(survey.Id);
        savedSurvey.Should().NotBeNull();
        savedSurvey!.Questions.Should().HaveCount(1);

        var responseCount = await _unitOfWork.Responses.GetResponseCountBySurveyIdAsync(survey.Id);
        responseCount.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleEntities_ShouldReturnCorrectCount()
    {
        
        await _unitOfWork.Surveys.AddAsync(new Survey("Survey 1"));
        await _unitOfWork.Surveys.AddAsync(new Survey("Survey 2"));
        await _unitOfWork.Surveys.AddAsync(new Survey("Survey 3"));

        
        var result = await _unitOfWork.SaveChangesAsync();

        
        result.Should().Be(3);
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancellationToken_ShouldWork()
    {
        
        var survey = new Survey("Test Survey");
        await _unitOfWork.Surveys.AddAsync(survey);
        var cancellationToken = new CancellationToken();

        
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Surveys_ShouldAllowChainedOperations()
    {
        
        var survey = new Survey("Test Survey");

        
        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();
        survey.SetTitle("Updated Title");
        await _unitOfWork.Surveys.UpdateAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        var result = await _unitOfWork.Surveys.GetByIdAsync(survey.Id);
        result!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task Responses_ShouldAllowChainedOperations()
    {
        
        var survey = new Survey("Test Survey");
        var question = new Question(survey.Id, "Test?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate();

        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        var response1 = new Response(survey.Id, "p1", null);
        var response2 = new Response(survey.Id, "p2", null);
        await _unitOfWork.Responses.AddAsync(response1);
        await _unitOfWork.Responses.AddAsync(response2);
        await _unitOfWork.SaveChangesAsync();

        
        var count = await _unitOfWork.Responses.GetResponseCountBySurveyIdAsync(survey.Id);
        count.Should().Be(2);
    }
}

public class UnitOfWorkTransactionTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTransactionTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldStartTransaction()
    {
        
        await _unitOfWork.BeginTransactionAsync();
        
        var survey = new Survey("Transaction Test");
        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();

        var result = await _context.Surveys.FindAsync(survey.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitAsync_WithTransaction_ShouldPersistChanges()
    {
        
        await _unitOfWork.BeginTransactionAsync();
        var survey = new Survey("Commit Test");
        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        await _unitOfWork.CommitAsync();
        
        using var verifyConnection = new SqliteConnection("DataSource=:memory:");
        verifyConnection.Open();
        var result = await _context.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == survey.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Commit Test");
    }

    [Fact]
    public async Task RollbackAsync_WithTransaction_ShouldRevertChanges()
    {
        
        await _unitOfWork.BeginTransactionAsync();
        var survey = new Survey("Rollback Test");
        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        await _unitOfWork.RollbackAsync();

        
        _context.ChangeTracker.Clear();
        var result = await _context.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == survey.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task BeginTransactionAsync_WithCancellationToken_ShouldWork()
    {
        
        var cancellationToken = new CancellationToken();
        var survey = new Survey("CancellationToken Test");

        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitAsync();

        
        var result = await _context.Surveys.FindAsync(survey.Id);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CommitAsync_WithCancellationToken_ShouldWork()
    {
        
        var cancellationToken = new CancellationToken();
        var survey = new Survey("Commit CancellationToken Test");
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        await _unitOfWork.CommitAsync(cancellationToken);

        
        var result = await _context.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == survey.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Commit CancellationToken Test");
    }

    [Fact]
    public async Task RollbackAsync_WithCancellationToken_ShouldWork()
    {
        
        var cancellationToken = new CancellationToken();
        var survey = new Survey("Rollback CancellationToken Test");
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        await _unitOfWork.RollbackAsync(cancellationToken);

        
        _context.ChangeTracker.Clear();
        var result = await _context.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == survey.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CommitAsync_CalledTwice_ShouldNotThrow()
    {
        
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.CommitAsync();

        var act = async () => await _unitOfWork.CommitAsync();

        
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RollbackAsync_CalledTwice_ShouldNotThrow()
    {
        
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.RollbackAsync();
        
        var act = async () => await _unitOfWork.RollbackAsync();

        
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task FullTransactionWorkflow_CommitSurveyWithQuestions_ShouldPersist()
    {
        
        await _unitOfWork.BeginTransactionAsync();

        var survey = new Survey("Full Workflow Test", "Description");
        var question = new Question(survey.Id, "Question 1?", 1, true);
        question.AddOption(new Option(question.Id, "Option A", 1));
        question.AddOption(new Option(question.Id, "Option B", 2));
        survey.AddQuestion(question);

        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        await _unitOfWork.CommitAsync();

        
        var result = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(survey.Id);
        result.Should().NotBeNull();
        result!.Questions.Should().HaveCount(1);
        result.Questions.First().Options.Should().HaveCount(2);
    }

    [Fact]
    public async Task FullTransactionWorkflow_RollbackSurveyWithQuestions_ShouldRevert()
    {
        
        await _unitOfWork.BeginTransactionAsync();

        var survey = new Survey("Rollback Workflow Test");
        var question = new Question(survey.Id, "Question?", 1);
        question.AddOption(new Option(question.Id, "Option", 1));
        survey.AddQuestion(question);

        await _unitOfWork.Surveys.AddAsync(survey);
        await _unitOfWork.SaveChangesAsync();

        
        await _unitOfWork.RollbackAsync();

        
        _context.ChangeTracker.Clear();
        var result = await _context.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == survey.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Dispose_WithActiveTransaction_ShouldNotThrow()
    {
        
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        var unitOfWork = new UnitOfWork(context);

        await unitOfWork.BeginTransactionAsync();

        
        var act = () =>
        {
            unitOfWork.Dispose();
            connection.Close();
            connection.Dispose();
        };

        
        act.Should().NotThrow();
    }
}
