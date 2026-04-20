namespace OnlineSurvey.Domain.Enums;

[Flags]
public enum SurveyCollectedFields
{
    None  = 0,
    Name  = 1,
    Email = 2,
    Age   = 4
}
