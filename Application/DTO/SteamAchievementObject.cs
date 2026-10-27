namespace Application.DTO
{
    // an achievement of a Steam game in both languages of the site
    public class SteamAchievementObject
    {
        // the API name of the achievement in Steam
        public string ApiName { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
        public string Description { get; set; }
        public string DescriptionRu { get; set; }
        public string IconUrl { get; set; }
        // the share of the players who unlocked it, rare ones usually mean the game is passed
        public double? Percent { get; set; }
        public bool IsHidden { get; set; }
    }
}
