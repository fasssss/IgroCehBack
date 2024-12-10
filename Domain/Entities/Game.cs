using System.Numerics;

namespace Domain.Entities
{
    public class Game: IBaseEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] ImageContent { get; set; }
        public string ImageType { get; set; }
        public string SteamUrl { get; set; }
        public List<EventRecord> EventRecords { get; set; }
    }
}
