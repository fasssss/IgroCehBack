﻿using System.Numerics;

namespace Domain.Entities
{
    public class Game: IBaseEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? SteamUrl { get; set; }
        public List<EventRecord> EventRecords { get; set; }
        public string CreatorId { get; set; }
        public User Creator { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
