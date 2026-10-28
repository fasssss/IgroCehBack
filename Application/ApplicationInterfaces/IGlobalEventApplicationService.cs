using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // events of the whole site made by its administrator, a game of such an event is passed by a Steam achievement;
    // the methods that ask Steam or the catalog throw SteamUnavailableException or GameCatalogUnavailableException
    public interface IGlobalEventApplicationService
    {
        public Task<List<SteamAchievementObject>> GetGameAchievementsAsync(int appId, CancellationToken ct = default);
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
