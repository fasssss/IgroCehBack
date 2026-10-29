namespace Application.DTO
{
    // the roll of a player in a global event: the games that dropped, the one picked and when it can be refused
    public class GlobalEventParticipationObject
    {
        // the Steam app ids of the rolled games in the order they dropped
        public List<int> RolledAppIds { get; set; }
        public DateTimeOffset RolledAt { get; set; }
        // empty until the player picks one of the rolled games
        public int? PickedAppId { get; set; }
        public DateTimeOffset? PickedAt { get; set; }
        // from then on the picked game can be refused for a new roll, empty until a game is picked
        public DateTimeOffset? RefusableAt { get; set; }
        // the picked game is passed, the participation is over
        public DateTimeOffset? PassedAt { get; set; }
        // the last check of the passing through Steam and from when the next one is allowed, both empty before the first check
        public DateTimeOffset? SteamCheckedAt { get; set; }
        public DateTimeOffset? SteamCheckableAt { get; set; }
        // the last claim for the picked game, empty without one
        public GlobalEventClaimObject Claim { get; set; }
    }
}
