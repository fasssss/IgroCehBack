namespace Application.Enum
{
    // mirrors Domain.Enums.GlobalEventClaimStatus for the API layer, which doesn't see the domain
    public enum ClaimStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
    }
}
