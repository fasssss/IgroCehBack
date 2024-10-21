using Application.DTO;
using Infrastructure.DTO;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ExternalInterfaces
{
    public interface IDiscordApi
    {
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        [Post("/api/v10/oauth2/token")]
        public Task<GetAuthorizationTokenResult> GetTokens([Header("Authorization")] string basicAuthString, [Body(BodySerializationMethod.UrlEncoded)] FormUrlEncodedContent data);
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        [Get("/api/v10/users/@me")]
        public Task<DiscordUserObject> GetCurrentUser([Header("Authorization")] string jwtAuthString);
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        [Get("/api/v10/users/@me/guilds")]
        public Task<List<DiscordGuildObject>> GetCurrentUsersGuilds([Header("Authorization")] string jwtAuthString);
    }
}
