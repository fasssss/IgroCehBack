using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // the Xbox account of a user, linked through the sign-in to Xbox via OpenXBL,
    // the methods that ask OpenXBL throw XboxUnavailableException
    public interface IXboxLinkApplicationService
    {
        public string GetSignInUrl();
        // the gamertag of the linked account, null unless it is linked
        public Task<(LinkXboxResult Result, string Gamertag)> LinkAsync(string userId, string code, CancellationToken ct = default);
        public Task UnlinkAsync(string userId);
    }
}
