using System.Numerics;

namespace Domain.Entities
{
    public class Game: IBaseEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? SteamUrl { get; set; }
        // id of the game in VideoGamesCritic catalog, set for games picked from the catalog search
        public int? VgcId { get; set; }
        // platforms from VideoGamesCritic catalog, empty for games added by hand
        public List<string> Platforms { get; set; } = new List<string>();
        public List<EventRecord> EventRecords { get; set; }
        public string CreatorId { get; set; }
        public User Creator { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
