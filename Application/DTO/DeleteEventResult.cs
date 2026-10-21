namespace Application.DTO
{
    public enum DeleteEventResult
    {
        Deleted,
        NotFound,
        // only an admin of the guild can delete its events
        Forbidden,
    }
}
