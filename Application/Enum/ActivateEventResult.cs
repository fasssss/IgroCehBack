namespace Application.Enum
{
    public enum ActivateEventResult
    {
        Activated,
        NotFound,
        // only the creator of the event activates it
        Forbidden,
        // the event isn't at the guessing stage or not every participant has guessed a game yet
        Unavailable,
    }
}
