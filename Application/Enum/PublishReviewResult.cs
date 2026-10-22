namespace Application.Enum
{
    public enum PublishReviewResult
    {
        Published,
        NotFound,
        // only the player who passed the game writes the review, only the one who guessed it answers
        Forbidden,
        // the game isn't passed yet, or the review or the answers are already published
        Unavailable,
    }
}
