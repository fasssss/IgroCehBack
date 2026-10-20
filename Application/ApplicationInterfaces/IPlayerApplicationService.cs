using Application.DTO;

namespace Application.ApplicationInterfaces
{
    public interface IPlayerApplicationService
    {
        // null when the player doesn't exist or shares no guild (or not the requested one) with the viewer,
        // the requested guild may be one where the player hasn't played yet, its statistics are empty then
        public Task<PlayerProfileObject> GetPlayerProfileAsync(string viewerId, string playerId, string guildId);
        public Task<PlayerHistoryObject> GetPlayerHistoryAsync(string viewerId, string playerId, string guildId, int startFrom);
    }
}
