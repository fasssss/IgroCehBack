using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    public interface IPlayerApplicationService
    {
        // null when the player doesn't exist or shares no guild (or not the requested one) with the viewer,
        // the requested guild may be one where the player hasn't played yet, its statistics are empty then
        public Task<PlayerProfileObject> GetPlayerProfileAsync(string viewerId, string playerId, string guildId);
        public Task<PlayerHistoryObject> GetPlayerHistoryAsync(string viewerId, string playerId, string guildId, int startFrom);
        public Task<List<PreferenceTagObject>> GetPreferenceTagsAsync();
        // null when the viewer can't see the profile of the player
        public Task<PlayerPreferencesObject> GetPlayerPreferencesAsync(string viewerId, string playerId);
        public Task<SetPreferencesResult> SetPreferencesAsync(string userId, List<int> preferredTagIds, List<int> avoidedTagIds);
    }
}
