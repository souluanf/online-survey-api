using FluentAssertions;
using Moq;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Services;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Exceptions;
using OnlineSurvey.Domain.Repositories;

namespace OnlineSurvey.Application.Tests.Services;

public class ResponseServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISurveyRepository> _surveyRepositoryMock;
    private readonly Mock<IResponseRepository> _responseRepositoryMock;
    private readonly ResponseService _service;

    public ResponseServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _surveyRepositoryMock = new Mock<ISurveyRepository>();
        _responseRepositoryMock = new Mock<IResponseRepository>();

        _unitOfWorkMock.Setup(u => u.Surveys).Returns(_surveyRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Responses).Returns(_responseRepositoryMock.Object);

        _service = new ResponseService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task SubmitResponseAsync_WithValidRequest_ShouldReturnResponseId()
    {
        
        var survey = CreateActiveSurvey();
        var questionId = survey.Questions.First().Id;
        var optionId = survey.Questions.First().Options.First().Id;

        var request = new SubmitResponseRequest(
            survey.Id,
            null,
            [new AnswerRequest(questionId, optionId)]
        );

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        
        var result = await _service.SubmitResponseAsync(request);

        
        result.Should().NotBe(Guid.Empty);
        _responseRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Response>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitResponseAsync_WhenSurveyNotFound_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();
        var request = new SubmitResponseRequest(surveyId, null, []);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        
        var act = () => _service.SubmitResponseAsync(request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Survey not found.");
    }

    [Fact]
    public async Task SubmitResponseAsync_WhenSurveyNotOpen_ShouldThrowDomainException()
    {
        
        var survey = new Survey("Draft Survey");
        var request = new SubmitResponseRequest(survey.Id, null, []);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        
        var act = () => _service.SubmitResponseAsync(request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Survey is not accepting responses.");
    }

    [Fact]
    public async Task SubmitResponseAsync_WhenParticipantAlreadyResponded_ShouldThrowDomainException()
    {
        
        var survey = CreateActiveSurvey();
        var request = new SubmitResponseRequest(survey.Id, "participant123", []);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _responseRepositoryMock
            .Setup(r => r.HasRespondedAsync(survey.Id, "participant123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        
        var act = () => _service.SubmitResponseAsync(request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("You have already responded to this survey.");
    }

    [Fact]
    public async Task SubmitResponseAsync_WhenRequiredQuestionNotAnswered_ShouldThrowDomainException()
    {
        
        var survey = CreateActiveSurvey();
        var request = new SubmitResponseRequest(survey.Id, null, []); // No answers

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        
        var act = () => _service.SubmitResponseAsync(request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*is required*");
    }

    [Fact]
    public async Task SubmitResponseAsync_WhenQuestionNotFound_ShouldThrowDomainException()
    {
        
        var survey = CreateActiveSurvey();
        var validQuestionId = survey.Questions.First().Id;
        var validOptionId = survey.Questions.First().Options.First().Id;
        var invalidQuestionId = Guid.NewGuid();

        // Include valid answer for required question, plus an invalid question
        var request = new SubmitResponseRequest(
            survey.Id,
            null,
            [
                new AnswerRequest(validQuestionId, validOptionId),
                new AnswerRequest(invalidQuestionId, Guid.NewGuid())
            ]
        );

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        
        var act = () => _service.SubmitResponseAsync(request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"Question with ID {invalidQuestionId} not found in this survey.");
    }

    [Fact]
    public async Task SubmitResponseAsync_WhenInvalidOptionSelected_ShouldThrowDomainException()
    {
        
        var survey = CreateActiveSurvey();
        var questionId = survey.Questions.First().Id;
        var invalidOptionId = Guid.NewGuid();

        var request = new SubmitResponseRequest(
            survey.Id,
            null,
            [new AnswerRequest(questionId, invalidOptionId)]
        );

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        
        var act = () => _service.SubmitResponseAsync(request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"Option with ID {invalidOptionId} is not valid*");
    }

    [Fact]
    public async Task SubmitResponseAsync_WithIpAddress_ShouldStoreIpAddress()
    {
        
        var survey = CreateActiveSurvey();
        var questionId = survey.Questions.First().Id;
        var optionId = survey.Questions.First().Options.First().Id;

        var request = new SubmitResponseRequest(
            survey.Id,
            null,
            [new AnswerRequest(questionId, optionId)]
        );

        Response? capturedResponse = null;
        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _responseRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Response>(), It.IsAny<CancellationToken>()))
            .Callback<Response, CancellationToken>((r, _) => capturedResponse = r);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        
        await _service.SubmitResponseAsync(request, "192.168.1.1");

        
        capturedResponse.Should().NotBeNull();
        capturedResponse!.IpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task GetSurveyResultsAsync_WithValidSurveyId_ShouldReturnResults()
    {
        
        var survey = CreateActiveSurvey();
        var questionId = survey.Questions.First().Id;
        var option1Id = survey.Questions.First().Options.First().Id;
        var option2Id = survey.Questions.First().Options.Last().Id;

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _responseRepositoryMock
            .Setup(r => r.GetResponseCountBySurveyIdAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        _responseRepositoryMock
            .Setup(r => r.GetOptionCountsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { { option1Id, 7 }, { option2Id, 3 } });

        
        var result = await _service.GetSurveyResultsAsync(survey.Id);

        
        result.Should().NotBeNull();
        result.SurveyId.Should().Be(survey.Id);
        result.SurveyTitle.Should().Be(survey.Title);
        result.TotalResponses.Should().Be(10);
        result.Questions.Should().HaveCount(1);
        result.Questions.First().Options.First().Count.Should().Be(7);
        result.Questions.First().Options.First().Percentage.Should().Be(70);
    }

    [Fact]
    public async Task GetSurveyResultsAsync_WhenSurveyNotFound_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        
        var act = () => _service.GetSurveyResultsAsync(surveyId);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Survey not found.");
    }

    [Fact]
    public async Task GetSurveyResultsAsync_WithNoResponses_ShouldReturnZeroPercentages()
    {
        
        var survey = CreateActiveSurvey();

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _responseRepositoryMock
            .Setup(r => r.GetResponseCountBySurveyIdAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _responseRepositoryMock
            .Setup(r => r.GetOptionCountsAsync(survey.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int>());

        
        var result = await _service.GetSurveyResultsAsync(survey.Id);

        
        result.TotalResponses.Should().Be(0);
        result.Questions.First().Options.Should().AllSatisfy(o => o.Percentage.Should().Be(0));
    }

    [Fact]
    public async Task GetResponseCountAsync_ShouldReturnCount()
    {
        
        var surveyId = Guid.NewGuid();

        _responseRepositoryMock
            .Setup(r => r.GetResponseCountBySurveyIdAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        
        var result = await _service.GetResponseCountAsync(surveyId);

        
        result.Should().Be(42);
    }

    private static Survey CreateActiveSurvey()
    {
        var survey = new Survey("Test Survey", "Test Description");
        var question = new Question(survey.Id, "Test Question?", 1, true);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);
        survey.Activate();
        return survey;
    }
}
