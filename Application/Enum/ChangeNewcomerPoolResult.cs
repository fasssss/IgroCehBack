namespace Application.Enum
{
    public enum ChangeNewcomerPoolResult
    {
        Changed,
        // the event, the game or the game of the pool doesn't exist
        NotFound,
        // only a participant puts games into the pool and only the author changes them
        Forbidden,
        // the event has no pool or doesn't gather players any more
        Unavailable,
        AlreadyInPool,
        // the participant has put as many games as one may
        LimitReached,
        // the game has dropped to a newcomer
        Dropped,
    }
}
