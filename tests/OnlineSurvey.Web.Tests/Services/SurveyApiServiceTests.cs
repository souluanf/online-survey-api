using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using OnlineSurvey.Web.Models;
using OnlineSurvey.Web.Services;
using RichardSzalay.MockHttp;

namespace OnlineSurvey.Web.Tests.Services;

public class SurveyApiServiceTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly SurveyApiService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public SurveyApiServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("http://localhost/");
        _service = new SurveyApiService(httpClient);
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    #region GetActiveSurveysAsync Tests

    [Fact]
    public async Task GetActiveSurveysAsync_ShouldReturnSurveys()
    {
        
        var surveys = new List<SurveyResponse>
        {
            CreateSurveyResponse("Survey 1"),
            CreateSurveyResponse("Survey 2")
        };

        _mockHttp.When("http://localhost/api/surveys/active")
            .Respond("application/json", JsonSerializer.Serialize(surveys, _jsonOptions));

        
        var result = await _service.GetActiveSurveysAsync();

        
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveSurveysAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        
        _mockHttp.When("http://localhost/api/surveys/active")
            .Respond("application/json", "[]");

        
        var result = await _service.GetActiveSurveysAsync();

        
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveSurveysAsync_WhenNullResponse_ShouldReturnEmptyList()
    {
        
        _mockHttp.When("http://localhost/api/surveys/active")
            .Respond("application/json", "null");

        
        var result = await _service.GetActiveSurveysAsync();

        
        result.Should().BeEmpty();
    }

    #endregion

    #region GetSurveysAsync Tests

    [Fact]
    public async Task GetSurveysAsync_ShouldReturnPaginatedResponse()
    {
        
        var paginatedResponse = new PaginatedResponse<SurveyResponse>(
            Items: [CreateSurveyResponse("Survey 1")],
            Page: 1,
            PageSize: 50,
            TotalCount: 1,
            TotalPages: 1
        );

        _mockHttp.When("http://localhost/api/surveys?page=1&pageSize=50")
            .Respond("application/json", JsonSerializer.Serialize(paginatedResponse, _jsonOptions));

        
        var result = await _service.GetSurveysAsync();

        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetSurveysAsync_WithStatusFilter_ShouldIncludeStatusInUrl()
    {
        
        var paginatedResponse = new PaginatedResponse<SurveyResponse>(
            Items: [],
            Page: 1,
            PageSize: 10,
            TotalCount: 0,
            TotalPages: 0
        );

        _mockHttp.When("http://localhost/api/surveys?page=1&pageSize=10&status=1")
            .Respond("application/json", JsonSerializer.Serialize(paginatedResponse, _jsonOptions));

        
        var result = await _service.GetSurveysAsync(page: 1, pageSize: 10, status: SurveyStatus.Active);

        
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSurveysAsync_WithCustomPagination_ShouldUseCorrectParameters()
    {
        
        var paginatedResponse = new PaginatedResponse<SurveyResponse>(
            Items: [],
            Page: 2,
            PageSize: 25,
            TotalCount: 30,
            TotalPages: 2
        );

        _mockHttp.When("http://localhost/api/surveys?page=2&pageSize=25")
            .Respond("application/json", JsonSerializer.Serialize(paginatedResponse, _jsonOptions));

        
        var result = await _service.GetSurveysAsync(page: 2, pageSize: 25);

        
        result.Should().NotBeNull();
        result!.Page.Should().Be(2);
        result.PageSize.Should().Be(25);
    }

    #endregion

    #region GetSurveyByIdAsync Tests

    [Fact]
    public async Task GetSurveyByIdAsync_WhenExists_ShouldReturnSurvey()
    {
        
        var surveyId = Guid.NewGuid();
        var survey = CreateSurveyDetailResponse(surveyId, "Test Survey");

        _mockHttp.When($"http://localhost/api/surveys/{surveyId}")
            .Respond("application/json", JsonSerializer.Serialize(survey, _jsonOptions));

        
        var result = await _service.GetSurveyByIdAsync(surveyId);

        
        result.Should().NotBeNull();
        result!.Id.Should().Be(surveyId);
        result.Title.Should().Be("Test Survey");
    }

    [Fact]
    public async Task GetSurveyByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        
        var surveyId = Guid.NewGuid();

        _mockHttp.When($"http://localhost/api/surveys/{surveyId}")
            .Respond(HttpStatusCode.NotFound);

         
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetSurveyByIdAsync(surveyId));
    }

    #endregion

    #region GetSurveyResultsAsync Tests

    [Fact]
    public async Task GetSurveyResultsAsync_ShouldReturnResults()
    {
        
        var surveyId = Guid.NewGuid();
        var results = new SurveyResultResponse(
            SurveyId: surveyId,
            SurveyTitle: "Test Survey",
            TotalResponses: 10,
            Questions:
            [
                new QuestionResultResponse(
                    QuestionId: Guid.NewGuid(),
                    QuestionText: "Question 1?",
                    Options:
                    [
                        new OptionResultResponse(Guid.NewGuid(), "Option A", 6, 60.0),
                        new OptionResultResponse(Guid.NewGuid(), "Option B", 4, 40.0)
                    ])
            ]);

        _mockHttp.When($"http://localhost/api/responses/surveys/{surveyId}/results")
            .Respond("application/json", JsonSerializer.Serialize(results, _jsonOptions));

        
        var result = await _service.GetSurveyResultsAsync(surveyId);

        
        result.Should().NotBeNull();
        result!.TotalResponses.Should().Be(10);
        result.Questions.Should().HaveCount(1);
    }

    #endregion

    #region SubmitResponseAsync Tests

    [Fact]
    public async Task SubmitResponseAsync_WhenSuccessful_ShouldReturnTrue()
    {
        
        var request = new SubmitResponseRequest(
            SurveyId: Guid.NewGuid(),
            ParticipantId: "participant1",
            Answers: [new AnswerRequest(Guid.NewGuid(), Guid.NewGuid())]
        );

        _mockHttp.When(HttpMethod.Post, "http://localhost/api/responses")
            .Respond(HttpStatusCode.Created);

        
        var result = await _service.SubmitResponseAsync(request);

        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitResponseAsync_WhenFailed_ShouldReturnFalse()
    {
        
        var request = new SubmitResponseRequest(
            SurveyId: Guid.NewGuid(),
            ParticipantId: null,
            Answers: []
        );

        _mockHttp.When(HttpMethod.Post, "http://localhost/api/responses")
            .Respond(HttpStatusCode.BadRequest);

        
        var result = await _service.SubmitResponseAsync(request);

        
        result.Should().BeFalse();
    }

    #endregion

    #region CreateSurveyAsync Tests

    [Fact]
    public async Task CreateSurveyAsync_WhenSuccessful_ShouldReturnSurvey()
    {
        
        var request = new CreateSurveyRequest(
            Title: "New Survey",
            Description: "Description",
            Questions:
            [
                new CreateQuestionRequest(
                    Text: "Question 1?",
                    Order: 1,
                    IsRequired: true,
                    Options:
                    [
                        new CreateOptionRequest("Option A", 1),
                        new CreateOptionRequest("Option B", 2)
                    ])
            ]);

        var responseData = CreateSurveyDetailResponse(Guid.NewGuid(), "New Survey");

        _mockHttp.When(HttpMethod.Post, "http://localhost/api/surveys")
            .Respond("application/json", JsonSerializer.Serialize(responseData, _jsonOptions));

        
        var result = await _service.CreateSurveyAsync(request);

        
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Survey");
    }

    [Fact]
    public async Task CreateSurveyAsync_WhenFailed_ShouldReturnNull()
    {
        
        var request = new CreateSurveyRequest(
            Title: "",
            Description: null,
            Questions: []
        );

        _mockHttp.When(HttpMethod.Post, "http://localhost/api/surveys")
            .Respond(HttpStatusCode.BadRequest);

        
        var result = await _service.CreateSurveyAsync(request);

        
        result.Should().BeNull();
    }

    #endregion

    #region ActivateSurveyAsync Tests

    [Fact]
    public async Task ActivateSurveyAsync_WhenSuccessful_ShouldReturnSurvey()
    {
        
        var surveyId = Guid.NewGuid();
        var request = new ActivateSurveyRequest(
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(7)
        );

        var responseData = CreateSurveyDetailResponse(surveyId, "Activated Survey");

        _mockHttp.When(HttpMethod.Post, $"http://localhost/api/surveys/{surveyId}/activate")
            .Respond("application/json", JsonSerializer.Serialize(responseData, _jsonOptions));

        
        var result = await _service.ActivateSurveyAsync(surveyId, request);

        
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateSurveyAsync_WhenFailed_ShouldReturnNull()
    {
        
        var surveyId = Guid.NewGuid();
        var request = new ActivateSurveyRequest(null, null);

        _mockHttp.When(HttpMethod.Post, $"http://localhost/api/surveys/{surveyId}/activate")
            .Respond(HttpStatusCode.BadRequest);

        
        var result = await _service.ActivateSurveyAsync(surveyId, request);

        
        result.Should().BeNull();
    }

    #endregion

    #region DeleteSurveyAsync Tests

    [Fact]
    public async Task DeleteSurveyAsync_WhenSuccessful_ShouldReturnTrue()
    {
        
        var surveyId = Guid.NewGuid();

        _mockHttp.When(HttpMethod.Delete, $"http://localhost/api/surveys/{surveyId}")
            .Respond(HttpStatusCode.NoContent);

        
        var result = await _service.DeleteSurveyAsync(surveyId);

        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSurveyAsync_WhenNotFound_ShouldReturnFalse()
    {
        
        var surveyId = Guid.NewGuid();

        _mockHttp.When(HttpMethod.Delete, $"http://localhost/api/surveys/{surveyId}")
            .Respond(HttpStatusCode.NotFound);

        
        var result = await _service.DeleteSurveyAsync(surveyId);

        
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateSurveyAsync Tests

    [Fact]
    public async Task UpdateSurveyAsync_WhenSuccessful_ShouldReturnSurvey()
    {
        
        var surveyId = Guid.NewGuid();
        var request = new UpdateSurveyRequest(
            Title: "Updated Title",
            Description: "Updated Description"
        );

        var responseData = CreateSurveyDetailResponse(surveyId, "Updated Title");

        _mockHttp.When(HttpMethod.Put, $"http://localhost/api/surveys/{surveyId}")
            .Respond("application/json", JsonSerializer.Serialize(responseData, _jsonOptions));

        
        var result = await _service.UpdateSurveyAsync(surveyId, request);

        
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateSurveyAsync_WhenFailed_ShouldReturnNull()
    {
        
        var surveyId = Guid.NewGuid();
        var request = new UpdateSurveyRequest("", null);

        _mockHttp.When(HttpMethod.Put, $"http://localhost/api/surveys/{surveyId}")
            .Respond(HttpStatusCode.BadRequest);

        
        var result = await _service.UpdateSurveyAsync(surveyId, request);

        
        result.Should().BeNull();
    }

    #endregion

    #region CloseSurveyAsync Tests

    [Fact]
    public async Task CloseSurveyAsync_WhenSuccessful_ShouldReturnSurvey()
    {
        
        var surveyId = Guid.NewGuid();
        var responseData = CreateSurveyDetailResponse(surveyId, "Closed Survey");

        _mockHttp.When(HttpMethod.Post, $"http://localhost/api/surveys/{surveyId}/close")
            .Respond("application/json", JsonSerializer.Serialize(responseData, _jsonOptions));

        
        var result = await _service.CloseSurveyAsync(surveyId);

        
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CloseSurveyAsync_WhenFailed_ShouldReturnNull()
    {
        
        var surveyId = Guid.NewGuid();

        _mockHttp.When(HttpMethod.Post, $"http://localhost/api/surveys/{surveyId}/close")
            .Respond(HttpStatusCode.BadRequest);

        
        var result = await _service.CloseSurveyAsync(surveyId);

        
        result.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static SurveyResponse CreateSurveyResponse(string title)
    {
        return new SurveyResponse(
            Id: Guid.NewGuid(),
            Title: title,
            Description: "Description",
            Status: SurveyStatus.Active,
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddDays(7),
            QuestionCount: 1,
            ResponseCount: 0,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }

    private static SurveyDetailResponse CreateSurveyDetailResponse(Guid id, string title)
    {
        return new SurveyDetailResponse(
            Id: id,
            Title: title,
            Description: "Description",
            Status: SurveyStatus.Draft,
            StartDate: null,
            EndDate: null,
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

    #endregion
}
