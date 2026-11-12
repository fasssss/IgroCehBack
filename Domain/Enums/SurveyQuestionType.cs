namespace Domain.Enums
{
    public enum SurveyQuestionType
    {
        // one of the options
        SingleChoice = 1,
        // several options, up to SurveyQuestion.MaxSelections when it is set
        MultipleChoice = 2,
        // a text the respondent writes, such a question has no options
        Text = 3,
    }
}
