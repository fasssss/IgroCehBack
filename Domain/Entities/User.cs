
namespace Domain.Entities
{
    public class User: IBaseEntity
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        // administrator of the whole application, unlike UserGuild.IsAdmin that comes from a Discord guild
        public bool IsAppAdmin { get; set; }
        // SteamID64 of the Steam account the user signed in to through Steam, one user per Steam account
        public string? SteamId { get; set; }
        // the Xbox account the user signed in to through OpenXBL, one user per account; the gamertag is the one of the last sign-in
        public string? XboxXuid { get; set; }
        public string? XboxGamertag { get; set; }
        public List<UserGuild> UserGuilds { get; set; }
        public List<EventRecord> EventRecords { get; set; }
        public List<Event> CreatorOfEvents { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
