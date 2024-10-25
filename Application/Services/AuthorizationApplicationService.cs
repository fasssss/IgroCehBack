using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public AuthorizationApplicationService(
            IAuthorizationService authorizationService,
            IUserRepository userRepository,
            IMapper mapper) 
        {
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<AuthorizationResult> AuthorizeAsync(string authCode)
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
                    AvatarUrl = guild.IconUrl,
                    OwnerId = guild.OwnerId,
                });
            }

            if(existingUser == null)
            {
                await _userRepository.AddAsync(new User
                {
                    Id = userData.Id,
                    UserName = userData.UserName,
                    Email = userData.Email,
                    AvatarUrl = userData.AvatarUrl,
                    Guilds = guildsEntities
                });
            }
            else
            {
                existingUser.Email = userData.Email;
                existingUser.AvatarUrl = userData.AvatarUrl;
                existingUser.UserName = userData.UserName;
                await _userRepository.UpdateUserGuildsAsync(existingUser.Id, guildsEntities);
            }

            await _userRepository.SaveAsync();

            return new AuthorizationResult(userData, authResult);
        }

        public async Task<UserObject> GetUserObjectAsync(long userId)
        {
            var existingUser = await _userRepository.GetByIdAsync(userId);
            if(existingUser != null)
            {
                return  new UserObject() 
                {
                    AvatarUrl = existingUser.AvatarUrl,
                    UserName = existingUser.UserName,
                    Email = existingUser.Email,
                };
            }

            return null;
        }
    }
}
