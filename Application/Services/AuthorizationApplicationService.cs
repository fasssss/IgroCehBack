using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthorizationApplicationService: IAuthorizationApplicationService
    {
        private readonly IAuthorizationService _authorizationService;

        public AuthorizationApplicationService(IAuthorizationService authorizationService) 
        {
            _authorizationService = authorizationService;
        }

        public async Task<AuthorizationResult> Authorize(string authCode)
        {
            return await _authorizationService.GetAuthorizationToken(authCode);
        }
    }
}
