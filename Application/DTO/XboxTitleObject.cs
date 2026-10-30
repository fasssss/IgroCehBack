namespace Application.DTO
{
    // a game in Xbox: the achievements, the stats and the title history are kept by its title id
    public class XboxTitleObject
    {
        // empty for a game the strict search found until its title id is looked up by its product
        public string TitleId { get; set; }
        // the product of the Microsoft Store, empty for a game known only by its title id
        public string ProductId { get; set; }
        public string Name { get; set; }
        // the box art of the store, empty when the store has none
        public string ImageUrl { get; set; }
    }

    // the achievements of a game in Xbox in the order of their ids
    public class XboxTitleAchievementsObject
    {
        public string TitleId { get; set; }
        public string Name { get; set; }
        public List<XboxAchievementObject> Achievements { get; set; }
    }

    // an achievement of a game in Xbox in both languages of the site, the fields are the same as of SteamAchievementObject
    public class XboxAchievementObject
    {
        // the id of the achievement in Xbox, e.g. "1", unique within the game
        public string ApiName { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
        public string Description { get; set; }
        public string DescriptionRu { get; set; }
        public string IconUrl { get; set; }
        // the share of the players who unlocked it
        public double? Percent { get; set; }
        // a secret achievement, Xbox gives its name and description anyway
        public bool IsHidden { get; set; }
    }
}
