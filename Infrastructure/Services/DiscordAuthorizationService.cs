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

        public async Task<AuthorizationTokens> GetAuthorizationToken(string authCode)
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
            return _mapper.Map<AuthorizationTokens>(authorizationResult);
        }

        public async Task<UserObject> GetUserObject(string authToken)
        {
            var authHeader = $"Bearer {authToken}";
            var discordUserObject = await _discordApi.GetCurrentUser(authHeader);
            var userObject = _mapper.Map<UserObject>(discordUserObject);
            if (userObject.Avatar != null)
                userObject.AvatarUrl = $"{_discordApiOptions.ImagesBaseUrl}avatars/{userObject.Id}/{userObject.Avatar}" + (userObject.Avatar.StartsWith("a_") ? ".gif" : ".jpg");

            return userObject;
        }

        public async Task<List<GuildObject>> GetUsersGuilds(string authToken)
        {
            var authHeader = $"Bearer {authToken}";
            var discordListOfGuildObject = await _discordApi.GetCurrentUsersGuilds(authHeader);
            var userGuilds = _mapper.Map<List<GuildObject>>(discordListOfGuildObject);
            foreach ( var guildObject in userGuilds)
            {
                if (guildObject.Icon != null)
                    guildObject.IconUrl = $"{_discordApiOptions.ImagesBaseUrl}icons/{guildObject.Id}/{guildObject.Icon}" + (guildObject.Icon.StartsWith("a_") ? ".gif" : ".jpg");
            }
            return userGuilds;
        }
    }
}
