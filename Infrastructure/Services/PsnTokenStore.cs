using System.Net;
using System.Text.RegularExpressions;
using Application.Consts;
using Application.DTO;
using Application.Exceptions;
using Infrastructure.Configurations;
using Infrastructure.DTO;
using Infrastructure.ExternalInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace Infrastructure.Services
{
    // one for the whole site: all the requests share the tokens of the service account and only one of them gets new ones, the others wait for it.
    // An access token lives about an hour and a refresh gives a new one; a refresh token lives about 10 days and a refresh doesn't prolong it,
    // after that the NPSSO signs in again. Tokens are kept only in memory and never logged
    public class PsnTokenStore
    {
        // a token this close to its end isn't used, it could end on the way
        private static readonly TimeSpan ExpiryMargin = TimeSpan.FromMinutes(1);
        // "error_code=" holds "code=" too, so the parameters are matched whole
        private static readonly Regex CodeParameter = new("[?&]code=([^&]+)");
        private static readonly Regex ErrorCodeParameter = new("[?&]error_code=([^&]+)");

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PsnRateLimiter _limiter;
        private readonly PsnApiOptions _options;
        private readonly ILogger<PsnTokenStore> _logger;
        private readonly SemaphoreSlim _renewal = new(1, 1);
        private volatile Tokens _tokens;
        private DateTimeOffset _signInRetryAt = DateTimeOffset.MinValue;
        private volatile bool _isSignInRefused;
        private readonly SemaphoreSlim _npssoChecking = new(1, 1);
        private volatile NpssoCheck _npssoCheck;

        public PsnTokenStore(IServiceScopeFactory scopeFactory, PsnRateLimiter limiter, IOptions<PsnApiOptions> options, ILogger<PsnTokenStore> logger)
        {
            _scopeFactory = scopeFactory;
            _limiter = limiter;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            if (IsAlive(_tokens?.AccessExpiresAt))
            {
                return _tokens.AccessToken;
            }

            await _renewal.WaitAsync(ct);
            try
            {
                // another request may have renewed them while this one waited
                if (IsAlive(_tokens?.AccessExpiresAt))
                {
                    return _tokens.AccessToken;
                }

                // the store is one for the whole site, the clients of the API aren't kept in it so that their handlers are renewed
                using var scope = _scopeFactory.CreateScope();
                var authApi = scope.ServiceProvider.GetRequiredService<IPsnAuthApi>();
                _tokens = await RefreshAsync(authApi, ct) ?? await SignInAsync(authApi, ct);
                return _tokens.AccessToken;
            }
            finally
            {
                _renewal.Release();
            }
        }

        public async Task<PsnStatusObject> GetStatusAsync(CancellationToken ct)
        {
            var isSignInWorking = true;
            try
            {
                await GetAccessTokenAsync(ct);
            }
            catch (PsnUnavailableException)
            {
                isSignInWorking = false;
            }

            await CheckNpssoAsync(ct);
            var npssoCheck = _npssoCheck;
            return new PsnStatusObject
            {
                IsSignInWorking = isSignInWorking,
                IsNpssoRefused = (!isSignInWorking && _isSignInRefused) || npssoCheck?.IsRefused == true || string.IsNullOrWhiteSpace(_options.Npsso),
                NpssoExpiresAt = npssoCheck?.ExpiresAt,
                IsNpssoEnding = IsNpssoEnding(npssoCheck?.ExpiresAt),
                RefreshTokenExpiresAt = _tokens?.RefreshExpiresAt,
            };
        }

        // PSN refused the token before its time; another request may have brought a new one by now, which stays
        public void Invalidate(string accessToken)
        {
            var tokens = _tokens;
            if (tokens?.AccessToken == accessToken)
            {
                Interlocked.CompareExchange(ref _tokens, tokens with { AccessExpiresAt = DateTimeOffset.MinValue }, tokens);
            }
        }

        private static bool IsAlive(DateTimeOffset? expiresAt) => expiresAt > DateTimeOffset.UtcNow + ExpiryMargin;

        private static bool IsNpssoEnding(DateTimeOffset? expiresAt) => expiresAt < DateTimeOffset.UtcNow.AddDays(PsnLimits.NpssoWarningDays);

        // PSN tells how long the NPSSO has left and whether it works at all, which the live tokens hide for up to 10 days. An answer is kept for a while,
        // anything else (a failure, a block of the host against bots) says nothing about the NPSSO and is asked again next time; none of it affects the requests of the site
        private async Task CheckNpssoAsync(CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_options.Npsso))
            {
                return;
            }

            await _npssoChecking.WaitAsync(ct);
            try
            {
                if (_npssoCheck != null && DateTimeOffset.UtcNow < _npssoCheck.CheckedAt.AddSeconds(_options.NpssoCheckSeconds))
                {
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var authApi = scope.ServiceProvider.GetRequiredService<IPsnAuthApi>();
                _limiter.Acquire();
                using var response = await authApi.GetSsoCookie($"npsso={_options.Npsso}", ct);
                if (response.IsSuccessStatusCode && response.Content?.ExpiresIn > 0)
                {
                    var expiresAt = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
                    _npssoCheck = new NpssoCheck(DateTimeOffset.UtcNow, expiresAt, false);
                    if (IsNpssoEnding(expiresAt))
                    {
                        _logger.LogWarning("The NPSSO of the service account works until about {ExpiresAt:yyyy-MM-dd}, it needs renewing", expiresAt);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest && response.Error?.Content?.Contains("invalid_grant") == true)
                {
                    _npssoCheck = new NpssoCheck(DateTimeOffset.UtcNow, null, true);
                    _logger.LogError("PSN refused the NPSSO of the service account, it needs renewing");
                }
                else
                {
                    _logger.LogWarning("PSN answered the question about the end of the NPSSO with {StatusCode}", (int)response.StatusCode);
                }
            }
            catch (Exception e) when ((e is PsnUnavailableException or HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogWarning(e, "PSN didn't answer the question about the end of the NPSSO");
            }
            finally
            {
                _npssoChecking.Release();
            }
        }

        // null when the refresh token is gone or doesn't work, then the NPSSO signs in
        private async Task<Tokens> RefreshAsync(IPsnAuthApi authApi, CancellationToken ct)
        {
            if (!IsAlive(_tokens?.RefreshExpiresAt))
            {
                return null;
            }

            var response = await RequestTokensAsync(authApi, new Dictionary<string, string>
            {
                ["refresh_token"] = _tokens.RefreshToken,
                ["grant_type"] = "refresh_token",
                ["token_format"] = "jwt",
                ["scope"] = _options.Scope,
            }, ct);
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("PSN refused the refresh token with {StatusCode}, signing in again", (int)response.StatusCode);
                return null;
            }

            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content?.AccessToken))
            {
                _logger.LogError("PSN answered the refresh with {StatusCode}", (int)response.StatusCode);
                throw new PsnUnavailableException($"PSN answered the refresh with {(int)response.StatusCode}");
            }

            return ToTokens(response.Content);
        }

        private async Task<Tokens> SignInAsync(IPsnAuthApi authApi, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_options.Npsso))
            {
                _logger.LogError("There is no NPSSO of the service account");
                throw new PsnUnavailableException("There is no NPSSO of the service account");
            }

            // without them the sign-in can only be refused, and the log of a refusal would send to renew the NPSSO
            if (new[] { _options.ClientId, _options.ClientAuthorization, _options.RedirectUri, _options.Scope }.Any(string.IsNullOrWhiteSpace))
            {
                _logger.LogError("The client of the PlayStation App isn't in the settings");
                throw new PsnUnavailableException("The client of the PlayStation App isn't in the settings");
            }

            if (DateTimeOffset.UtcNow < _signInRetryAt)
            {
                throw new PsnUnavailableException("The last sign-in to PSN failed, it isn't repeated yet");
            }

            try
            {
                var code = await RequestCodeAsync(authApi, ct);
                var response = await RequestTokensAsync(authApi, new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["redirect_uri"] = _options.RedirectUri,
                    ["grant_type"] = "authorization_code",
                    ["token_format"] = "jwt",
                }, ct);
                if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(response.Content?.AccessToken))
                {
                    _logger.LogError("PSN answered the code of the sign-in with {StatusCode}", (int)response.StatusCode);
                    throw new PsnUnavailableException($"PSN answered the code of the sign-in with {(int)response.StatusCode}");
                }

                return ToTokens(response.Content);
            }
            catch (PsnUnavailableException)
            {
                _signInRetryAt = DateTimeOffset.UtcNow.AddSeconds(_options.SignInRetrySeconds);
                throw;
            }
        }

        // the address of the redirect carries the code, it isn't logged
        private async Task<string> RequestCodeAsync(IPsnAuthApi authApi, CancellationToken ct)
        {
            _limiter.Acquire();
            try
            {
                using var response = await authApi.Authorize($"npsso={_options.Npsso}", "offline", _options.ClientId, _options.RedirectUri, "code", _options.Scope, ct);
                var location = response.Headers.Location?.ToString() ?? "";
                var code = CodeParameter.Match(location);
                if (!code.Success)
                {
                    // 4165 means that the NPSSO doesn't work anymore; an answer without a redirect says nothing about the NPSSO
                    var errorCode = ErrorCodeParameter.Match(location);
                    _isSignInRefused = errorCode.Success;
                    _logger.LogError("PSN refused the sign-in of the service account with {StatusCode}, error code {ErrorCode}, the NPSSO may need renewing",
                        (int)response.StatusCode, errorCode.Success ? errorCode.Groups[1].Value : null);
                    throw new PsnUnavailableException("PSN refused the sign-in of the service account");
                }

                _isSignInRefused = false;
                return code.Groups[1].Value;
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "PSN can't be reached");
                throw new PsnUnavailableException("PSN can't be reached", e);
            }
        }

        private async Task<ApiResponse<PsnTokens>> RequestTokensAsync(IPsnAuthApi authApi, Dictionary<string, string> form, CancellationToken ct)
        {
            _limiter.Acquire();
            try
            {
                return await authApi.GetTokens(_options.ClientAuthorization, form, ct);
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "PSN can't be reached");
                throw new PsnUnavailableException("PSN can't be reached", e);
            }
        }

        private static Tokens ToTokens(PsnTokens tokens) => new(tokens.AccessToken, DateTimeOffset.UtcNow.AddSeconds(tokens.ExpiresIn),
            tokens.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(tokens.RefreshTokenExpiresIn));

        private record Tokens(string AccessToken, DateTimeOffset AccessExpiresAt, string RefreshToken, DateTimeOffset RefreshExpiresAt);

        private record NpssoCheck(DateTimeOffset CheckedAt, DateTimeOffset? ExpiresAt, bool IsRefused);
    }
}
