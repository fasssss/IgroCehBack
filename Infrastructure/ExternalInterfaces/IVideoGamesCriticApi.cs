using Infrastructure.DTO;
using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // https://videogamescritic.com/developers
    public interface IVideoGamesCriticApi
    {
        [Get("/api/v1/search")]
        public Task<ApiResponse<VgcSearchResult>> SearchGames([AliasAs("q")] string query, int limit, CancellationToken ct);

        [Get("/api/v1/game/{id}")]
        public Task<ApiResponse<VgcGame>> GetGame(int id, CancellationToken ct);
    }
}
