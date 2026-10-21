namespace Application.DTO
{
    public enum SubmitPassingResult
    {
        Submitted,
        NotFound,
        // only the one who guessed the game marks it as passed
        Forbidden,
        // the event isn't active, the game isn't guessed yet or it is already marked as passed
        Unavailable,
    }
}
