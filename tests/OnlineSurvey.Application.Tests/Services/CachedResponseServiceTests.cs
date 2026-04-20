using FluentAssertions;
using Moq;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Application.Services;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Repositories;

namespace OnlineSurvey.Application.Tests.Services;

public class CachedResponseServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISurveyRepository> _surveyRepositoryMock;
    private readonly Mock<IResponseRepository> _responseRepositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly ResponseService _innerService;
    private readonly CachedResponseService _cachedService;

    public CachedResponseServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _surveyRepositoryMock = new Mock<ISurveyRepository>();
        _responseRepositoryMock = new Mock<IResponseRepository>();
        _cacheMock = new Mock<ICacheService>();

        _unitOfWorkMock.Setup(u => u.Surveys).Returns(_surveyRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Responses).Returns(_responseRepositoryMock.Object);

        _innerService = new ResponseService(_unitOfWorkMock.Object);
        _cachedService = new CachedResponseService(_innerService, _cacheMock.Object);
    }

    [Fact]
    public async Task GetSurveyResultsAsync_WhenCached_ShouldReturnCachedResult()
    {
        
        var surveyId = Guid.NewGuid();
        var cachedResult = new SurveyResultResponse(surveyId, "Test Survey", 10, []);

        _cacheMock.Setup(c => c.GetAsync<SurveyResultResponse>(
                It.Is<string>(k => k.Contains(surveyId.ToString())),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedResult);

        
        var result = await _cachedService.GetSurveyResultsAsync(surveyId);

        
        result.Should().Be(cachedResult);
        _surveyRepositoryMock.Verify(r => r.GetByIdWithQuestionsAndOptionsAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSurveyResultsAsync_WhenNotCached_ShouldFetchAndCache()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);

        _cacheMock.Setup(c => c.GetAsync<SurveyResultResponse>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SurveyResultResponse?)null);

        _surveyRepositoryMock.Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(
                surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _responseRepositoryMock.Setup(r => r.GetResponseCountBySurveyIdAsync(
                surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _responseRepositoryMock.Setup(r => r.GetOptionCountsAsync(
                surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        
        var result = await _cachedService.GetSurveyResultsAsync(surveyId);

        
        result.Should().NotBeNull();
        result.SurveyId.Should().Be(surveyId);
        _cacheMock.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains(surveyId.ToString())),
            It.IsAny<SurveyResultResponse>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetResponseCountAsync_WhenCached_ShouldReturnCachedResult()
    {
        
        var surveyId = Guid.NewGuid();
        var cachedCount = 42;

        _cacheMock.Setup(c => c.GetAsync<int?>(
                It.Is<string>(k => k.Contains(surveyId.ToString())),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedCount);

        
        var result = await _cachedService.GetResponseCountAsync(surveyId);

        
        result.Should().Be(cachedCount);
        _responseRepositoryMock.Verify(r => r.GetResponseCountBySurveyIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetResponseCountAsync_WhenNotCached_ShouldFetchAndCache()
    {
        
        var surveyId = Guid.NewGuid();
        var count = 15;

        _cacheMock.Setup(c => c.GetAsync<int?>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        _responseRepositoryMock.Setup(r => r.GetResponseCountBySurveyIdAsync(
                surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        
        var result = await _cachedService.GetResponseCountAsync(surveyId);

        
        result.Should().Be(count);
        _cacheMock.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains(surveyId.ToString())),
            count,
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitResponseAsync_ShouldInvalidateCache()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);
        ActivateSurvey(survey);

        var questionId = survey.Questions.First().Id;
        var optionId = survey.Questions.First().Options.First().Id;

        var request = new SubmitResponseRequest(
            surveyId,
            "participant1",
            [new AnswerRequest(questionId, optionId)]
        );

        _surveyRepositoryMock.Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(
                surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _responseRepositoryMock.Setup(r => r.HasRespondedAsync(
                surveyId, "participant1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        
        await _cachedService.SubmitResponseAsync(request);

        
        _cacheMock.Verify(c => c.RemoveAsync(
            It.Is<string>(k => k.Contains("survey_results_") && k.Contains(surveyId.ToString())),
            It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(
            It.Is<string>(k => k.Contains("survey_count_") && k.Contains(surveyId.ToString())),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitResponseAsync_ShouldReturnResponseId()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);
        ActivateSurvey(survey);

        var questionId = survey.Questions.First().Id;
        var optionId = survey.Questions.First().Options.First().Id;

        var request = new SubmitResponseRequest(
            surveyId,
            null,
            [new AnswerRequest(questionId, optionId)]
        );

        _surveyRepositoryMock.Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(
                surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        
        var result = await _cachedService.SubmitResponseAsync(request);

        
        result.Should().NotBe(Guid.Empty);
    }

    private static Survey CreateTestSurvey(Guid surveyId)
    {
        var survey = new Survey("Test Survey", "Description");
        SetEntityId(survey, surveyId);

        var question = new Question(surveyId, "Test question?", 1, true);
        var option1 = new Option(question.Id, "Option 1", 1);
        var option2 = new Option(question.Id, "Option 2", 2);

        question.AddOption(option1);
        question.AddOption(option2);
        survey.AddQuestion(question);

        return survey;
    }

    private static void ActivateSurvey(Survey survey)
    {
        survey.Activate();
    }

    private static void SetEntityId(Entity entity, Guid id)
    {
        var idProperty = typeof(Entity).GetProperty("Id");
        idProperty!.SetValue(entity, id);
    }
}
