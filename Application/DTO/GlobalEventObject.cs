namespace Application.DTO
{
    public class GlobalEventObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        // empty while the event is active
        public DateTimeOffset? FinishedAt { get; set; }
        // the players who rolled its games; with any of them the games can't change
        public int ParticipantCount { get; set; }
        public List<GlobalEventGameObject> Games { get; set; }
    }

    public class GlobalEventGameObject
    {
        public GameObject Game { get; set; }
        // the Steam app id, the same as the catalog id of the game
        public int AppId { get; set; }
        // any one of them unlocked means the game is passed
        public List<GlobalEventAchievementObject> Achievements { get; set; }
    }

    public class GlobalEventAchievementObject
    {
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
        public List<string> AchievementApiNames { get; set; }
    }
}
