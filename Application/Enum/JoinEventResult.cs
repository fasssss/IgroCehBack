namespace Application.Enum
{
    public enum JoinEventResult
    {
        Joined,
        NotFound,
        // only a member of the event's guild joins it
        Forbidden,
        // the event doesn't gather players any more
        Unavailable,
        AlreadyJoined,
    }
}
