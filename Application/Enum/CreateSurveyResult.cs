namespace Application.Enum
{
    public enum CreateSurveyResult
    {
        Created,
        // another administrator published a survey or stopped the active one at the same moment, nothing is published
        ChangedMeanwhile,
    }
}
