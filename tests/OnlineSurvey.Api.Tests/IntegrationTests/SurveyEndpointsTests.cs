using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Domain.Enums;
using OnlineSurvey.Infrastructure.Data;

namespace OnlineSurvey.Api.Tests.IntegrationTests;

public class SurveyEndpointsTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SurveyEndpointsTests(WebApplicationFactory<Program> factory)
    {
        var testDbName = "TestDb_" + Guid.NewGuid();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", "");

            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(testDbName));
            });
        });

        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateSurvey_WithValidRequest_ShouldReturn201()
    {
        
        var request = CreateValidSurveyRequest();

        
        var response = await _client.PostAsJsonAsync("/api/surveys", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var survey = await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        survey.Should().NotBeNull();
        survey!.Title.Should().Be(request.Title);
        survey.Status.Should().Be(SurveyStatus.Draft);
        survey.Questions.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateSurvey_WithInvalidRequest_ShouldReturn400()
    {
        
        var request = new CreateSurveyRequest("", null, []);

        
        var response = await _client.PostAsJsonAsync("/api/surveys", request);

        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSurveyById_WhenExists_ShouldReturn200()
    {
        
        var createdSurvey = await CreateSurveyAsync();

        
        var response = await _client.GetAsync($"/api/surveys/{createdSurvey.Id}");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var survey = await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        survey!.Id.Should().Be(createdSurvey.Id);
    }

    [Fact]
    public async Task GetSurveyById_WhenNotExists_ShouldReturn404()
    {
        
        var response = await _client.GetAsync($"/api/surveys/{Guid.NewGuid()}");

        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActivateSurvey_WhenDraft_ShouldChangeStatusToActive()
    {
        
        var createdSurvey = await CreateSurveyAsync();
        var activateRequest = new ActivateSurveyRequest(null, null);

        
        var response = await _client.PostAsJsonAsync($"/api/surveys/{createdSurvey.Id}/activate", activateRequest);

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var survey = await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        survey!.Status.Should().Be(SurveyStatus.Active);
    }

    [Fact]
    public async Task CloseSurvey_WhenActive_ShouldChangeStatusToClosed()
    {
        
        var createdSurvey = await CreateAndActivateSurveyAsync();

        
        var response = await _client.PostAsync($"/api/surveys/{createdSurvey.Id}/close", null);

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var survey = await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        survey!.Status.Should().Be(SurveyStatus.Closed);
    }

    [Fact]
    public async Task SubmitResponse_ToActiveSurvey_ShouldReturn201()
    {
        
        var survey = await CreateAndActivateSurveyAsync();
        var submitRequest = new SubmitResponseRequest(
            survey.Id,
            "participant123",
            [new AnswerRequest(survey.Questions[0].Id, survey.Questions[0].Options[0].Id)]
        );

        
        var response = await _client.PostAsJsonAsync("/api/responses", submitRequest);

        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetSurveyResults_AfterResponses_ShouldReturnAggregatedData()
    {
        
        var survey = await CreateAndActivateSurveyAsync();

        
        for (int i = 0; i < 3; i++)
        {
            var submitRequest = new SubmitResponseRequest(
                survey.Id,
                $"participant{i}",
                [new AnswerRequest(survey.Questions[0].Id, survey.Questions[0].Options[0].Id)]
            );
            await _client.PostAsJsonAsync("/api/responses", submitRequest);
        }

        
        var response = await _client.GetAsync($"/api/responses/surveys/{survey.Id}/results");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var results = await response.Content.ReadFromJsonAsync<SurveyResultResponse>();
        results!.TotalResponses.Should().Be(3);
        results.Questions[0].Options[0].Count.Should().Be(3);
        results.Questions[0].Options[0].Percentage.Should().Be(100);
    }

    [Fact]
    public async Task GetActiveSurveys_ShouldReturnOnlyActiveSurveys()
    {
        
        await CreateSurveyAsync(); // Draft
        await CreateAndActivateSurveyAsync(); // Active

        
        var response = await _client.GetAsync("/api/surveys/active");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var surveys = await response.Content.ReadFromJsonAsync<IEnumerable<SurveyResponse>>();
        surveys.Should().OnlyContain(s => s.Status == SurveyStatus.Active);
    }

    [Fact]
    public async Task DeleteSurvey_WhenDraft_ShouldReturn204()
    {
        
        var survey = await CreateSurveyAsync();

        
        var response = await _client.DeleteAsync($"/api/surveys/{survey.Id}");

        
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/surveys/{survey.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturn200()
    {
        
        var response = await _client.GetAsync("/health");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSurveys_WithPagination_ShouldReturnPaginatedResults()
    {
        
        await CreateSurveyAsync();
        await CreateSurveyAsync();
        await CreateSurveyAsync();

        
        var response = await _client.GetAsync("/api/surveys?page=1&pageSize=2");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.PageSize.Should().Be(2);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetSurveys_WithInvalidPage_ShouldUseDefaultPage()
    {
        
        await CreateSurveyAsync();

        
        var response = await _client.GetAsync("/api/surveys?page=-1&pageSize=10");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetSurveys_WithInvalidPageSize_ShouldUseDefaultPageSize()
    {
        
        await CreateSurveyAsync();

        
        var response = await _client.GetAsync("/api/surveys?page=1&pageSize=-5");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetSurveys_WithLargePageSize_ShouldCapAt100()
    {
        
        await CreateSurveyAsync();

        
        var response = await _client.GetAsync("/api/surveys?page=1&pageSize=500");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetSurveys_WithStatusFilter_ShouldReturnOnlyMatchingStatus()
    {
        
        await CreateSurveyAsync(); // Draft
        await CreateAndActivateSurveyAsync(); // Active

        
        var response = await _client.GetAsync("/api/surveys?status=1"); // Active = 1

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.Items.Should().OnlyContain(s => s.Status == SurveyStatus.Active);
    }

    [Fact]
    public async Task GetSurveys_WithDraftStatusFilter_ShouldReturnOnlyDrafts()
    {
        
        await CreateSurveyAsync(); // Draft
        await CreateAndActivateSurveyAsync(); // Active

        
        var response = await _client.GetAsync("/api/surveys?status=0"); // Draft = 0

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.Items.Should().OnlyContain(s => s.Status == SurveyStatus.Draft);
    }

    [Fact]
    public async Task GetSurveys_WithNoFilters_ShouldReturnAllSurveys()
    {
        
        await CreateSurveyAsync();
        await CreateAndActivateSurveyAsync();

        
        var response = await _client.GetAsync("/api/surveys");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateSurvey_WhenDraft_ShouldUpdateTitleAndDescription()
    {
        
        var survey = await CreateSurveyAsync();
        var updateRequest = new UpdateSurveyRequest("Updated Title", "Updated Description");

        
        var response = await _client.PutAsJsonAsync($"/api/surveys/{survey.Id}", updateRequest);

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        updated!.Title.Should().Be("Updated Title");
        updated.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateSurvey_ShouldPersistChanges()
    {
        
        var survey = await CreateSurveyAsync();
        var updateRequest = new UpdateSurveyRequest("Persisted Title", null);

        
        await _client.PutAsJsonAsync($"/api/surveys/{survey.Id}", updateRequest);
        var getResponse = await _client.GetAsync($"/api/surveys/{survey.Id}");

        
        var fetched = await getResponse.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        fetched!.Title.Should().Be("Persisted Title");
    }

    [Fact]
    public async Task ActivateSurvey_WithDates_ShouldSetStartAndEndDates()
    {
        
        var survey = await CreateSurveyAsync();
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(30);
        var activateRequest = new ActivateSurveyRequest(startDate, endDate);

        
        var response = await _client.PostAsJsonAsync($"/api/surveys/{survey.Id}/activate", activateRequest);

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activated = await response.Content.ReadFromJsonAsync<SurveyDetailResponse>();
        activated!.Status.Should().Be(SurveyStatus.Active);
        activated.StartDate.Should().BeCloseTo(startDate, TimeSpan.FromSeconds(1));
        activated.EndDate.Should().BeCloseTo(endDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetSurveys_WithZeroPage_ShouldUseDefaultPage()
    {
        
        await CreateSurveyAsync();

        
        var response = await _client.GetAsync("/api/surveys?page=0");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetSurveys_WithZeroPageSize_ShouldUseDefaultPageSize()
    {
        
        await CreateSurveyAsync();

        
        var response = await _client.GetAsync("/api/surveys?pageSize=0");

        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<SurveyResponse>>();
        result!.PageSize.Should().Be(10);
    }

    private static CreateSurveyRequest CreateValidSurveyRequest() => new(
        $"Test Survey {Guid.NewGuid():N}",
        "Test Description",
        [
            new CreateQuestionRequest(
                "What is your favorite color?",
                1,
                true,
                [
                    new CreateOptionRequest("Red", 1),
                    new CreateOptionRequest("Blue", 2),
                    new CreateOptionRequest("Green", 3)
                ]
            )
        ]
    );

    private async Task<SurveyDetailResponse> CreateSurveyAsync()
    {
        var request = CreateValidSurveyRequest();
        var response = await _client.PostAsJsonAsync("/api/surveys", request);
        return (await response.Content.ReadFromJsonAsync<SurveyDetailResponse>())!;
    }

    private async Task<SurveyDetailResponse> CreateAndActivateSurveyAsync()
    {
        var survey = await CreateSurveyAsync();
        var activateRequest = new ActivateSurveyRequest(null, null);
        var response = await _client.PostAsJsonAsync($"/api/surveys/{survey.Id}/activate", activateRequest);
        return (await response.Content.ReadFromJsonAsync<SurveyDetailResponse>())!;
    }
}
