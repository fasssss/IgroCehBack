namespace Application.Enum
{
    public enum SubmitPassingResult
    {
        Submitted,
        NotFound,
        // only the one who guessed the game or an admin of the guild marks it as passed
        Forbidden,
        // the event isn't active, the game isn't guessed yet or it is already marked as passed
        Unavailable,
    }
}
