namespace Application.Enum
{
    public enum ChangeSurveyResult
    {
        Changed,
        NotFound,
        // the survey is closed already, or was being closed by someone else at the same moment
        Unavailable,
    }
}
