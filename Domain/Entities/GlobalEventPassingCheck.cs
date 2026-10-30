using Domain.Enums;

namespace Domain.Entities
{
    // the last automatic check of the passing of a participant on one platform, the next one there is allowed a while later
    // (GlobalEventLimits.PassingCheckDelayMinutes); a platform the participant never checked on has none
    public class GlobalEventPassingCheck : IBaseEntity
    {
        public string Id { get; set; }
        public string ParticipantId { get; set; }
        public GlobalEventParticipant Participant { get; set; }
        public GlobalEventAchievementPlatform Platform { get; set; }
        public DateTimeOffset CheckedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
