namespace Infrastructure.Configurations
{
    public class SteamApiOptions
    {
        public string Address { get; set; } = "https://api.steampowered.com";

        // the Steam Web API key of a Steam account (steamcommunity.com/dev/apikey), comes from the environment: SteamApiOptions__Key
        public string Key { get; set; }
    }
}
