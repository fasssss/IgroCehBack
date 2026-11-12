namespace Application.Enum
{
    // mirrors Domain.Enums.SurveyQuestionType for the API layer, which doesn't see the domain
    public enum QuestionType
    {
        SingleChoice = 1,
        MultipleChoice = 2,
        Text = 3,
    }
}
