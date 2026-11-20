using Application.Enum;

namespace Application.DTO
{
    // the ladder of a player in the profile: the Igrofest (the global events) now, the Regular roster when it appears
    public class PlayerLadderObject
    {
        public PlayerFestObject Fest { get; set; }
    }

    public class PlayerFestObject
    {
        // the picked and not yet passed game of the active global event, null without one
        public PlayerFestGameObject Current { get; set; }
        // the passed games of every global event, the latest first
        public List<PlayerFestGameObject> Passed { get; set; }
    }

    public class PlayerFestGameObject
    {
        public GameObject Game { get; set; }
        public string GlobalEventId { get; set; }
        public string GlobalEventName { get; set; }
        public DateTimeOffset? PickedAt { get; set; }
        public DateTimeOffset? PassedAt { get; set; }
        // null for a passing confirmed by an administrator
        public AchievementPlatform? PassedPlatform { get; set; }
    }
}
