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
    public class GuildApplicationService : IGuildApplicationService
    {
        private readonly IGuildRepository _guildRepository;

        public GuildApplicationService(IGuildRepository guildRepository) 
        {
            _guildRepository = guildRepository;
        }

        public async Task<ICollection<GuildObject>> GetFilteredGuildsAsync(long userId, GuildsFilter filter)
        {
            var guilds = await _guildRepository.GetGuildsByUserIdAsync(userId);
            var filteredGuilds = guilds.Where(g => g.Name.Contains(filter.SearchString));
            var guildObjects = new List<GuildObject>();
            foreach (var guild in filteredGuilds)
            {
                guildObjects.Add(new() {
                    Id = guild.Id,
                    Name = guild.Name,
                    IconUrl = guild.AvatarUrl,
                    OwnerId = guild.OwnerId,
                });
            }

            return guildObjects;
        }
    }
}
