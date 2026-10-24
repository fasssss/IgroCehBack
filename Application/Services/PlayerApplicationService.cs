using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    // the profile opens for a viewer who shares a guild with the player; totals of all guilds are the same for every viewer,
    // but guilds, events and other players are shown only from the shared guilds (players who left a guild are still shown there)
    public class PlayerApplicationService : IPlayerApplicationService
    {
        private const int HistoryPageSize = 10;
        private static readonly int[] RewardValues = { 3, 2, 1, 0 };

        private readonly IUserRepository _userRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IPreferenceRepository _preferenceRepository;

        public PlayerApplicationService(IUserRepository userRepository, IEventRepository eventRepository, IPreferenceRepository preferenceRepository)
        {
            _userRepository = userRepository;
            _eventRepository = eventRepository;
            _preferenceRepository = preferenceRepository;
        }

        public async Task<List<PreferenceTagObject>> GetPreferenceTagsAsync()
        {
            return (await _preferenceRepository.GetTagsAsync()).Select(ToTagObject).ToList();
        }

        // seen by the same players as the profile
        public async Task<PlayerPreferencesObject> GetPlayerPreferencesAsync(string viewerId, string playerId)
        {
            var canSee = await _userRepository.CustomAnyAsync(_userRepository.Where(u => u.Id == playerId
                && (u.Id == viewerId || u.UserGuilds.Any(ug => ug.Guild.UserGuilds.Any(viewerGuild => viewerGuild.UserId == viewerId)))));
            if (!canSee)
            {
                return null;
            }

            var preferences = await _preferenceRepository.GetUserPreferencesAsync(playerId);
            return new PlayerPreferencesObject
            {
                Preferred = preferences.Where(p => !p.IsAvoided).Select(p => ToTagObject(p.PreferenceTag)).ToList(),
                Avoided = preferences.Where(p => p.IsAvoided).Select(p => ToTagObject(p.PreferenceTag)).ToList(),
            };
        }

        // both lists are saved at once; the request validation keeps a tag out of both lists and the lists short
        public async Task<SetPreferencesResult> SetPreferencesAsync(string userId, List<int> preferredTagIds, List<int> avoidedTagIds)
        {
            var tagIds = (await _preferenceRepository.GetTagsAsync()).Select(t => t.Id).ToHashSet();
            if (!preferredTagIds.Concat(avoidedTagIds).All(tagIds.Contains))
            {
                return SetPreferencesResult.UnknownTag;
            }

            UserPreference ToPreference(int tagId, int order, bool isAvoided) =>
                new() { UserId = userId, PreferenceTagId = tagId, IsAvoided = isAvoided, Order = order };
            await _preferenceRepository.SetUserPreferencesAsync(userId, preferredTagIds.Select((id, index) => ToPreference(id, index, false))
                .Concat(avoidedTagIds.Select((id, index) => ToPreference(id, index, true)))
                .ToList());

            return SetPreferencesResult.Saved;
        }

        public async Task<PlayerProfileObject> GetPlayerProfileAsync(string viewerId, string playerId, string guildId)
        {
            var player = await _userRepository.FirstOrDefaultAsync(u => u.Id == playerId);
            if (player == null)
            {
                return null;
            }

            var playerGuilds = await _userRepository.CustomToListAsync(_userRepository
                .Where(u => u.Id == playerId)
                .SelectMany(u => u.UserGuilds)
                .Select(ug => new PlayerGuildRow
                {
                    Guild = new PlayerGuildObject
                    {
                        Id = ug.GuildId,
                        Name = ug.Guild.Name,
                        IconUrl = ug.Guild.AvatarUrl,
                        PlayersCount = ug.Guild.UserGuilds.Count(),
                        HasLeft = ug.IsDeleted,
                    },
                    HasPlayed = ug.Guild.Events.Any(e => e.EventRecords.Any(er => er.ParticipantId == playerId)),
                    IsShared = ug.Guild.UserGuilds.Any(viewerGuild => viewerGuild.UserId == viewerId),
                }));

            // rewards of passed games count right away, the event doesn't have to be summarized
            var scores = await _eventRepository.GetRewardScoresAsync(e => e.Guild.UserGuilds.Any(ug => ug.UserId == playerId));
            foreach (var guild in playerGuilds.Select(g => g.Guild))
            {
                guild.Score = scores.Where(s => s.GuildId == guild.Id && s.UserId == playerId).Sum(s => s.Score);
                guild.Place = scores.Count(s => s.GuildId == guild.Id && s.Score > guild.Score) + 1;
            }

            // any shared guild opens the profile, even if the player hasn't played yet
            var sharedGuilds = playerGuilds.Where(g => g.IsShared).ToList();
            if (sharedGuilds.Count == 0 || (guildId != null && !sharedGuilds.Any(g => g.Guild.Id == guildId)))
            {
                return null;
            }

            // only shared guilds where the player took part in an event are listed
            var playedGuilds = sharedGuilds
                .Where(g => g.HasPlayed)
                .Select(g => g.Guild)
                .OrderByDescending(g => g.Score)
                .ThenBy(g => g.Name)
                .ToList();
            var selectedGuilds = guildId == null
                ? playerGuilds.Select(g => g.Guild).ToList()
                : playedGuilds.Where(g => g.Id == guildId).ToList();

            // records of every guild of the player, favorite platforms don't depend on the selected guild
            var records = await _eventRepository.CustomToListAsync(_eventRepository
                .Where(e => e.EventRecords.Any(er => er.ParticipantId == playerId || er.ToUserId == playerId))
                .SelectMany(e => e.EventRecords)
                .Where(er => er.ParticipantId == playerId || er.ToUserId == playerId)
                .Select(er => new PlayerRecordRow
                {
                    GuildId = er.Event.GuildId,
                    EventStatusId = er.Event.StatusId,
                    EventStartDate = er.Event.StartDate,
                    ParticipantId = er.ParticipantId,
                    ToUserId = er.ToUserId,
                    GameId = er.GameId,
                    GamePlatforms = er.Game.Platforms,
                    Reward = er.Reward,
                    SucceededAt = er.SucceededAt,
                }));

            return new PlayerProfileObject
            {
                Id = player.Id,
                UserName = player.UserName,
                AvatarUrl = player.AvatarUrl,
                PlayingSince = player.CreatedAt,
                Guilds = playedGuilds,
                OtherGuildsCount = playerGuilds.Count(g => g.HasPlayed && !g.IsShared),
                Statistics = CalculateStatistics(playerId, selectedGuilds,
                    guildId == null ? records : records.Where(r => r.GuildId == guildId).ToList()),
                FavoritePlatforms = CalculateFavoritePlatforms(playerId, records),
            };
        }

        public async Task<PlayerHistoryObject> GetPlayerHistoryAsync(string viewerId, string playerId, string guildId, int startFrom)
        {
            // one more event is taken to know whether the next page exists
            var events = (await _eventRepository.CustomToListAsync(_eventRepository
                .Where(e => e.EventRecords.Any(er => er.ParticipantId == playerId)
                    && e.Guild.UserGuilds.Any(ug => ug.UserId == viewerId)
                    && (guildId == null || e.GuildId == guildId))
                .OrderByDescending(e => e.StartDate)
                .ThenByDescending(e => e.CreatedAt)
                .ThenBy(e => e.Id)
                .Skip(Math.Max(startFrom, 0))
                .Take(HistoryPageSize + 1)
                .Select(e => new HistoryEventRow
                {
                    EventId = e.Id,
                    EventName = e.Name,
                    GuildId = e.GuildId,
                    GuildName = e.Guild.Name,
                    GuildIconUrl = e.Guild.AvatarUrl,
                    StatusId = e.StatusId,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Records = e.EventRecords
                        .Where(er => er.ParticipantId == playerId || er.ToUserId == playerId)
                        .Select(er => new HistoryRecordRow
                        {
                            ParticipantId = er.ParticipantId,
                            ParticipantUserName = er.Participant.UserName,
                            ParticipantAvatarUrl = er.Participant.AvatarUrl,
                            ToUserId = er.ToUserId,
                            ToUserName = er.ToUser.UserName,
                            ToUserAvatarUrl = er.ToUser.AvatarUrl,
                            GameId = er.GameId,
                            GameName = er.Game.Name,
                            GameImageUrl = er.Game.ImageUrl,
                            GameSteamUrl = er.Game.SteamUrl,
                            Reward = er.Reward,
                            SucceededAt = er.SucceededAt,
                        })
                        .ToList(),
                })))
                .ToList();

            return new PlayerHistoryObject
            {
                Items = events.Take(HistoryPageSize).Select(e => ToHistoryItem(playerId, e)).ToList(),
                HasMore = events.Count > HistoryPageSize,
            };
        }

        private static PlayerStatisticsObject CalculateStatistics(string playerId, List<PlayerGuildObject> guilds, ICollection<PlayerRecordRow> records)
        {
            // every participant has one record per event, the record is about the game they guess for someone else
            var playedEvents = records.Where(r => r.ParticipantId == playerId).ToList();
            var suggestedGames = playedEvents.Where(r => r.GameId != null).ToList();
            var receivedGames = records.Where(r => r.ToUserId == playerId && r.GameId != null).ToList();
            var completedGames = receivedGames.Where(r => r.Reward != null).ToList();
            var daysToComplete = completedGames
                .Where(r => r.SucceededAt != null)
                .Select(r => Math.Max((r.SucceededAt.Value - new DateTimeOffset(r.EventStartDate, TimeSpan.Zero)).TotalDays, 0))
                .ToList();

            return new PlayerStatisticsObject
            {
                Score = guilds.Sum(g => g.Score),
                EventsPlayed = playedEvents.Count,
                EventsInProgress = playedEvents.Count(r => !r.IsEventFinished),
                GamesCompleted = completedGames.Count,
                GamesFailed = receivedGames.Count(r => r.Reward == null && r.IsEventFinished),
                AverageDaysToComplete = daysToComplete.Count > 0 ? Math.Round(daysToComplete.Average(), 1) : null,
                GamesSuggested = suggestedGames.Count,
                SuggestedGamesCompleted = suggestedGames.Count(r => r.Reward != null),
                SuggestedGamesFailed = suggestedGames.Count(r => r.Reward == null && r.IsEventFinished),
                Rewards = RewardValues
                    .Select(reward => new RewardCountObject { Reward = reward, Count = completedGames.Count(r => r.Reward == reward) })
                    .ToList(),
            };
        }

        private static PreferenceTagObject ToTagObject(PreferenceTag tag) => new() { Id = tag.Id, Name = tag.Name, NameRu = tag.NameRu };

        // platforms of games the player completed
        private static List<PlatformCountObject> CalculateFavoritePlatforms(string playerId, ICollection<PlayerRecordRow> records)
        {
            return records
                .Where(r => r.ToUserId == playerId && r.GameId != null && r.Reward != null)
                .SelectMany(r => r.GamePlatforms ?? new List<string>())
                .GroupBy(platform => platform)
                .Select(group => new PlatformCountObject { Platform = group.Key, Count = group.Count() })
                .OrderByDescending(p => p.Count)
                .ThenBy(p => p.Platform)
                .ToList();
        }

        private static PlayerHistoryItemObject ToHistoryItem(string playerId, HistoryEventRow row)
        {
            var suggested = row.Records.FirstOrDefault(r => r.ParticipantId == playerId);
            var received = row.Records.FirstOrDefault(r => r.ToUserId == playerId);

            return new PlayerHistoryItemObject
            {
                EventId = row.EventId,
                EventName = row.EventName,
                GuildId = row.GuildId,
                GuildName = row.GuildName,
                GuildIconUrl = row.GuildIconUrl,
                StatusId = row.StatusId,
                StartDate = row.StartDate,
                EndDate = row.EndDate,
                ReceivedGame = received == null ? null : ToHistoryRecord(received,
                    new PlayerShortObject { Id = received.ParticipantId, UserName = received.ParticipantUserName, AvatarUrl = received.ParticipantAvatarUrl }),
                SuggestedGame = suggested == null ? null : ToHistoryRecord(suggested, suggested.ToUserId == null ? null :
                    new PlayerShortObject { Id = suggested.ToUserId, UserName = suggested.ToUserName, AvatarUrl = suggested.ToUserAvatarUrl }),
            };
        }

        private static PlayerHistoryRecordObject ToHistoryRecord(HistoryRecordRow record, PlayerShortObject partner)
        {
            return new PlayerHistoryRecordObject
            {
                Game = record.GameId == null ? null : new GameObject
                {
                    Id = record.GameId,
                    Name = record.GameName,
                    ImageUrl = record.GameImageUrl,
                    SteamUrl = record.GameSteamUrl,
                },
                Partner = partner,
                Reward = record.Reward,
                SucceededAt = record.SucceededAt,
            };
        }

        private class PlayerGuildRow
        {
            public PlayerGuildObject Guild { get; set; }
            public bool HasPlayed { get; set; }
            public bool IsShared { get; set; }
        }

        private class PlayerRecordRow
        {
            public string GuildId { get; set; }
            public EventStatusId EventStatusId { get; set; }
            public DateTime EventStartDate { get; set; }
            public string ParticipantId { get; set; }
            public string ToUserId { get; set; }
            public string GameId { get; set; }
            public List<string> GamePlatforms { get; set; }
            public int? Reward { get; set; }
            public DateTimeOffset? SucceededAt { get; set; }
            public bool IsEventFinished => EventStatusId == EventStatusId.Finished;
        }

        private class HistoryEventRow
        {
            public string EventId { get; set; }
            public string EventName { get; set; }
            public string GuildId { get; set; }
            public string GuildName { get; set; }
            public string GuildIconUrl { get; set; }
            public EventStatusId StatusId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<HistoryRecordRow> Records { get; set; }
        }

        private class HistoryRecordRow
        {
            public string ParticipantId { get; set; }
            public string ParticipantUserName { get; set; }
            public string ParticipantAvatarUrl { get; set; }
            public string ToUserId { get; set; }
            public string ToUserName { get; set; }
            public string ToUserAvatarUrl { get; set; }
            public string GameId { get; set; }
            public string GameName { get; set; }
            public string GameImageUrl { get; set; }
            public string GameSteamUrl { get; set; }
            public int? Reward { get; set; }
            public DateTimeOffset? SucceededAt { get; set; }
        }
    }
}
