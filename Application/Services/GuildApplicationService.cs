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
        private readonly IGuildRepository _guildRepository;

        public GuildApplicationService(IGuildRepository guildRepository) 
        {
            _guildRepository = guildRepository;
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
    }
}
