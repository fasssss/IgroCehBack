using System.Text.Json;
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

    // the API of OpenXBL wraps the answer of Xbox Live into content with its status code
    public class OpenXblResponse<T>
    {
        public T Content { get; set; }
        // the status Xbox Live answered OpenXBL with, a number
        public JsonElement? Code { get; set; }
    }

    // a search of the Microsoft Store, the fields are named by the store
    public class OpenXblStoreSearch
    {
        public OpenXblStorePayload Payload { get; set; }
    }

    public class OpenXblStorePayload
    {
        public List<OpenXblStoreProduct> SearchResults { get; set; }
    }

    public class OpenXblStoreProduct
    {
        public string ProductId { get; set; }
        public string Title { get; set; }
        public List<OpenXblStoreImage> Images { get; set; }
        // added by enrich=titleid, empty for add-ons, bundles and PC-only listings; it may come as a number or as a string
        public JsonElement? XboxTitleId { get; set; }
    }

    public class OpenXblStoreImage
    {
        // BoxArt, Poster, Tile, Screenshot...
        public string ImageType { get; set; }
        public string Url { get; set; }
    }

    // a page of the achievements of a title, the next one comes by the continuation token
    public class OpenXblAchievements
    {
        public List<OpenXblAchievement> Achievements { get; set; }
        public OpenXblPagingInfo PagingInfo { get; set; }
    }

    public class OpenXblPagingInfo
    {
        // empty on the last page
        public string ContinuationToken { get; set; }
    }

    public class OpenXblAchievement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LockedDescription { get; set; }
        public bool IsSecret { get; set; }
        // Achieved, InProgress or NotStarted for the player asked about
        public string ProgressState { get; set; }
        public OpenXblProgression Progression { get; set; }
        public List<OpenXblMediaAsset> MediaAssets { get; set; }
        public OpenXblRarity Rarity { get; set; }
        public List<OpenXblTitleAssociation> TitleAssociations { get; set; }
    }

    public class OpenXblProgression
    {
        // 0001-01-01 while the achievement is locked
        public DateTimeOffset? TimeUnlocked { get; set; }
    }

    public class OpenXblMediaAsset
    {
        // Icon for the picture of the achievement
        public string Type { get; set; }
        public string Url { get; set; }
    }

    public class OpenXblRarity
    {
        public double? CurrentPercentage { get; set; }
    }

    public class OpenXblTitleAssociation
    {
        public string Name { get; set; }
        // title ids are unsigned 32-bit numbers
        public long Id { get; set; }
    }
}
