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
        private readonly IGuildRepository _guildRepository;
        private readonly IMapper _mapper;

        public AuthorizationApplicationService(
            IAuthorizationService authorizationService,
            IUserRepository userRepository,
            IGuildRepository guildRepository,
            IMapper mapper) 
        {
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _guildRepository = guildRepository;
            _mapper = mapper;
        }

        public async Task<AuthorizationResult> AuthorizeAsync(string authCode)
        {
            var authResult = await _authorizationService.GetAuthorizationToken(authCode);
            var userData = await _authorizationService.GetUserObject(authResult.AccessToken);
            var guildsData = await _authorizationService.GetUsersGuilds(authResult.AccessToken);
            var existingUser = await _userRepository.GetByIdAsync(userData.Id);
            var guildsEntities = new List<Guild>();
            var userGuildEntities = new List<UserGuild>();

            if (existingUser == null)
            {
                await _userRepository.AddAsync(new User
                {
                    Id = userData.Id,
                    UserName = userData.UserName,
                    Email = userData.Email,
                    AvatarUrl = userData.AvatarUrl,
                });
            }
            else
            {
                existingUser.Email = userData.Email;
                existingUser.AvatarUrl = userData.AvatarUrl;
                existingUser.UserName = userData.UserName;
            }

            var existingCopyGuilds = await _guildRepository.CustomToListAsync(_guildRepository.Where(g => guildsData.Any(gd => g.Id == gd.Id)));
            foreach (var guild in guildsData)
            {
                var existingGuild = existingCopyGuilds.FirstOrDefault(eg => eg.Id == guild.Id);
                if (existingGuild == null)
                {
                    await _guildRepository.AddAsync(new Guild
                    {
                        Id = guild.Id,
                        Name = guild.Name,
                        AvatarUrl = guild.IconUrl,
                    });
                }
                else
                {
                    existingGuild.Name = guild.Name;
                    existingGuild.AvatarUrl = guild.IconUrl;
                }

                userGuildEntities.Add(new UserGuild()
                {
                    UserId = userData.Id,
                    GuildId = guild.Id,
                    IsAdmin = (long.Parse(guild.Permissions) & 0x8) == 0x8 ? true : false,
                });
            };

            await _userRepository.UpdateUserGuildsAsync(userData.Id, userGuildEntities);

            await _userRepository.SaveAsync();

            return new AuthorizationResult(userData, authResult);
        }

        public async Task<UserObject> GetUserObjectAsync(string userId)
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
