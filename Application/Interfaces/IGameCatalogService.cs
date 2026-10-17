using Application.DTO;

namespace Application.Interfaces
{
    // external catalog of games, methods throw GameCatalogUnavailableException when catalog can't be reached
    public interface IGameCatalogService
    {
        public Task<List<CatalogGameObject>> SearchGamesAsync(string query, CancellationToken ct = default);
        public Task<CatalogGameObject> GetGameAsync(int vgcId, CancellationToken ct = default);
    }
}
