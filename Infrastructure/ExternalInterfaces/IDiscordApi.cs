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
        [Post("/api/oauth2/token")]
        public Task<string> GetTokens([Header("Authorization")] string basicAuthString);
    }
}
