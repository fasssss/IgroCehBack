namespace Domain.Entities
{
    // a game of a global event, always a catalog game: its VgcId is the Steam app id the Steam achievements come from
    public class GlobalEventGame : IBaseEntity
    {
        public string Id { get; set; }
        public string GlobalEventId { get; set; }
        public GlobalEvent GlobalEvent { get; set; }
        public string GameId { get; set; }
        public Game Game { get; set; }
        // the order the administrator added the games in
        public int Order { get; set; }
        // the game in Xbox the Xbox achievements belong to, empty without them; the event keeps its own, so another event can't change it
        public string? XboxTitleId { get; set; }
        public string? XboxTitleName { get; set; }
        // any one of them unlocked means the game is passed; without them the game is passed only by a claim checked by hand
        public List<GlobalEventAchievement> Achievements { get; set; } = new List<GlobalEventAchievement>();
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
