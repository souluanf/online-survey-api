using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Pages.Surveys;
using OnlineSurvey.Web.Services;

namespace OnlineSurvey.Web.Tests.Pages;

public class SurveyResultsTests : MudBunitContext
{
    private readonly Mock<ISurveyApiService> _surveyServiceMock;

    public SurveyResultsTests()
    {
        _surveyServiceMock = new Mock<ISurveyApiService>();
        Services.AddSingleton(_surveyServiceMock.Object);
    }

    [Fact]
    public void ShouldShowProgressCircular_WhenLoading()
    {
        
        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(It.IsAny<Guid>()))
            .Returns(new TaskCompletionSource<SurveyResultResponse?>().Task);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        
        cut.FindAll(".mud-progress-circular").Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldShowNotFoundMessage_WhenResultsIsNull()
    {
        
        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((SurveyResultResponse?)null);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, Guid.NewGuid()));

        
        cut.WaitForState(() => cut.Markup.Contains("não encontrados"));
        cut.Markup.Should().Contain("Resultados não encontrados");
    }

    [Fact]
    public async Task ShouldRenderSurveyTitle()
    {
        
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Customer Satisfaction Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        
        cut.WaitForState(() => cut.Markup.Contains("Customer Satisfaction Survey"));
        cut.Markup.Should().Contain("Customer Satisfaction Survey");
    }

    [Fact]
    public async Task ShouldRenderTotalResponses()
    {
        
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey", totalResponses: 42);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        
        cut.WaitForState(() => cut.Markup.Contains("42"));
        cut.Markup.Should().Contain("Total de respostas");
        cut.Markup.Should().Contain("42");
    }

    [Fact]
    public async Task ShouldRenderQuestions()
    {
        
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        
        cut.WaitForState(() => cut.Markup.Contains("Sample Question"));
        cut.Markup.Should().Contain("Sample Question");
    }

    [Fact]
    public async Task ShouldRenderOptions_WithCountsAndPercentages()
    {
        
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        
        cut.WaitForState(() => cut.Markup.Contains("Option A"));
        cut.Markup.Should().Contain("Option A");
        cut.Markup.Should().Contain("Option B");
        cut.Markup.Should().Contain("votos");
    }

    [Fact]
    public async Task ShouldRenderProgressBars()
    {
        
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        
        cut.WaitForState(() => cut.Markup.Contains("mud-progress-linear"));
        var progressBars = cut.FindAll(".mud-progress-linear");
        progressBars.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForHighPercentage()
    {
        
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "High Vote", 60, 60.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("mud-progress-linear"));
        cut.Find(".mud-progress-linear").ClassList.Should().Contain("mud-progress-linear-color-success");
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForMediumPercentage()
    {
        
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Medium Vote", 30, 30.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("mud-progress-linear"));
        cut.Find(".mud-progress-linear").ClassList.Should().Contain("mud-progress-linear-color-info");
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForLowPercentage()
    {
        
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Low Vote", 15, 15.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("mud-progress-linear"));
        cut.Find(".mud-progress-linear").ClassList.Should().Contain("mud-progress-linear-color-warning");
    }

    [Fact]
    public async Task ShouldApplyCorrectProgressBarColor_ForVeryLowPercentage()
    {
        
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 100,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Very Low Vote", 5, 5.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        cut.WaitForState(() => cut.Markup.Contains("mud-progress-linear"));
        cut.Find(".mud-progress-linear").ClassList.Should().Contain("mud-progress-linear-color-default");
    }

    [Fact]
    public async Task ShouldRenderBackToSurveysLink()
    {
        
        var surveyId = Guid.NewGuid();
        var results = CreateSurveyResultResponse(surveyId, "Survey");

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        
        cut.WaitForState(() => cut.Markup.Contains("Voltar"));
        cut.Markup.Should().Contain("Voltar às Pesquisas");
        // MudButton with Href renders as anchor
        var backLink = cut.Find("a[href='/surveys']");
        backLink.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldRenderMultipleQuestions()
    {
        
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Multi-Question Survey",
            TotalResponses: 50,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question 1",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option 1A", 30, 60.0),
                        new OptionResultResponse(Guid.NewGuid(), "Option 1B", 20, 40.0)
                    ]),
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question 2",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option 2A", 25, 50.0),
                        new OptionResultResponse(Guid.NewGuid(), "Option 2B", 25, 50.0)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        
        cut.WaitForState(() => cut.Markup.Contains("Question 1"));
        cut.Markup.Should().Contain("Question 1");
        cut.Markup.Should().Contain("Question 2");
        // MudCard renders with mud-card class
        cut.FindAll(".mud-card").Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ShouldFormatPercentage_WithOneDecimalPlace()
    {
        
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Survey",
            TotalResponses: 3,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option", 1, 33.33)
                    ])
            ]);

        _surveyServiceMock
            .Setup(x => x.GetSurveyResultsAsync(surveyId))
            .ReturnsAsync(results);

        
        var cut = Render<SurveyResults>(parameters =>
            parameters.Add(p => p.SurveyId, surveyId));

        cut.Markup.Should().Contain("33.3%");
    }

    private static SurveyResultResponse CreateSurveyResultResponse(
        Guid surveyId,
        string title,
        int totalResponses = 10)
    {
        return new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: title,
            TotalResponses: totalResponses,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Sample Question?",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option A", 6, 60.0),
                        new OptionResultResponse(Guid.NewGuid(), "Option B", 4, 40.0)
                    ])
            ]);
    }
}
