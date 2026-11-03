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
        // the catalog id of the game (VgcId), its Steam app id when the game is in Steam
        public int AppId { get; set; }
        // the game on every platform with an id of its own its achievements belong to, e.g. in Xbox
        public List<GlobalEventGameTitleObject> Titles { get; set; }
        // the game in Xbox from Titles for a front of the old contract, removed once every front reads Titles
        public string XboxTitleId { get; set; }
        public string XboxTitleName { get; set; }
        // any one of them unlocked means the game is passed; by the platforms in the order of their values, within one in the order of the platform
        public List<GlobalEventAchievementObject> Achievements { get; set; }
    }

    public class GlobalEventGameTitleObject
    {
        public AchievementPlatform Platform { get; set; }
        // the id of the game on the platform, e.g. the title id in Xbox
        public string TitleRef { get; set; }
        public string TitleName { get; set; }
    }

    public class GlobalEventAchievementObject
    {
        public AchievementPlatform Platform { get; set; }
        // the id of the achievement on the platform: the API name in Steam, the id in Xbox
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
        // the achievements chosen on each platform
        public List<NewGlobalEventPlatformObject> Platforms { get; set; }
        // the old contract, sent by a tab of the administrator opened before the platforms came and read only without Platforms;
        // removed once every front sends Platforms
        public List<string> AchievementApiNames { get; set; }
        public string XboxTitleId { get; set; }
        public List<string> XboxAchievementIds { get; set; } = new List<string>();

        // Platforms, or the same built from the fields of the old contract
        public List<NewGlobalEventPlatformObject> PlatformsOrOldFields() => Platforms ?? new List<NewGlobalEventPlatformObject>
        {
            new NewGlobalEventPlatformObject { Platform = AchievementPlatform.Steam, AchievementIds = AchievementApiNames ?? new List<string>() },
            new NewGlobalEventPlatformObject { Platform = AchievementPlatform.Xbox, TitleRef = XboxTitleId, AchievementIds = XboxAchievementIds ?? new List<string>() },
        };
    }

    // the achievements of a game chosen on one platform; a platform without them is left out or comes with none
    public class NewGlobalEventPlatformObject
    {
        public AchievementPlatform Platform { get; set; }
        // the game on a platform where it has an id of its own (IAchievementPlatformCatalog), empty on the others
        public string TitleRef { get; set; }
        // the ids of the achievements on the platform
        public List<string> AchievementIds { get; set; }
    }
}
