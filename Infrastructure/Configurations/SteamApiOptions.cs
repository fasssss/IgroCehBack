namespace Infrastructure.Configurations
{
    public class SteamApiOptions
    {
        public string Address { get; set; } = "https://api.steampowered.com";

        // the sign-in through Steam (OpenID) lives on the community site
        public string CommunityAddress { get; set; } = "https://steamcommunity.com";

        // the Steam Web API key of a Steam account (steamcommunity.com/dev/apikey), from appsettings or the environment: SteamApiOptions__Key
        public string Key { get; set; }
    }
}
