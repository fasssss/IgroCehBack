namespace Domain.Entities
{
    // a game of a global event, always a catalog game: the VgcId of a game in Steam is the Steam app id the Steam achievements come from,
    // a game outside Steam (Application.Consts.CatalogIds) has achievements only on other platforms or none
    public class GlobalEventGame : IBaseEntity
    {
        public string Id { get; set; }
        public string GlobalEventId { get; set; }
        public GlobalEvent GlobalEvent { get; set; }
        public string GameId { get; set; }
        public Game Game { get; set; }
        // the order the administrator added the games in
        public int Order { get; set; }
        // the game on every platform with an id of its own its achievements belong to, e.g. in Xbox
        public List<GlobalEventGameTitle> Titles { get; set; } = new List<GlobalEventGameTitle>();
        // any one of them unlocked means the game is passed; without them the game is passed only by a claim checked by hand
        public List<GlobalEventAchievement> Achievements { get; set; } = new List<GlobalEventAchievement>();
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
