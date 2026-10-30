using Infrastructure.DTO;
using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // https://api.xbl.io/docs/json
    public interface IOpenXblApi
    {
        // the code the sign-in brought to the return address, exchanged for the account it belongs to; a code works once
        [Post("/app/claim")]
        public Task<ApiResponse<OpenXblClaim>> Claim([Body] OpenXblClaimRequest request, CancellationToken ct);

        // the games of the Microsoft Store; enrich=titleid adds the Xbox title ids, deviceFamily=Windows.Xbox leaves the Xbox listings that have them
        [Get("/v2/marketplace/search")]
        public Task<ApiResponse<OpenXblResponse<OpenXblStoreSearch>>> SearchStore([Header("X-Authorization")] string key, [AliasAs("q")] string query,
            string enrich, string deviceFamily, CancellationToken ct);

        // the achievements of a title as the owner of the key sees them, the whole list even for a title the owner never played;
        // 150 a page, the next page by the continuation token; an unknown title gives an empty list
        [Get("/v2/achievements/title/{titleId}")]
        public Task<ApiResponse<OpenXblResponse<OpenXblAchievements>>> GetTitleAchievements([Header("X-Authorization")] string key,
            [Header("Accept-Language")] string language, string titleId, string continuationToken, CancellationToken ct);

        // the achievements of a player in a title with their progress, all of them at once; Xbox Live answers 403
        // when the privacy settings of the player hide them, OpenXBL passes it either as its own status or in the code of the answer
        [Get("/v2/achievements/player/{xuid}/{titleId}")]
        public Task<ApiResponse<OpenXblResponse<OpenXblAchievements>>> GetPlayerAchievements([Header("X-Authorization")] string key, string xuid,
            string titleId, CancellationToken ct);
    }
}
