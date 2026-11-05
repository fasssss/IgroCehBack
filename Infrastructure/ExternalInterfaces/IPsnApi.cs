using Infrastructure.DTO;
using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // the trophies, answers 404 for an id in the wrong service and for a game the player hasn't played
    public interface IPsnApi
    {
        // the trophy list of a game even if the service account never played it, the hidden trophies come with their names;
        // the names come in the language asked for when the game has it
        [Get("/api/trophy/v1/npCommunicationIds/{communicationId}/trophyGroups/all/trophies")]
        public Task<ApiResponse<PsnTrophies>> GetTitleTrophies([Header("Authorization")] string authorization, [Header("Accept-Language")] string language,
            string communicationId, string npServiceName, int? offset, CancellationToken ct);

        // the groups of the list, it's where the name of the game is
        [Get("/api/trophy/v1/npCommunicationIds/{communicationId}/trophyGroups")]
        public Task<ApiResponse<PsnTrophyGroups>> GetTitleTrophyGroups([Header("Authorization")] string authorization, string communicationId,
            string npServiceName, CancellationToken ct);

        // the trophies of a player in a game, 403 when the privacy settings of the player hide them
        [Get("/api/trophy/v1/users/{accountId}/npCommunicationIds/{communicationId}/trophyGroups/all/trophies")]
        public Task<ApiResponse<PsnTrophies>> GetPlayerTrophies([Header("Authorization")] string authorization, string accountId, string communicationId,
            string npServiceName, int? offset, CancellationToken ct);
    }
}
