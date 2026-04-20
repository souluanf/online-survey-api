using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyEditTests : MudBunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyEditTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldShowProgressCircular_WhenLoading()
    {
        
        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .Returns(new TaskCompletionSource<SurveyDetailResponse?>().Task);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, Guid.NewGuid()));

        cut.FindAll(".mud-progress-circular").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldShowSurveyNotFound_WhenSurveyIsNull()
    {
        
        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, Guid.NewGuid()));
        
        cut.WaitForState(() => cut.Markup.Contains("não encontrada"));
        cut.Markup.Should().Contain("Pesquisa não encontrada");
    }

    [Fact]
    public async Task ShouldShowCannotEditMessage_WhenSurveyIsActive()
    {
        
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Active);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("não pode ser editada"));
        cut.Markup.Should().Contain("não pode ser editada");
    }

    [Fact]
    public async Task ShouldShowCannotEditMessage_WhenSurveyIsClosed()
    {
        
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Closed);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("não pode ser editada"));
        cut.Markup.Should().Contain("Encerrada");
    }

    [Fact]
    public async Task ShouldRenderEditForm_WhenSurveyIsDraft()
    {
        
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        
        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));
        cut.Markup.Should().Contain("Editar Pesquisa");
    }

    [Fact]
    public async Task ShouldPopulateTitleInput_WithExistingValue()
    {
        
        var survey = CreateSurveyDetailResponse("Existing Title", status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));
        cut.Markup.Should().Contain("Existing Title");
    }

    [Fact]
    public async Task ShouldPopulateDescriptionTextarea_WithExistingValue()
    {
        
        var survey = CreateSurveyDetailResponse("Title", "Existing Description", SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        
        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));
        cut.Markup.Should().Contain("Existing Description");
    }

    [Fact]
    public async Task ShouldDisplayQuestions_ReadOnly()
    {
        
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        
        cut.WaitForState(() => cut.Markup.Contains("Pergunta 1"));
        cut.Markup.Should().Contain("Question 1?");
        cut.Markup.Should().Contain("Option A");
        cut.Markup.Should().Contain("Option B");
    }

    [Fact]
    public async Task ShouldShowRequiredBadge_ForRequiredQuestions()
    {
        
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Obrigatória"));
        cut.Markup.Should().Contain("Obrigatória");
        cut.FindAll(".mud-chip-color-error").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SaveChanges_WhenSuccessful_ShouldShowSuccessMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);
        var updatedSurvey = CreateSurveyDetailResponse("Updated Title", id: surveyId, status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.UpdateSurveyAsync(surveyId, It.IsAny<UpdateSurveyRequest>()))
            .ReturnsAsync(updatedSurvey);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));

        var saveButton = cut.Find("button.mud-button-root.mud-button-filled-primary");
        await cut.InvokeAsync(() => saveButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("atualizada com sucesso"));
        cut.Markup.Should().Contain("atualizada com sucesso");
    }

    [Fact]
    public async Task SaveChanges_WhenFailed_ShouldShowErrorMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.UpdateSurveyAsync(surveyId, It.IsAny<UpdateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));

        
        var saveButton = cut.Find("button.mud-button-root.mud-button-filled-primary");
        await cut.InvokeAsync(() => saveButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("Erro ao salvar"));
        cut.Markup.Should().Contain("Erro ao salvar");
    }

    [Fact]
    public async Task ActivateSurvey_WhenSuccessful_ShouldNavigateToSurveysList()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);
        var activatedSurvey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Active);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync(activatedSurvey);

        var navManager = Services.GetRequiredService<NavigationManager>();

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Ativar Pesquisa"));

        var activateButton = cut.Find("button.mud-button-root.mud-button-filled-success");
        await cut.InvokeAsync(() => activateButton.Click());

        
        navManager.Uri.Should().EndWith("/surveys");
    }

    [Fact]
    public async Task ActivateSurvey_WhenFailed_ShouldShowErrorMessage()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", id: surveyId, status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("Ativar Pesquisa"));

        
        var activateButton = cut.Find("button.mud-button-root.mud-button-filled-success");
        await cut.InvokeAsync(() => activateButton.Click());

        
        cut.WaitForState(() => cut.Markup.Contains("Erro ao ativar"));
        cut.Markup.Should().Contain("Erro ao ativar pesquisa");
    }

    [Fact]
    public async Task SaveButton_ShouldBeDisabled_WhenTitleIsEmpty()
    {
        
        var survey = CreateSurveyDetailResponse("Test Survey", status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Editar Pesquisa"));

        // Limpar o título via input interno do MudTextField
        var titleInput = cut.Find("input.mud-input-slot");
        titleInput.Change("");

        
        var saveButton = cut.Find("button.mud-button-root.mud-button-filled-primary");
        saveButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task BackButton_ShouldLinkToSurveysList()
    {
        
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Voltar"));

        var backLink = cut.Find("a[href='/surveys']");
        backLink.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldShowInfoMessage_AboutEditingQuestionsLimitation()
    {
        
        var survey = CreateSurveyDetailResponse(status: SurveyStatus.Draft);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        
        var cut = Render<SurveyEdit>(parameters =>
            parameters.Add(p => p.Id, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Para editar perguntas"));
        cut.Markup.Should().Contain("exclua a pesquisa e crie uma nova");
    }

    private static SurveyDetailResponse CreateSurveyDetailResponse(
        string title = "Test Survey",
        string? description = null,
        SurveyStatus status = SurveyStatus.Draft,
        Guid? id = null)
    {
        return new SurveyDetailResponse(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: description,
            Status: status,
            StartDate: status == SurveyStatus.Active ? DateTime.UtcNow : null,
            EndDate: status == SurveyStatus.Active ? DateTime.UtcNow.AddDays(7) : null,
            Questions:
            [
                new QuestionResponse(
                    Id: Guid.NewGuid(),
                    Text: "Question 1?",
                    Order: 1,
                    IsRequired: true,
                    Options:
                    [
                        new OptionResponse(Guid.NewGuid(), "Option A", 1),
                        new OptionResponse(Guid.NewGuid(), "Option B", 2)
                    ])
            ],
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }
}
