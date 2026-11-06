
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
        // an account of a platform can belong to several users: one player can have several Discord accounts
        // SteamID64 of the Steam account the user signed in to through Steam
        public string? SteamId { get; set; }
        // the Xbox account the user signed in to through OpenXBL; the gamertag is the one of the last sign-in
        public string? XboxXuid { get; set; }
        public string? XboxGamertag { get; set; }
        // the PSN account whose About me showed the code given to the user; the online id is the one of the last link
        public string? PsnAccountId { get; set; }
        public string? PsnOnlineId { get; set; }
        public List<UserGuild> UserGuilds { get; set; }
        public List<EventRecord> EventRecords { get; set; }
        public List<Event> CreatorOfEvents { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
