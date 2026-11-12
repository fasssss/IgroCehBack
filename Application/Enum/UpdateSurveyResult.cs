namespace Application.Enum
{
    public enum UpdateSurveyResult
    {
        Updated,
        NotFound,
        // only the active survey changes
        Closed,
        // a user has answered already, the questions and their options can't change
        QuestionsLocked,
        // the survey was closed or answered while it was being saved
        ChangedMeanwhile,
    }
}
