using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyListTests : MudBunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyListTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldShowLoadingIndicator_WhenLoading()
    {
        
        _surveyServiceMock
            .Setup(x => x.GetSurveysAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SurveyStatus?>()))
            .Returns(new TaskCompletionSource<PaginatedResponse<SurveyResponse>?>().Task);

        
        var cut = Render<SurveyList>();

        cut.FindAll(".mud-progress-circular").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldRenderPageTitle()
    {
        
        SetupEmptySurveys();

        
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Pesquisas"));

        
        cut.Markup.Should().Contain("Pesquisas");
    }

    [Fact]
    public async Task ShouldShowCreateButton()
    {
        
        SetupEmptySurveys();

        
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Nova Pesquisa"));

        var createLink = cut.Find("a[href='/surveys/create']");
        createLink.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldShowTabs()
    {
        
        SetupEmptySurveys();

        
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Ativas"));

        cut.Markup.Should().Contain("Ativas");
        cut.Markup.Should().Contain("Rascunhos");
        cut.Markup.Should().Contain("Encerradas");
    }

    [Fact]
    public async Task ShouldShowEmptyMessage_WhenNoActiveSurveys()
    {
        
        SetupEmptySurveys();

        
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Nenhuma pesquisa ativa"));

        cut.Markup.Should().Contain("Nenhuma pesquisa ativa");
    }

    [Fact]
    public async Task ShouldShowActiveSurveys()
    {
        
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Active Survey 1", SurveyStatus.Active),
            CreateSurveyResponse("Active Survey 2", SurveyStatus.Active)
        };
        SetupSurveys(surveys);

        
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Active Survey 1"));

        cut.Markup.Should().Contain("Active Survey 1");
        cut.Markup.Should().Contain("Active Survey 2");
        cut.FindAll(".mud-card").Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ShouldSwitchToDraftTab()
    {
        
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft)
        };
        SetupSurveys(surveys);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Rascunhos"));

        var tabs = cut.FindAll(".mud-tab");
        await cut.InvokeAsync(() => tabs[1].Click());

        
        cut.WaitForState(() => cut.Markup.Contains("Draft Survey"));
        cut.Markup.Should().Contain("Draft Survey");
    }

    [Fact]
    public async Task ShouldSwitchToClosedTab()
    {
        
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Closed Survey", SurveyStatus.Closed)
        };
        SetupSurveys(surveys);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Encerradas"));

        var tabs = cut.FindAll(".mud-tab");
        await cut.InvokeAsync(() => tabs[2].Click());

        
        cut.WaitForState(() => cut.Markup.Contains("Closed Survey"));
        cut.Markup.Should().Contain("Closed Survey");
    }

    [Fact]
    public async Task ActivateSurvey_WhenSuccessful_ShouldShowSuccessMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync(new SurveyDetailResponse(surveyId, "Draft Survey", null, SurveyStatus.Active, null, null, [], DateTime.UtcNow, null));

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Rascunhos"));

        var tabs = cut.FindAll(".mud-tab");
        await cut.InvokeAsync(() => tabs[1].Click());
        cut.WaitForState(() => cut.Markup.Contains("Draft Survey"));

        var activateButton = cut.Find("button.mud-button-root.mud-button-filled-success");
        await cut.InvokeAsync(() => activateButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("ativada com sucesso"));
        cut.Markup.Should().Contain("ativada com sucesso");
    }

    [Fact]
    public async Task ActivateSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Rascunhos"));

        var tabs = cut.FindAll(".mud-tab");
        await cut.InvokeAsync(() => tabs[1].Click());
        cut.WaitForState(() => cut.Markup.Contains("Draft Survey"));

        
        var activateButton = cut.Find("button.mud-button-root.mud-button-filled-success");
        await cut.InvokeAsync(() => activateButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("Erro ao ativar"));
        cut.Markup.Should().Contain("Erro ao ativar");
    }

    [Fact]
    public async Task DeleteSurvey_WhenSuccessful_ShouldShowSuccessMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.DeleteSurveyAsync(surveyId))
            .ReturnsAsync(true);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Rascunhos"));

        var tabs = cut.FindAll(".mud-tab");
        await cut.InvokeAsync(() => tabs[1].Click());
        cut.WaitForState(() => cut.Markup.Contains("Draft Survey"));

        var deleteButton = cut.Find("button.mud-button-root.mud-button-outlined-error");
        await cut.InvokeAsync(() => deleteButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("excluída com sucesso"));
        cut.Markup.Should().Contain("excluída com sucesso");
    }

    [Fact]
    public async Task DeleteSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.DeleteSurveyAsync(surveyId))
            .ReturnsAsync(false);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Rascunhos"));

        var tabs = cut.FindAll(".mud-tab");
        await cut.InvokeAsync(() => tabs[1].Click());
        cut.WaitForState(() => cut.Markup.Contains("Draft Survey"));

        
        var deleteButton = cut.Find("button.mud-button-root.mud-button-outlined-error");
        await cut.InvokeAsync(() => deleteButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("Erro ao excluir"));
        cut.Markup.Should().Contain("Erro ao excluir");
    }

    [Fact]
    public async Task CloseSurvey_WhenSuccessful_ShouldShowSuccessMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Active Survey", SurveyStatus.Active, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.CloseSurveyAsync(surveyId))
            .ReturnsAsync(new SurveyDetailResponse(surveyId, "Active Survey", null, SurveyStatus.Closed, null, null, [], DateTime.UtcNow, null));

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Active Survey"));

        var closeButton = cut.Find("button.mud-button-root.mud-button-outlined-warning");
        await cut.InvokeAsync(() => closeButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("encerrada com sucesso"));
        cut.Markup.Should().Contain("encerrada com sucesso");
    }

    [Fact]
    public async Task CloseSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Active Survey", SurveyStatus.Active, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.CloseSurveyAsync(surveyId))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Active Survey"));

        
        var closeButton = cut.Find("button.mud-button-root.mud-button-outlined-warning");
        await cut.InvokeAsync(() => closeButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("Erro ao encerrar"));
        cut.Markup.Should().Contain("Erro ao encerrar");
    }

    [Fact]
    public async Task ShouldShowSurveyDetails()
    {
        
        var surveys = new List<SurveyResponse>
        {
            new(Guid.NewGuid(), "Test Survey", "Test Description", SurveyStatus.Active,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(7), 5, 10, DateTime.UtcNow, null)
        };
        SetupSurveys(surveys);

        
        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Test Survey"));

        
        cut.Markup.Should().Contain("Test Survey");
        cut.Markup.Should().Contain("Test Description");
        cut.Markup.Should().Contain("5 perguntas");
        cut.Markup.Should().Contain("10 respostas");
    }

    [Fact]
    public async Task ShouldDismissMessage_WhenCloseIconClicked()
    {
        
        var surveyId = Guid.NewGuid();
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Draft Survey", SurveyStatus.Draft, surveyId)
        };
        SetupSurveys(surveys);

        _surveyServiceMock
            .Setup(x => x.DeleteSurveyAsync(surveyId))
            .ReturnsAsync(true);

        var cut = Render<SurveyList>();
        cut.WaitForState(() => cut.Markup.Contains("Rascunhos"));

        var tabs = cut.FindAll(".mud-tab");
        await cut.InvokeAsync(() => tabs[1].Click());
        cut.WaitForState(() => cut.Markup.Contains("Draft Survey"));

        await cut.InvokeAsync(() => cut.Find("button.mud-button-root.mud-button-outlined-error").Click());
        cut.WaitForState(() => cut.Markup.Contains("excluída com sucesso"));

        var closeButton = cut.Find("button.mud-alert-close-button");
        await cut.InvokeAsync(() => closeButton.Click());

        
        cut.Markup.Should().NotContain("excluída com sucesso");
    }

    private void SetupEmptySurveys()
    {
        _surveyServiceMock
            .Setup(x => x.GetSurveysAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SurveyStatus?>()))
            .ReturnsAsync(new PaginatedResponse<SurveyResponse>([], 1, 100, 0, 0));
    }

    private void SetupSurveys(List<SurveyResponse> surveys)
    {
        _surveyServiceMock
            .Setup(x => x.GetSurveysAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SurveyStatus?>()))
            .ReturnsAsync(new PaginatedResponse<SurveyResponse>(surveys, 1, 100, surveys.Count, 1));
    }

    private static SurveyResponse CreateSurveyResponse(string title, SurveyStatus status, Guid? id = null)
    {
        return new SurveyResponse(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: null,
            Status: status,
            StartDate: status == SurveyStatus.Active ? DateTime.UtcNow : null,
            EndDate: status == SurveyStatus.Active ? DateTime.UtcNow.AddDays(7) : null,
            QuestionCount: 1,
            ResponseCount: 0,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }
}
