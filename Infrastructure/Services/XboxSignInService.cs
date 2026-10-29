using System.Net;
using Application.Consts;
using Application.DTO;
using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.DTO;
using Infrastructure.ExternalInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace Infrastructure.Services
{
    // the app of OpenXBL sends the user to Microsoft, OpenXBL gets the code of Microsoft at its own address
    // and passes it on to the return address of the app; only OpenXBL can exchange it, for the account of the user
    public class XboxSignInService : IXboxSignInService
    {
        private readonly IOpenXblApi _openXblApi;
        private readonly XboxApiOptions _options;
        private readonly ILogger<XboxSignInService> _logger;

        public XboxSignInService(IOpenXblApi openXblApi, IOptions<XboxApiOptions> options, ILogger<XboxSignInService> logger)
        {
            _openXblApi = openXblApi;
            _options = options.Value;
            _logger = logger;
        }

        public string GetSignInUrl() => $"{_options.Address.TrimEnd('/')}/app/auth/{Uri.EscapeDataString(_options.PublicKey ?? "")}";

        public async Task<XboxAccountObject> ClaimAsync(string code, CancellationToken ct = default)
        {
            ApiResponse<OpenXblClaim> response;
            try
            {
                response = await _openXblApi.Claim(new OpenXblClaimRequest { Code = code, AppKey = _options.PublicKey }, ct);
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "OpenXBL can't be reached to claim a sign-in");
                throw new XboxUnavailableException("OpenXBL can't be reached", e);
            }

            // OpenXBL refuses an unknown, used or outdated code with 400
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                _logger.LogError(response.Error, "OpenXBL answered the claim with {StatusCode}", (int)response.StatusCode);
                throw new XboxUnavailableException($"OpenXBL answered with {(int)response.StatusCode}", response.Error);
            }

            var xuid = response.Content.Xuid;
            var gamertag = response.Content.Gamertag;
            if (string.IsNullOrEmpty(xuid) || xuid.Length > XboxLimits.XuidMaxLength || !xuid.All(char.IsAsciiDigit)
                || string.IsNullOrWhiteSpace(gamertag) || gamertag.Length > XboxLimits.GamertagMaxLength)
            {
                _logger.LogError("OpenXBL answered the claim without a valid account");
                return null;
            }

            return new XboxAccountObject { Xuid = xuid, Gamertag = gamertag };
        }
    }
}
