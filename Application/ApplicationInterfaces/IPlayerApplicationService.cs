using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    public interface IPlayerApplicationService
    {
        // null when the player doesn't exist or shares no guild with the viewer
        public Task<PlayerProfileObject> GetPlayerProfileAsync(string viewerId, string playerId);
        // the events of the guilds shared with the viewer, newest first
        public Task<PlayerHistoryObject> GetPlayerHistoryAsync(string viewerId, string playerId, int startFrom);
        // null when the viewer can't see the profile of the player
        public Task<PlayerLadderObject> GetPlayerLadderAsync(string viewerId, string playerId);
        // null when the viewer can't see the profile of the player
        public Task<PlayerGamesObject> GetPlayerGamesAsync(string viewerId, string playerId, PlayerGamesKind kind, PlayerGamesSort sort, int startFrom);
        public Task<List<PreferenceTagObject>> GetPreferenceTagsAsync();
        // null when the viewer can't see the profile of the player
        public Task<PlayerPreferencesObject> GetPlayerPreferencesAsync(string viewerId, string playerId);
        public Task<SetPreferencesResult> SetPreferencesAsync(string userId, List<int> preferredTagIds, List<int> avoidedTagIds);
        public Task<List<GameAspectObject>> GetGameAspectsAsync();
        // UnknownTag for an id out of the catalog of aspects
        public Task<SetPreferencesResult> SetLikedAspectsAsync(string userId, List<int> aspectIds);
    }
}
