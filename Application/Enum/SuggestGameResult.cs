namespace Application.Enum
{
    public enum SuggestGameResult
    {
        Suggested,
        // the record or the game doesn't exist
        NotFound,
        // only the participant of the record guesses its game
        Forbidden,
        // games are guessed only while the event is at the guessing stage
        Unavailable,
    }
}
