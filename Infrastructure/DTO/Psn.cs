using System.Text.Json.Serialization;

namespace Infrastructure.DTO
{
    // the tokens of the account, the lifetimes in seconds: about an hour and about 10 days
    public class PsnTokens
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; set; }
    }

    public class PsnProfileResponse
    {
        public PsnProfile Profile { get; set; }
    }

    public class PsnProfile
    {
        public string AccountId { get; set; }
        public string OnlineId { get; set; }
        public string AboutMe { get; set; }
    }

    // a page of the trophies of a game, the next one comes by the offset; empty on the last page
    public class PsnTrophies
    {
        public List<PsnTrophy> Trophies { get; set; }
        public int? NextOffset { get; set; }
    }

    // a trophy of a game (the names) or of a player (earned), the other fields are empty
    public class PsnTrophy
    {
        public int TrophyId { get; set; }
        public bool TrophyHidden { get; set; }
        public string TrophyName { get; set; }
        public string TrophyDetail { get; set; }
        public string TrophyIconUrl { get; set; }
        public bool Earned { get; set; }
        public DateTimeOffset? EarnedDateTime { get; set; }
    }

    public class PsnTrophyGroups
    {
        public string TrophyTitleName { get; set; }
    }
}
