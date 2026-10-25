namespace Application.Enum
{
    public enum RemoveFromEventResult
    {
        Removed,
        // the event doesn't exist or the player isn't in it
        NotFound,
        // a player leaves by themselves, others are removed by the creator
        Forbidden,
        // players leave only during the registration, later it would break the chain of guessed games
        Unavailable,
    }
}
