using OnlineSurvey.Application.DTOs;
using OnlineSurvey.Application.Interfaces;
using OnlineSurvey.Domain.Entities;
using OnlineSurvey.Domain.Enums;
using OnlineSurvey.Domain.Exceptions;
using OnlineSurvey.Domain.Repositories;

namespace OnlineSurvey.Application.Services;

public class SurveyService : ISurveyService
{
    private const string SurveyNotFoundMessage = "Survey not found.";
    private readonly IUnitOfWork _unitOfWork;

    public SurveyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SurveyDetailResponse> CreateSurveyAsync(CreateSurveyRequest request, string ownerId = "", CancellationToken cancellationToken = default)
    {
        var survey = new Survey(request.Title, request.Description, ownerId);

        foreach (var questionDto in request.Questions.OrderBy(q => q.Order))
        {
            var question = new Question(survey.Id, questionDto.Text, questionDto.Order, questionDto.IsRequired);

            foreach (var optionDto in questionDto.Options.OrderBy(o => o.Order))
            {
                var option = new Option(question.Id, optionDto.Text, optionDto.Order);
                question.AddOption(option);
            }

            survey.AddQuestion(question);
        }

        await _unitOfWork.Surveys.AddAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(survey);
    }

    public async Task<SurveyDetailResponse?> GetSurveyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(id, cancellationToken);
        if (survey is null)
            return null;

        return MapToDetailResponse(survey);
    }

    public async Task<PaginatedResponse<SurveyResponse>> GetSurveysAsync(
        int page,
        int pageSize,
        string ownerId = "",
        SurveyStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var (surveys, totalCount) = string.IsNullOrEmpty(ownerId)
            ? await _unitOfWork.Surveys.GetPaginatedAsync(page, pageSize, status, cancellationToken)
            : await _unitOfWork.Surveys.GetPaginatedByOwnerAsync(ownerId, page, pageSize, status, cancellationToken);

        var items = new List<SurveyResponse>();
        foreach (var survey in surveys)
        {
            var responseCount = await _unitOfWork.Responses.GetResponseCountBySurveyIdAsync(survey.Id, cancellationToken);
            items.Add(MapToResponse(survey, responseCount));
        }

        return new PaginatedResponse<SurveyResponse>(
            items,
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }

    public async Task<IEnumerable<SurveyResponse>> GetActiveSurveysAsync(CancellationToken cancellationToken = default)
    {
        var surveys = await _unitOfWork.Surveys.GetActiveSurveysAsync(cancellationToken);

        var result = new List<SurveyResponse>();
        foreach (var survey in surveys)
        {
            var responseCount = await _unitOfWork.Responses.GetResponseCountBySurveyIdAsync(survey.Id, cancellationToken);
            result.Add(MapToResponse(survey, responseCount));
        }

        return result;
    }

    public async Task<SurveyDetailResponse> UpdateSurveyAsync(Guid id, UpdateSurveyRequest request, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(id, cancellationToken)
            ?? throw new DomainException(SurveyNotFoundMessage);

        if (survey.Status != SurveyStatus.Draft)
            throw new DomainException("Only draft surveys can be updated.");

        survey.SetTitle(request.Title);
        survey.SetDescription(request.Description);

        await _unitOfWork.Surveys.UpdateAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(survey);
    }

    public async Task<SurveyDetailResponse> ActivateSurveyAsync(Guid id, ActivateSurveyRequest request, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(id, cancellationToken)
            ?? throw new DomainException(SurveyNotFoundMessage);

        survey.Activate(request.StartDate, request.EndDate);

        await _unitOfWork.Surveys.UpdateAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(survey);
    }

    public async Task<SurveyDetailResponse> CloseSurveyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdWithQuestionsAndOptionsAsync(id, cancellationToken)
            ?? throw new DomainException(SurveyNotFoundMessage);

        survey.Close();

        await _unitOfWork.Surveys.UpdateAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDetailResponse(survey);
    }

    public async Task DeleteSurveyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var survey = await _unitOfWork.Surveys.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException(SurveyNotFoundMessage);

        if (survey.Status == SurveyStatus.Active)
            throw new DomainException("Cannot delete an active survey. Close it first.");

        await _unitOfWork.Surveys.DeleteAsync(survey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static SurveyResponse MapToResponse(Survey survey, int responseCount) =>
        new(
            survey.Id,
            survey.Title,
            survey.Description,
            survey.Status,
            survey.StartDate,
            survey.EndDate,
            survey.Questions.Count,
            responseCount,
            survey.CreatedAt,
            survey.UpdatedAt
        );

    private static SurveyDetailResponse MapToDetailResponse(Survey survey) =>
        new(
            survey.Id,
            survey.Title,
            survey.Description,
            survey.Status,
            survey.StartDate,
            survey.EndDate,
            survey.Questions
                .OrderBy(q => q.Order)
                .Select(q => new QuestionResponse(
                    q.Id,
                    q.Text,
                    q.Order,
                    q.IsRequired,
                    q.Options
                        .OrderBy(o => o.Order)
                        .Select(o => new OptionResponse(o.Id, o.Text, o.Order))
                        .ToList()
                ))
                .ToList(),
            survey.CreatedAt,
            survey.UpdatedAt
        );
}
