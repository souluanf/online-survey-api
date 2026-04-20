using FluentAssertions;
using Moq;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Services;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Enums;
using OnlineSurvey.Domain.Exceptions;
using OnlineSurvey.Domain.Repositories;

namespace OnlineSurvey.Application.Tests.Services;

public class SurveyServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ISurveyRepository> _surveyRepositoryMock;
    private readonly Mock<IResponseRepository> _responseRepositoryMock;
    private readonly SurveyService _service;

    public SurveyServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _surveyRepositoryMock = new Mock<ISurveyRepository>();
        _responseRepositoryMock = new Mock<IResponseRepository>();

        _unitOfWorkMock.Setup(u => u.Surveys).Returns(_surveyRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Responses).Returns(_responseRepositoryMock.Object);

        _service = new SurveyService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateSurveyAsync_WithValidRequest_ShouldReturnSurveyDetail()
    {
        
        var request = new CreateSurveyRequest(
            "Election Poll 2024",
            "Public opinion poll",
            [
                new CreateQuestionRequest(
                    "Who will you vote for?",
                    1,
                    true,
                    [
                        new CreateOptionRequest("Candidate A", 1),
                        new CreateOptionRequest("Candidate B", 2)
                    ]
                )
            ]
        );

        
        var result = await _service.CreateSurveyAsync(request);

        
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.Description.Should().Be(request.Description);
        result.Status.Should().Be(SurveyStatus.Draft);
        result.Questions.Should().HaveCount(1);
        result.Questions.First().Options.Should().HaveCount(2);

        _surveyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Survey>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSurveyByIdAsync_WhenSurveyExists_ShouldReturnSurveyDetail()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        _responseRepositoryMock
            .Setup(r => r.GetResponseCountBySurveyIdAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        
        var result = await _service.GetSurveyByIdAsync(surveyId);

        
        result.Should().NotBeNull();
        result!.Id.Should().Be(surveyId);
        result.Title.Should().Be(survey.Title);
    }

    [Fact]
    public async Task GetSurveyByIdAsync_WhenSurveyDoesNotExist_ShouldReturnNull()
    {
        
        var surveyId = Guid.NewGuid();
        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        
        var result = await _service.GetSurveyByIdAsync(surveyId);

        
        result.Should().BeNull();
    }

    [Fact]
    public async Task ActivateSurveyAsync_WhenSurveyIsDraft_ShouldActivate()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var request = new ActivateSurveyRequest(null, null);

        
        var result = await _service.ActivateSurveyAsync(surveyId, request);

        
        result.Status.Should().Be(SurveyStatus.Active);
        _surveyRepositoryMock.Verify(r => r.UpdateAsync(survey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSurveyAsync_WhenSurveyDoesNotExist_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();
        _surveyRepositoryMock
            .Setup(r => r.GetByIdAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        
        var act = () => _service.DeleteSurveyAsync(surveyId);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Survey not found.");
    }

    [Fact]
    public async Task GetSurveysAsync_ShouldReturnPaginatedResults()
    {
        
        var surveys = new List<Survey>
        {
            CreateTestSurvey(Guid.NewGuid()),
            CreateTestSurvey(Guid.NewGuid())
        };

        _surveyRepositoryMock
            .Setup(r => r.GetPaginatedAsync(1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((surveys, 2));

        _responseRepositoryMock
            .Setup(r => r.GetResponseCountBySurveyIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        
        var result = await _service.GetSurveysAsync(1, 10);

        
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetActiveSurveysAsync_ShouldReturnOnlyActiveSurveys()
    {
        
        var activeSurvey = CreateActiveSurvey(Guid.NewGuid());
        var surveys = new List<Survey> { activeSurvey };

        _surveyRepositoryMock
            .Setup(r => r.GetActiveSurveysAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(surveys);

        _responseRepositoryMock
            .Setup(r => r.GetResponseCountBySurveyIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        
        var result = await _service.GetActiveSurveysAsync();

        
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(SurveyStatus.Active);
    }

    [Fact]
    public async Task UpdateSurveyAsync_WhenSurveyIsDraft_ShouldUpdateSuccessfully()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var request = new UpdateSurveyRequest("Updated Title", "Updated Description");

        
        var result = await _service.UpdateSurveyAsync(surveyId, request);

        
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        _surveyRepositoryMock.Verify(r => r.UpdateAsync(survey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSurveyAsync_WhenSurveyIsActive_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateActiveSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var request = new UpdateSurveyRequest("Updated Title", "Updated Description");

        
        var act = () => _service.UpdateSurveyAsync(surveyId, request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Only draft surveys can be updated.");
    }

    [Fact]
    public async Task UpdateSurveyAsync_WhenSurveyNotFound_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();
        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        var request = new UpdateSurveyRequest("Updated Title", null);

        
        var act = () => _service.UpdateSurveyAsync(surveyId, request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Survey not found.");
    }

    [Fact]
    public async Task ActivateSurveyAsync_WhenSurveyNotFound_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();
        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        var request = new ActivateSurveyRequest(null, null);

        
        var act = () => _service.ActivateSurveyAsync(surveyId, request);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Survey not found.");
    }

    [Fact]
    public async Task CloseSurveyAsync_WhenSurveyIsActive_ShouldCloseSuccessfully()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateActiveSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        
        var result = await _service.CloseSurveyAsync(surveyId);

        
        result.Status.Should().Be(SurveyStatus.Closed);
        _surveyRepositoryMock.Verify(r => r.UpdateAsync(survey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseSurveyAsync_WhenSurveyNotFound_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();
        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Survey?)null);

        
        var act = () => _service.CloseSurveyAsync(surveyId);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Survey not found.");
    }

    [Fact]
    public async Task DeleteSurveyAsync_WhenSurveyIsDraft_ShouldDeleteSuccessfully()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        
        await _service.DeleteSurveyAsync(surveyId);

        
        _surveyRepositoryMock.Verify(r => r.DeleteAsync(survey, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSurveyAsync_WhenSurveyIsActive_ShouldThrowDomainException()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateActiveSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        
        var act = () => _service.DeleteSurveyAsync(surveyId);

        
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Cannot delete an active survey. Close it first.");
    }

    [Fact]
    public async Task ActivateSurveyAsync_WithCustomDates_ShouldSetDates()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateTestSurvey(surveyId);

        _surveyRepositoryMock
            .Setup(r => r.GetByIdWithQuestionsAndOptionsAsync(surveyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(survey);

        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var request = new ActivateSurveyRequest(startDate, endDate);

        
        var result = await _service.ActivateSurveyAsync(surveyId, request);

        
        result.Status.Should().Be(SurveyStatus.Active);
        result.StartDate.Should().Be(startDate);
        result.EndDate.Should().Be(endDate);
    }

    private static Survey CreateTestSurvey(Guid? id = null)
    {
        var survey = new Survey("Test Survey", "Test Description");

        if (id.HasValue)
        {
            typeof(Survey).GetProperty("Id")!.SetValue(survey, id.Value);
        }

        var question = new Question(survey.Id, "Test Question?", 1);
        question.AddOption(new Option(question.Id, "Option 1", 1));
        question.AddOption(new Option(question.Id, "Option 2", 2));
        survey.AddQuestion(question);

        return survey;
    }

    private static Survey CreateActiveSurvey(Guid? id = null)
    {
        var survey = CreateTestSurvey(id);
        survey.Activate();
        return survey;
    }
}
