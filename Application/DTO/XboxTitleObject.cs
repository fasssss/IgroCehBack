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
        public List<PlatformAchievementObject> Achievements { get; set; }
    }
}
