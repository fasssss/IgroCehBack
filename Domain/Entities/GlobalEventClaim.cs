using Domain.Enums;

namespace Domain.Entities
{
    // the passing of the picked game of a global event shown by a profile on a platform, an administrator of the site checks it by hand
    public class GlobalEventClaim : IBaseEntity
    {
        public string Id { get; set; }
        public string ParticipantId { get; set; }
        public GlobalEventParticipant Participant { get; set; }
        // the game that was picked when the claim was sent
        public string GlobalEventGameId { get; set; }
        public GlobalEventGame GlobalEventGame { get; set; }
        public GlobalEventClaimPlatform Platform { get; set; }
        // a name or a link the player types into the claim, it isn't kept anywhere else
        public string Profile { get; set; }
        public GlobalEventClaimStatus Status { get; set; }
        // the reason of a rejection for the player, may be empty
        public string? Comment { get; set; }
        public string? DecidedById { get; set; }
        public User? DecidedBy { get; set; }
        public DateTimeOffset? DecidedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
