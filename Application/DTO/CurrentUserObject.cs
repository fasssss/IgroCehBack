namespace Application.DTO
{
    // the signed in user about themselves; other players come as UserObject without the flags
    public class CurrentUserObject : UserObject
    {
        // set by hand in the database, opens constructing global events
        public bool IsAppAdmin { get; set; }
        // the linked Steam account, null until the user signs in to Steam from the site
        public string SteamId { get; set; }
    }
}
