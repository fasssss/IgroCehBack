namespace Application.DTO
{
    // an achievement of a game on a platform in both languages of the site
    public class PlatformAchievementObject
    {
        // the id of the achievement on the platform, unique within the game: the API name in Steam, e.g. "ACH_WIN", the id in Xbox, e.g. "1"
        public string ApiName { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
        public string Description { get; set; }
        public string DescriptionRu { get; set; }
        public string IconUrl { get; set; }
        // the share of the players who unlocked it, rare ones usually mean the game is passed
        public double? Percent { get; set; }
        // a secret achievement, the platforms give its name and description anyway
        public bool IsHidden { get; set; }
    }

    // the achievements of a catalog game on a platform the administrator picks from, in the order of the platform
    public class PlatformTitleAchievementsObject
    {
        // the game on the platform, null on a platform that knows the game by its catalog id (Steam)
        public string TitleRef { get; set; }
        public string TitleName { get; set; }
        public List<PlatformAchievementObject> Achievements { get; set; }
    }
}
