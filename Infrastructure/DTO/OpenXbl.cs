using System.Text.Json.Serialization;

namespace Infrastructure.DTO
{
    public class OpenXblClaimRequest
    {
        public string Code { get; set; }
        // the public key of the app
        [JsonPropertyName("app_key")]
        public string AppKey { get; set; }
    }

    // the key of the player (it opens their account through OpenXBL) and the e-mail come too, they aren't read
    public class OpenXblClaim
    {
        public string Xuid { get; set; }
        public string Gamertag { get; set; }
    }
}
