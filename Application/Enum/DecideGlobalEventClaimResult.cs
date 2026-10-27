namespace Application.Enum
{
    public enum DecideGlobalEventClaimResult
    {
        Decided,
        NotFound,
        // another administrator has decided it already
        AlreadyDecided,
    }
}
