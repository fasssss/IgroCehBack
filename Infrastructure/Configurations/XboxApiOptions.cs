namespace Infrastructure.Configurations
{
    // OpenXBL (https://api.xbl.io/docs/json) stands between the site and Xbox Live: the sign-in to Xbox and the achievements of the players
    public class XboxApiOptions
    {
        public string Address { get; set; } = "https://api.xbl.io";

        // the public key of the app in the dashboard of OpenXBL; the sign-in link carries it, so it isn't a secret
        public string PublicKey { get; set; }

        // the API key of the OpenXBL account (xbl.io dashboard, API Keys), a secret, from appsettings or the environment: XboxApiOptions__Key
        public string Key { get; set; }
    }
}
