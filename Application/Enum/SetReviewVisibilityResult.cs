namespace Application.Enum
{
    public enum SetReviewVisibilityResult
    {
        Changed,
        // the review isn't written yet
        NotFound,
        // only the author changes it
        Forbidden,
    }
}
