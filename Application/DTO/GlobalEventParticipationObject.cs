using Application.Enum;

namespace Application.DTO
{
    // the roll of a player in a global event: the games that dropped, the one picked and when it can be refused
    public class GlobalEventParticipationObject
    {
        // the catalog ids of the rolled games (VgcId) in the order they dropped
        public List<int> RolledAppIds { get; set; }
        public DateTimeOffset RolledAt { get; set; }
        // empty until the player picks one of the rolled games
        public int? PickedAppId { get; set; }
        public DateTimeOffset? PickedAt { get; set; }
        // from then on the picked game can be refused for a new roll, empty until a game is picked
        public DateTimeOffset? RefusableAt { get; set; }
        // the picked game is passed, the participation is over
        public DateTimeOffset? PassedAt { get; set; }
        // the platform whose achievement passed the game, empty for a claim
        public AchievementPlatform? PassedPlatform { get; set; }
        // the last check of the passing on each platform the player checked on
        public List<GlobalEventPassingCheckObject> PassingChecks { get; set; }
        // the last claim for the picked game, empty without one
        public GlobalEventClaimObject Claim { get; set; }
    }
}
