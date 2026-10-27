namespace Domain.Entities
{
    // a Steam achievement that means a game of a global event is passed; the names are kept, so the event page doesn't ask Steam
    public class GlobalEventAchievement : IBaseEntity
    {
        public string Id { get; set; }
        public string GlobalEventGameId { get; set; }
        public GlobalEventGame GlobalEventGame { get; set; }
        // the API name of the achievement in Steam, it is checked against the achievements of a player
        public string ApiName { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
        public string? Description { get; set; }
        public string? DescriptionRu { get; set; }
        public string? IconUrl { get; set; }
        // the order of the achievements in Steam
        public int Order { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
