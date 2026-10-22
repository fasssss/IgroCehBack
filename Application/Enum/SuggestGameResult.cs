namespace Application.Enum
{
    public enum SuggestGameResult
    {
        Suggested,
        // the record or the game doesn't exist
        NotFound,
        // only the participant of the record guesses its game
        Forbidden,
        // games are guessed and changed only while the event is at the guessing stage, before the creator activates it
        Unavailable,
        // the player the game is guessed for already got it in another event
        AlreadyReceived,
    }
}
