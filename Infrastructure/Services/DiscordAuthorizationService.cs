using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using Infrastructure.Configurations;
using Infrastructure.ExternalInterfaces;
using Microsoft.Extensions.Options;
using System.Text;

namespace Infrastructure.Services
{
    public class DiscordAuthorizationService : IAuthorizationService
    {
        private readonly IDiscordApi _discordApi;
        private readonly DiscordApiOptions _discordApiOptions;
        private readonly IMapper _mapper;

        public DiscordAuthorizationService(IDiscordApi discordApi,
            IOptions<DiscordApiOptions> discordApiOptions,
            IMapper mapper) 
        {
            _discordApi = discordApi;
            _discordApiOptions = discordApiOptions.Value;
            _mapper = mapper;
        }

        public async Task<AuthorizationResult> GetAuthorizationToken(string authCode)
        {
            var encodedClientIdAndSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_discordApiOptions.ClientId}:{_discordApiOptions.ClientSecret}"));
            var authHeader = $"Basic {encodedClientIdAndSecret}";
            List<KeyValuePair<string, string>> dataKeyPairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authCode),
                new KeyValuePair<string, string>("redirect_uri", _discordApiOptions.RedirectUri)
            };

            var data = new FormUrlEncodedContent(dataKeyPairs);
            var authorizationResult = await _discordApi.GetTokens(authHeader, data);
            return _mapper.Map<AuthorizationResult>(authorizationResult);
        }

        public async Task<UserObject> GetUserObject(string authToken)
        {
            var authHeader = $"Bearer {authToken}";
            var discordUserObject = await _discordApi.GetCurrentUser(authHeader);
            return new UserObject();
        }
    }
}
