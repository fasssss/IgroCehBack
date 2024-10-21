using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthorizationService
    {
        public Task<AuthorizationTokens> GetAuthorizationToken(string authCode);
        public Task<UserObject> GetUserObject(string authToken);
        public Task<List<GuildObject>> GetUsersGuilds(string authToken);
    }
}
