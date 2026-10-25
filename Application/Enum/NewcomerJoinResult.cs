namespace Application.Enum
{
    public enum NewcomerJoinResult
    {
        Succeeded,
        NotFound,
        // only a member of the event's guild joins it
        Forbidden,
        // the event isn't active, has no pool or nothing in the pool can drop to the player
        Unavailable,
        // the player is in the event already
        AlreadyJoined,
        // fewer games than the minimum would stay to roll from
        TooManyExcluded,
    }
}
