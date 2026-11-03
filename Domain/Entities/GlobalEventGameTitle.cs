using Domain.Enums;

namespace Domain.Entities
{
    // the game on a platform where it has an id of its own (a title id in Xbox) its achievements belong to, one per platform;
    // a platform that knows the game by its catalog id (Steam) has none; the event keeps its own, so another event can't change it
    public class GlobalEventGameTitle : IBaseEntity
    {
        public string Id { get; set; }
        public string GlobalEventGameId { get; set; }
        public GlobalEventGame GlobalEventGame { get; set; }
        public GlobalEventAchievementPlatform Platform { get; set; }
        // the id of the game on the platform, its format is the platform's own
        public string TitleRef { get; set; }
        public string TitleName { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
