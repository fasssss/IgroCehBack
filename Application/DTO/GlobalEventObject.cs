using Application.Enum;

namespace Application.DTO
{
    public class GlobalEventObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        // the badge a player gets for passing the event, the name of a badge image of the front
        public string BadgeId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        // empty while the event is active
        public DateTimeOffset? FinishedAt { get; set; }
        // the players who rolled its games; with any of them the games can't change
        public int ParticipantCount { get; set; }
        // the participants who passed their picked game
        public int PassedCount { get; set; }
        public List<GlobalEventGameObject> Games { get; set; }
    }

    public class GlobalEventGameObject
    {
        public GameObject Game { get; set; }
        // the Steam app id, the same as the catalog id of the game
        public int AppId { get; set; }
        // the game in Xbox the Xbox achievements belong to, empty without them
        public string XboxTitleId { get; set; }
        public string XboxTitleName { get; set; }
        // any one of them unlocked means the game is passed; the Steam ones first
        public List<GlobalEventAchievementObject> Achievements { get; set; }
    }

    public class GlobalEventAchievementObject
    {
        public AchievementPlatform Platform { get; set; }
        // the API name in Steam or the id in Xbox
        public string ApiName { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
        public string Description { get; set; }
        public string DescriptionRu { get; set; }
        public string IconUrl { get; set; }
    }

    // a game the administrator puts into a new event with the achievements that mean it is passed
    public class NewGlobalEventGameObject
    {
        public int AppId { get; set; }
        // in Steam
        public List<string> AchievementApiNames { get; set; }
        // the game in Xbox, needed only with Xbox achievements
        public string XboxTitleId { get; set; }
        public List<string> XboxAchievementIds { get; set; } = new List<string>();
    }
}
