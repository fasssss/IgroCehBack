using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

namespace API.Helpers
{
    // the sign-in through Steam returns to the front with a state bound to the user who started it,
    // so an answer of Steam opened by someone else can't link a stranger's account to them
    public class SteamLinkState
    {
        // the page of the front that takes the answer of Steam
        private const string LinkPagePath = "/steam-link";
        private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

        private readonly ITimeLimitedDataProtector _protector;
        private readonly string _frontendUrl;

        public SteamLinkState(IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
        {
            _protector = dataProtectionProvider.CreateProtector("SteamLink").ToTimeLimitedDataProtector();
            _frontendUrl = configuration.GetValue<string>("FrontendUrl")!.TrimEnd('/');
        }

        // Steam names the site to the user and accepts only return addresses inside it
        public string Realm => _frontendUrl;

        // the user comes back to returnPath after the sign-in
        public string CreateReturnTo(string userId, string returnPath) => BuildReturnTo(returnPath, _protector.Protect(userId, Lifetime));

        // the address Steam had to send the user to, null when the state is another user's, outdated or forged
        public string? GetReturnTo(string userId, string? returnPath, string? state)
        {
            if (string.IsNullOrEmpty(returnPath) || string.IsNullOrEmpty(state))
            {
                return null;
            }

            try
            {
                return _protector.Unprotect(state) == userId ? BuildReturnTo(returnPath, state) : null;
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        // only a path inside the front, not another site
        public static bool IsLocalPath(string path) =>
            !string.IsNullOrEmpty(path) && path.StartsWith('/') && !path.StartsWith("//") && !path.StartsWith("/\\");

        private string BuildReturnTo(string returnPath, string state) =>
            $"{_frontendUrl}{LinkPagePath}?returnPath={Uri.EscapeDataString(returnPath)}&state={Uri.EscapeDataString(state)}";
    }
}
