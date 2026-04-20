using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyAnswerTests : MudBunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyAnswerTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldShowProgressCircular_WhenLoading()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .Returns(new TaskCompletionSource<SurveyDetailResponse?>().Task); // Never completes

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        // Assert — MudProgressCircular, sem texto "Carregando pesquisa..."
        cut.FindAll(".mud-progress-circular").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldShowSurveyNotFound_WhenSurveyIsNull()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        // Assert — MudAlert Severity.Error
        cut.WaitForState(() => cut.Markup.Contains("não encontrada"));
        cut.Markup.Should().Contain("Pesquisa não encontrada");
    }

    [Fact]
    public async Task ShouldRenderSurveyTitle_WhenLoaded()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey Title");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Test Survey Title"));
        cut.Markup.Should().Contain("Test Survey Title");
    }

    [Fact]
    public async Task ShouldRenderDescription_WhenPresent()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey", description: "Test Description");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert — MudText Typo.subtitle1 com a descrição
        cut.WaitForState(() => cut.Markup.Contains("Test Description"));
        cut.Markup.Should().Contain("Test Description");
    }

    [Fact]
    public async Task ShouldRenderQuestions()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert — MudCard renderiza com mud-card class
        cut.WaitForState(() => cut.Markup.Contains("Question 1"));
        cut.FindAll(".mud-card").Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ShouldRenderOptions_AsMudRadios()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert — MudRadio renderiza input[type=radio]
        cut.WaitForState(() => cut.FindAll("input[type='radio']").Count >= 2);
        cut.FindAll("input[type='radio']").Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ShouldShowRequiredIndicator_ForRequiredQuestions()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        // Act
        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        // Assert — MudText Color.Error com "*"
        cut.WaitForState(() => cut.Markup.Contains("mud-error-text"));
        cut.Find(".mud-error-text").TextContent.Should().Contain("*");
    }

    [Fact]
    public async Task HandleSubmit_WhenRequiredQuestionUnanswered_ShouldShowError()
    {
        // Arrange
        var survey = CreateSurveyDetailResponse("Test Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.Markup.Contains("Enviar Respostas"));

        // Act — submeter sem selecionar opção
        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert — MudAlert Severity.Error com mensagem de validação
        cut.WaitForState(() => cut.Markup.Contains("Por favor, responda a pergunta"));
        cut.Markup.Should().Contain("Por favor, responda a pergunta");
    }

    [Fact]
    public async Task HandleSubmit_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", questionId: questionId, optionId: optionId);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .ReturnsAsync(true);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.FindAll("input[type='radio']").Count >= 2);

        // Selecionar opção via MudRadioGroup ValueChanged
        var radioGroup = cut.FindComponent<MudRadioGroup<Guid>>();
        await cut.InvokeAsync(() => radioGroup.Instance.ValueChanged.InvokeAsync(optionId));

        // Act
        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert — tela de sucesso com "Obrigado por participar!"
        cut.WaitForState(() => cut.Markup.Contains("Obrigado por participar"));
        cut.Markup.Should().Contain("Obrigado por participar");
    }

    [Fact]
    public async Task HandleSubmit_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", questionId: questionId, optionId: optionId);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .ReturnsAsync(false);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.FindAll("input[type='radio']").Count >= 2);

        var radioGroup = cut.FindComponent<MudRadioGroup<Guid>>();
        await cut.InvokeAsync(() => radioGroup.Instance.ValueChanged.InvokeAsync(optionId));

        // Act
        var form = cut.Find("form");
        await cut.InvokeAsync(() => form.Submit());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao enviar resposta"));
        cut.Markup.Should().Contain("Erro ao enviar resposta");
    }

    [Fact]
    public async Task SuccessMessage_ShouldContainLinksToResultsAndSurveys()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", surveyId, questionId: questionId, optionId: optionId);

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(surveyId))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .ReturnsAsync(true);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        cut.WaitForState(() => cut.FindAll("input[type='radio']").Count >= 2);

        var radioGroup = cut.FindComponent<MudRadioGroup<Guid>>();
        await cut.InvokeAsync(() => radioGroup.Instance.ValueChanged.InvokeAsync(optionId));
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        cut.WaitForState(() => cut.Markup.Contains("Obrigado"));

        // Assert — links Ver Resultados e Voltar às Pesquisas
        var resultLink = cut.Find($"a[href='/surveys/{surveyId}/results']");
        resultLink.Should().NotBeNull();

        var surveysLink = cut.Find("a[href='/surveys']");
        surveysLink.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitButton_ShouldBeDisabled_WhileSubmitting()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse("Test Survey", questionId: questionId, optionId: optionId);
        var submitTcs = new TaskCompletionSource<bool>();

        _surveyServiceMock
            .Setup(x => x.GetSurveyByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(survey);

        _surveyServiceMock
            .Setup(x => x.SubmitResponseAsync(It.IsAny<SubmitResponseRequest>()))
            .Returns(submitTcs.Task);

        var cut = Render<SurveyAnswer>(parameters =>
            parameters.Add(p => p.SurveyId, survey.Id));

        cut.WaitForState(() => cut.FindAll("input[type='radio']").Count >= 2);

        var radioGroup = cut.FindComponent<MudRadioGroup<Guid>>();
        await cut.InvokeAsync(() => radioGroup.Instance.ValueChanged.InvokeAsync(optionId));

        // Act — iniciar submit sem completar
        var submitTask = cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert — botão desabilitado e texto "Enviando..."
        cut.WaitForState(() => cut.Markup.Contains("Enviando"));
        cut.Find("button[type='submit']").HasAttribute("disabled").Should().BeTrue();

        // Cleanup
        submitTcs.SetResult(true);
        await submitTask;
    }

    private static SurveyDetailResponse CreateSurveyDetailResponse(
        string title,
        Guid? id = null,
        string? description = null,
        Guid? questionId = null,
        Guid? optionId = null)
    {
        var qId = questionId ?? Guid.NewGuid();
        var oId = optionId ?? Guid.NewGuid();

        return new SurveyDetailResponse(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: description,
            Status: SurveyStatus.Active,
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(7),
            Questions:
            [
                new QuestionResponse(
                    Id: qId,
                    Text: "Question 1?",
                    Order: 1,
                    IsRequired: true,
                    Options:
                    [
                        new OptionResponse(oId, "Option A", 1),
                        new OptionResponse(Guid.NewGuid(), "Option B", 2)
                    ])
            ],
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }
}
