using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ApplicationInterfaces
{
    public record AuthorizationResult(UserObject UserObject, AuthorizationTokens AuthorizationTokens);
    public interface IAuthorizationApplicationService
    {
        public Task<AuthorizationResult> AuthorizeAsync(string authCode);

        public Task<UserObject> GetUserObjectAsync(long userId);
    }
}
