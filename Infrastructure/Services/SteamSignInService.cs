using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.ExternalInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using System.Text.RegularExpressions;

namespace Infrastructure.Services
{
    public class SteamSignInService : ISteamSignInService
    {
        private const string OpenIdNamespace = "http://specs.openid.net/auth/2.0";
        private const string IdentifierSelect = "http://specs.openid.net/auth/2.0/identifier_select";
        // the claimed id is always on steamcommunity.com, even when the sign-in page is elsewhere (a mock in development)
        private static readonly Regex ClaimedId = new(@"^https://steamcommunity\.com/openid/id/(\d{17})$");
        // the fields the signature has to cover, otherwise a valid answer could carry another account
        private static readonly string[] SignedFields = { "op_endpoint", "claimed_id", "identity", "return_to", "response_nonce" };

        private readonly ISteamCommunityApi _steamCommunityApi;
        private readonly SteamApiOptions _options;
        private readonly ILogger<SteamSignInService> _logger;

        public SteamSignInService(ISteamCommunityApi steamCommunityApi, IOptions<SteamApiOptions> options, ILogger<SteamSignInService> logger)
        {
            _steamCommunityApi = steamCommunityApi;
            _options = options.Value;
            _logger = logger;
        }

        private string LoginAddress => $"{_options.CommunityAddress.TrimEnd('/')}/openid/login";

        public string GetSignInUrl(string returnTo, string realm)
        {
            var parameters = new Dictionary<string, string>
            {
                ["openid.ns"] = OpenIdNamespace,
                ["openid.mode"] = "checkid_setup",
                ["openid.return_to"] = returnTo,
                ["openid.realm"] = realm,
                // Steam asks the user which account to sign in to
                ["openid.identity"] = IdentifierSelect,
                ["openid.claimed_id"] = IdentifierSelect,
            };
            return LoginAddress + "?" + string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        }

        public async Task<string> VerifyAsync(IReadOnlyDictionary<string, string> parameters, string returnTo, CancellationToken ct = default)
        {
            string Get(string key) => parameters.TryGetValue(key, out var value) ? value : null;

            var claimedId = ClaimedId.Match(Get("openid.claimed_id") ?? "");
            var signed = (Get("openid.signed") ?? "").Split(',');
            if (Get("openid.ns") != OpenIdNamespace || Get("openid.mode") != "id_res" || Get("openid.op_endpoint") != LoginAddress
                || Get("openid.return_to") != returnTo || !claimedId.Success || Get("openid.identity") != Get("openid.claimed_id")
                || SignedFields.Except(signed).Any())
            {
                return null;
            }

            // Steam checks the signature itself: the same fields are sent back with another mode
            var check = parameters.Where(p => p.Key.StartsWith("openid.")).ToDictionary(p => p.Key, p => p.Value);
            check["openid.mode"] = "check_authentication";
            ApiResponse<string> response;
            try
            {
                response = await _steamCommunityApi.CheckAuthentication(check, ct);
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "Steam can't be reached to check a sign-in");
                throw new SteamUnavailableException("Steam can't be reached", e);
            }

            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                _logger.LogError(response.Error, "Steam answered the sign-in check with {StatusCode}", (int)response.StatusCode);
                throw new SteamUnavailableException($"Steam answered with {(int)response.StatusCode}", response.Error);
            }

            var isValid = response.Content.Split('\n').Any(line => line.Trim() == "is_valid:true");
            return isValid ? claimedId.Groups[1].Value : null;
        }
    }
}
