using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
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
        private readonly IUserRepository _userRepository;

        public AuthorizationApplicationService(
            IAuthorizationService authorizationService,
            IUserRepository userRepository) 
        {
            _authorizationService = authorizationService;
            _userRepository = userRepository;
        }

        public async Task<AuthorizationResult> Authorize(string authCode)
        {
            var authResult = await _authorizationService.GetAuthorizationToken(authCode);
            var userData = await _authorizationService.GetUserObject(authResult.AccessToken);
            var guildsData = await _authorizationService.GetUsersGuilds(authResult.AccessToken);
            var existingUser = await _userRepository.GetByIdAsync(userData.Id);
            var guildsEntities = new List<Guild>();

            foreach (var guild in guildsData)
            {
                guildsEntities.Add(new Guild
                {
                    Id = guild.Id,
                    Name = guild.Name,
                    AvatarHash = guild.Icon
                });
            }

            if(existingUser == null)
            {
                await _userRepository.AddAsync(new User
                {
                    Id = userData.Id,
                    UserName = userData.UserName,
                    Email = userData.Email,
                    AvatarHash = userData.Avatar,
                    Guilds = guildsEntities
                });
            }
            else
            {
                existingUser.Email = userData.Email;
                existingUser.AvatarHash = userData.Avatar;
                existingUser.UserName = userData.UserName;
                await _userRepository.UpdateUserGuildsAsync(existingUser.Id, guildsEntities);
            }

            await _userRepository.SaveAsync();

            return new AuthorizationResult(userData, authResult);
        }
    }
}
