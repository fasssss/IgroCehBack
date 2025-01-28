using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GuildApplicationService : IGuildApplicationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGuildRepository _guildRepository;

        public GuildApplicationService(IGuildRepository guildRepository, IUserRepository userRepository) 
        {
            _guildRepository = guildRepository;
            _userRepository = userRepository;
        }

        public async Task<ICollection<GuildObject>> GetFilteredGuildsAsync(string userId, GuildsFilter filter)
        {
            var guilds = await _guildRepository.GetGuildsByUserIdAsync(userId);
            var filteredGuilds = guilds.Where(g => g.Name.Contains(filter.SearchString, StringComparison.OrdinalIgnoreCase));
            var guildObjects = new List<GuildObject>();
            foreach (var guild in filteredGuilds)
            {
                guildObjects.Add(new() {
                    Id = guild.Id,
                    Name = guild.Name,
                    IconUrl = guild.AvatarUrl,
                });
            }

            return guildObjects;
        }

        public async Task<GuildObject> GetGuildByIdAsync(string userId, string guildId)
        {
            var guild = await _guildRepository.FirstOrDefaultAsync(g => g.Id == guildId && g.UserGuilds.Any(u => u.UserId == userId));
            if(guild != null)
            {
                var guildObject = new GuildObject()
                {
                    IconUrl = guild.AvatarUrl,
                    Name = guild.Name,
                    Id = guild.Id,
                };

                return guildObject;
            }

            return null;
        }

        public async Task<ICollection<ScoreObject>> GetScoreByGuildIdAsync(string guildId)
        {
            var userGuildsForCertainGuild = await _userRepository.GetUserGuildsByGuildIdAsync(guildId);
    
            var userEventsStatistic = await _userRepository.CustomToListAsync(
                _userRepository
                .Where(user => user.UserGuilds.Any(ug => ug.GuildId == guildId), true)
                .Select(user => new ScoreObject()
                {
                    UserId = user.Id,
                    UserName = user.UserName, 
                    AvatarUrl = user.AvatarUrl,
                    Score = user.UserGuilds.FirstOrDefault(ug => ug.GuildId == guildId).Score,
                    EventsPlayed = user.EventRecords.Where(evr => evr.Event.GuildId == guildId).Count()
                }));

            return userEventsStatistic;
        }
    }
}
