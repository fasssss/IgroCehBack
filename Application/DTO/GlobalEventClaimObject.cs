using Application.Enum;

namespace Application.DTO
{
    // a claim that the picked game of a global event is passed, with the profile that shows it
    public class GlobalEventClaimObject
    {
        public string Id { get; set; }
        // empty in the participation of the player themselves
        public PlayerShortObject Player { get; set; }
        // the Steam app id of the game the claim is about
        public int AppId { get; set; }
        public ClaimPlatform Platform { get; set; }
        public string Profile { get; set; }
        public ClaimStatus Status { get; set; }
        // the reason of a rejection, may be empty
        public string Comment { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? DecidedAt { get; set; }
        // the administrator who decided it, empty while it is under review and for the player themselves
        public PlayerShortObject DecidedBy { get; set; }
    }
}
