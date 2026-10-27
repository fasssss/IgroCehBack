using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // the Steam account of a user, linked through the sign-in to Steam; its achievements prove the passing of global events,
    // the methods that ask Steam throw SteamUnavailableException
    public interface ISteamLinkApplicationService
    {
        public string GetSignInUrl(string returnTo, string realm);
        // the visibility of the game details is checked right away, null when Steam doesn't answer
        public Task<(LinkSteamResult Result, bool? IsGameDataPublic)> LinkAsync(string userId, IReadOnlyDictionary<string, string> parameters, string returnTo,
            CancellationToken ct = default);
        public Task UnlinkAsync(string userId);
        // null when the user has no Steam linked
        public Task<bool?> IsGameDataPublicAsync(string userId, CancellationToken ct = default);
    }
}
