using Domain.Enums;

namespace Domain.Entities
{
    // an achievement in Steam or in Xbox that means a game of a global event is passed; the names are kept, so the event page asks neither
    public class GlobalEventAchievement : IBaseEntity
    {
        public string Id { get; set; }
        public string GlobalEventGameId { get; set; }
        public GlobalEventGame GlobalEventGame { get; set; }
        public GlobalEventAchievementPlatform Platform { get; set; }
        // the API name of the achievement in Steam or its id in Xbox, it is checked against the achievements of a player on that platform
        public string ApiName { get; set; }
        public string Name { get; set; }
        public string NameRu { get; set; }
        public string? Description { get; set; }
        public string? DescriptionRu { get; set; }
        public string? IconUrl { get; set; }
        // by the platforms in the order of their values (Steam first), within one in the order of the platform
        public int Order { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
