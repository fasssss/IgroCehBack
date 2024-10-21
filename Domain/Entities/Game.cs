using System.Numerics;

namespace Domain.Entities
{
    public class Game: IBaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string SteamUrl { get; set; }
        public List<EventRecord> EventRecords { get; set; }
    }
}
