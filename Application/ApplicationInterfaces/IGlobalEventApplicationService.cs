using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // events of the whole site made by its administrator, a game of such an event is passed by a Steam or an Xbox achievement; the methods
    // that ask Steam, Xbox or the catalog throw SteamUnavailableException, XboxUnavailableException or GameCatalogUnavailableException
    public interface IGlobalEventApplicationService
    {
        public Task<List<SteamAchievementObject>> GetGameAchievementsAsync(int appId, CancellationToken ct = default);
        // the games in Xbox the administrator chooses from for a catalog game, found by its name
        public Task<List<XboxTitleObject>> SearchXboxTitlesAsync(string query, CancellationToken ct = default);
        // null when the game in Xbox has no achievements or there is no such game
        public Task<XboxTitleAchievementsObject> GetXboxAchievementsAsync(string titleId, CancellationToken ct = default);
        public Task<(CreateGlobalEventResult Result, GlobalEventObject GlobalEvent)> CreateAsync(string userId, string name, string description,
            string badgeId, List<NewGlobalEventGameObject> games, CancellationToken ct = default);
        public Task<(UpdateGlobalEventResult Result, GlobalEventObject GlobalEvent)> UpdateAsync(string userId, string globalEventId, string name,
            string description, string badgeId, List<NewGlobalEventGameObject> games, CancellationToken ct = default);
        public Task<ChangeGlobalEventResult> SetImageAsync(string globalEventId, string imageUrl);
        // null when no event is active
        public Task<GlobalEventObject> GetActiveAsync();
        // null when there is no such event or it is deleted
        public Task<GlobalEventObject> GetAsync(string globalEventId);
        public Task<ChangeGlobalEventResult> FinishAsync(string userId, string globalEventId);
        public Task<ChangeGlobalEventResult> DeleteAsync(string userId, string globalEventId);
    }
}
