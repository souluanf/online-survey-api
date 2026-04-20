using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyCreateTests : MudBunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyCreateTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldRenderPageTitle()
    {
        // Act
        var cut = Render<SurveyCreate>();

        // Assert
        cut.Markup.Should().Contain("Criar Nova Pesquisa");
    }

    [Fact]
    public void ShouldRenderInitialFormWithOneQuestion()
    {
        // Act
        var cut = Render<SurveyCreate>();

        // Assert — MudCard renderiza com mud-card class
        // Uma pergunta inicial + card de informações = pelo menos 1 mud-card de pergunta
        cut.Markup.Should().Contain("Pergunta 1");
    }

    [Fact]
    public void ShouldShowTitleInput()
    {
        // Act
        var cut = Render<SurveyCreate>();

        // Assert — MudTextField renderiza input interno
        var inputs = cut.FindAll("input");
        inputs.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldShowDescriptionTextarea()
    {
        // Act
        var cut = Render<SurveyCreate>();

        // Assert — MudTextField com Lines>1 renderiza textarea
        var textarea = cut.Find("textarea");
        textarea.Should().NotBeNull();
    }

    [Fact]
    public void AddQuestion_ShouldAddNewQuestionCard()
    {
        // Arrange
        var cut = Render<SurveyCreate>();
        var initialCount = cut.FindAll(".mud-paper-outlined").Count;

        // Act — botão "Adicionar Pergunta" (Variant.Outlined, Color.Success)
        var addButton = cut.Find("button.mud-button-root.mud-button-outlined-success");
        addButton.Click();

        // Assert
        cut.FindAll(".mud-paper-outlined").Count.Should().Be(initialCount + 1);
    }

    [Fact]
    public void RemoveQuestion_ShouldRemoveQuestionCard()
    {
        // Arrange
        var cut = Render<SurveyCreate>();
        // Adicionar pergunta extra primeiro
        cut.Find("button.mud-button-root.mud-button-outlined-success").Click();
        var countAfterAdd = cut.FindAll(".mud-paper-outlined").Count;

        // Act — MudIconButton de deletar (Color.Error)
        var removeButton = cut.Find("button.mud-icon-button.mud-error-text");
        removeButton.Click();

        // Assert
        cut.FindAll(".mud-paper-outlined").Count.Should().Be(countAfterAdd - 1);
    }

    [Fact]
    public void AddOption_ShouldAddNewOptionInput()
    {
        // Arrange
        var cut = Render<SurveyCreate>();
        // Contar inputs de opção iniciais (2 por padrão)
        var initialInputCount = cut.FindAll("input.mud-input-slot").Count;

        // Act — botão "Adicionar Opção" (Variant.Outlined, Color.Primary, Size.Small)
        var addOptionButton = cut.Find("button.mud-button-root.mud-button-outlined-primary");
        addOptionButton.Click();

        // Assert — mais um input de opção
        cut.FindAll("input.mud-input-slot").Count.Should().BeGreaterThan(initialInputCount);
    }

    [Fact]
    public void SubmitButton_ShouldBeDisabled_WhenFormIsInvalid()
    {
        // Arrange
        var cut = Render<SurveyCreate>();

        // Assert — botão submit desabilitado quando título vazio
        var submitButton = cut.Find("button[type='submit']");
        submitButton.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public async Task HandleSubmit_WhenSuccessful_ShouldShowSuccessMessage()
    {
        // Arrange
        var createdSurvey = CreateSurveyDetailResponse();

        _surveyServiceMock
            .Setup(x => x.CreateSurveyAsync(It.IsAny<CreateSurveyRequest>()))
            .ReturnsAsync(createdSurvey);

        var cut = Render<SurveyCreate>();

        // Preencher título via input interno do MudTextField
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[0].Change("Test Survey");
        });
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[1].Change("Test Question?");
        });
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[2].Change("Option A");
        });
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[3].Change("Option B");
        });

        // Act
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Pesquisa criada com sucesso"));
        cut.Markup.Should().Contain("Pesquisa criada com sucesso");
    }

    [Fact]
    public async Task HandleSubmit_WhenFailed_ShouldShowErrorMessage()
    {
        // Arrange
        _surveyServiceMock
            .Setup(x => x.CreateSurveyAsync(It.IsAny<CreateSurveyRequest>()))
            .ReturnsAsync((SurveyDetailResponse?)null);

        var cut = Render<SurveyCreate>();

        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[0].Change("Test Survey");
        });
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[1].Change("Test Question?");
        });
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[2].Change("Option A");
        });
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input.mud-input-slot");
            inputs[3].Change("Option B");
        });

        // Act
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Erro ao criar pesquisa"));
        cut.Markup.Should().Contain("Erro ao criar pesquisa");
    }

    [Fact]
    public async Task ActivateAndGo_WhenSuccessful_ShouldNavigate()
    {
        // Arrange
        var surveyId = Guid.NewGuid();
        var createdSurvey = CreateSurveyDetailResponse(surveyId);
        var activatedSurvey = CreateSurveyDetailResponse(surveyId);

        _surveyServiceMock
            .Setup(x => x.CreateSurveyAsync(It.IsAny<CreateSurveyRequest>()))
            .ReturnsAsync(createdSurvey);

        _surveyServiceMock
            .Setup(x => x.ActivateSurveyAsync(surveyId, It.IsAny<ActivateSurveyRequest>()))
            .ReturnsAsync(activatedSurvey);

        var navManager = Services.GetRequiredService<NavigationManager>();

        var cut = Render<SurveyCreate>();

        await cut.InvokeAsync(() => cut.FindAll("input.mud-input-slot")[0].Change("Test Survey"));
        await cut.InvokeAsync(() => cut.FindAll("input.mud-input-slot")[1].Change("Test Question?"));
        await cut.InvokeAsync(() => cut.FindAll("input.mud-input-slot")[2].Change("Option A"));
        await cut.InvokeAsync(() => cut.FindAll("input.mud-input-slot")[3].Change("Option B"));

        await cut.InvokeAsync(() => cut.Find("form").Submit());
        cut.WaitForState(() => cut.Markup.Contains("Pesquisa criada com sucesso"));

        // Act — botão "Ativar Pesquisa" dentro do alert de sucesso
        var activateButton = cut.Find("button.mud-button-root.mud-button-filled-primary");
        await cut.InvokeAsync(() => activateButton.Click());

        // Assert
        navManager.Uri.Should().Contain($"/surveys/{surveyId}");
    }

    [Fact]
    public void IsRequiredCheckbox_ShouldBeCheckedByDefault()
    {
        // Arrange
        var cut = Render<SurveyCreate>();

        // Assert — MudCheckBox renderiza input[type=checkbox]
        var checkbox = cut.Find("input[type='checkbox']");
        checkbox.HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void CancelButton_ShouldLinkToSurveysList()
    {
        // Arrange
        var cut = Render<SurveyCreate>();

        // Assert — MudButton com Href="/surveys" renderiza como anchor
        var cancelLink = cut.Find("a[href='/surveys']");
        cancelLink.Should().NotBeNull();
    }

    private static SurveyDetailResponse CreateSurveyDetailResponse(Guid? id = null)
    {
        return new SurveyDetailResponse(
            Id: id ?? Guid.NewGuid(),
            Title: "Test Survey",
            Description: "Description",
            Status: SurveyStatus.Draft,
            StartDate: null,
            EndDate: null,
            Questions:
            [
                new QuestionResponse(
                    Id: Guid.NewGuid(),
                    Text: "Test Question?",
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
