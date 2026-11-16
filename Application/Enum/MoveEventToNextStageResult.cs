namespace Application.Enum
{
    public enum MoveEventToNextStageResult
    {
        Moved,
        NotFound,
        // only the creator moves the event
        Forbidden,
        // the requested stage isn't the next one or the event isn't ready for it
        Unavailable,
    }
}
