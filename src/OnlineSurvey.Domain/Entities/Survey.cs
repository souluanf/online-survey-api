using OnlineSurvey.Domain.Enums;
using OnlineSurvey.Domain.Exceptions;

namespace OnlineSurvey.Domain.Entities;

public class Survey : Entity
{
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public SurveyStatus Status { get; private set; }
    public string OwnerId { get; private set; } = null!;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public SurveyAccessMode AccessMode { get; private set; } = SurveyAccessMode.Anonymous;
    public SurveyCollectedFields CollectedFields { get; private set; } = SurveyCollectedFields.None;
    public bool IsPublic { get; private set; } = true;

    private readonly List<Question> _questions = [];
    public IReadOnlyCollection<Question> Questions => _questions.AsReadOnly();

    private readonly List<Response> _responses = [];
    public IReadOnlyCollection<Response> Responses => _responses.AsReadOnly();

    private Survey() { }

    public Survey(string title, string? description = null, string ownerId = "",
        SurveyAccessMode accessMode = SurveyAccessMode.Anonymous,
        SurveyCollectedFields collectedFields = SurveyCollectedFields.None,
        bool isPublic = true)
    {
        SetTitle(title);
        Description = description;
        OwnerId = ownerId;
        Status = SurveyStatus.Draft;
        AccessMode = accessMode;
        CollectedFields = collectedFields;
        IsPublic = isPublic;
    }

    public void Configure(SurveyAccessMode accessMode, SurveyCollectedFields collectedFields, bool isPublic)
    {
        AccessMode = accessMode;
        CollectedFields = collectedFields;
        IsPublic = isPublic;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Survey title cannot be empty.");

        if (title.Length > 200)
            throw new DomainException("Survey title cannot exceed 200 characters.");

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDescription(string? description)
    {
        if (description?.Length > 1000)
            throw new DomainException("Survey description cannot exceed 1000 characters.");

        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (Status != SurveyStatus.Draft)
            throw new DomainException("Only draft surveys can be activated.");

        if (_questions.Count == 0)
            throw new DomainException("Survey must have at least one question to be activated.");

        if (_questions.Any(q => q.Options.Count < 2))
            throw new DomainException("All questions must have at least 2 options.");

        StartDate = startDate ?? DateTime.UtcNow;
        EndDate = endDate;
        Status = SurveyStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status != SurveyStatus.Active)
            throw new DomainException("Only active surveys can be closed.");

        Status = SurveyStatus.Closed;
        EndDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddQuestion(Question question)
    {
        if (Status != SurveyStatus.Draft)
            throw new DomainException("Cannot add questions to a non-draft survey.");

        if (_questions.Count >= 50)
            throw new DomainException("Survey cannot have more than 50 questions.");

        _questions.Add(question);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveQuestion(Guid questionId)
    {
        if (Status != SurveyStatus.Draft)
            throw new DomainException("Cannot remove questions from a non-draft survey.");

        var question = _questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
            throw new DomainException("Question not found.");

        _questions.Remove(question);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsOpen => Status == SurveyStatus.Active &&
                          (EndDate is null || EndDate > DateTime.UtcNow);
}
