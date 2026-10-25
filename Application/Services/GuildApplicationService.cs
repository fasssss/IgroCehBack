using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
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
        private readonly IEventRepository _eventRepository;

        public GuildApplicationService(IGuildRepository guildRepository, IUserRepository userRepository, IEventRepository eventRepository)
        {
            _guildRepository = guildRepository;
            _userRepository = userRepository;
            _eventRepository = eventRepository;
        }

        // guilds where the user took part in events, the one with the latest event first
        public async Task<List<PlayedGuildObject>> GetPlayedGuildsAsync(string userId)
        {
            var playedEvents = await _eventRepository.CustomToListAsync(_eventRepository
                // a newcomer is only the one who got a game in the event
                .Where(e => e.EventRecords.Any(er => er.ParticipantId == userId || er.ToUserId == userId))
                .Select(e => new PlayedEventRow
                {
                    GuildId = e.GuildId,
                    GuildName = e.Guild.Name,
                    GuildIconUrl = e.Guild.AvatarUrl,
                    StatusId = e.StatusId,
                    StartDate = e.StartDate,
                    CreatedAt = e.CreatedAt,
                }));

            return playedEvents
                .GroupBy(e => e.GuildId)
                .Select(guildEvents => new
                {
                    // events created before CreatedAt was tracked are ordered by their season start
                    LastPlayedAt = guildEvents.Max(e => e.CreatedAt ?? new DateTimeOffset(e.StartDate, TimeSpan.Zero)),
                    Guild = new PlayedGuildObject
                    {
                        Id = guildEvents.Key,
                        Name = guildEvents.First().GuildName,
                        IconUrl = guildEvents.First().GuildIconUrl,
                        EventsPlayed = guildEvents.Count(),
                        EventsInProgress = guildEvents.Count(e => e.StatusId != EventStatusId.Finished),
                    },
                })
                .OrderByDescending(g => g.LastPlayedAt)
                .ThenBy(g => g.Guild.Name)
                .Select(g => g.Guild)
                .ToList();
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
                    IsCurrentUserAdmin = await _userRepository.CustomAnyAsync(_userRepository
                        .Where(u => u.Id == userId && u.UserGuilds.Any(ug => ug.GuildId == guildId && ug.IsAdmin && !ug.IsDeleted))),
                };

                return guildObject;
            }

            return null;
        }

        public async Task<ICollection<ScoreObject>> GetScoreByGuildIdAsync(string guildId)
        {
    
            var userEventsStatistic = await _userRepository.CustomToListAsync(
                _userRepository
                .Where(user => user.UserGuilds.Any(ug => ug.GuildId == guildId), true)
                .Select(user => new ScoreObject()
                {
                    UserId = user.Id,
                    UserName = user.UserName, 
                    AvatarUrl = user.AvatarUrl,
                    EventsPlayed = user.EventRecords.Where(evr => evr.Event.GuildId == guildId).Count()
                }));

            // rewards of passed games count right away, the event doesn't have to be summarized
            var scores = (await _eventRepository.GetRewardScoresAsync(e => e.GuildId == guildId))
                .ToDictionary(score => score.UserId, score => score.Score);
            foreach (var userStatistic in userEventsStatistic)
            {
                userStatistic.Score = scores.GetValueOrDefault(userStatistic.UserId);
            }

            return userEventsStatistic;
        }

        private class PlayedEventRow
        {
            public string GuildId { get; set; }
            public string GuildName { get; set; }
            public string GuildIconUrl { get; set; }
            public EventStatusId StatusId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTimeOffset? CreatedAt { get; set; }
        }
    }
}
