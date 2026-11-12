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
        private const int GamesPageSize = 6;
        private static readonly int[] RewardValues = { 3, 2, 1, 0 };

        private readonly IUserRepository _userRepository;
        private readonly IGlobalEventParticipantRepository _participantRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IPreferenceRepository _preferenceRepository;
        private readonly IGameRepository _gameRepository;

        public PlayerApplicationService(IUserRepository userRepository, IEventRepository eventRepository, IPreferenceRepository preferenceRepository,
            IGlobalEventParticipantRepository participantRepository, IGameRepository gameRepository)
        {
            _userRepository = userRepository;
            _participantRepository = participantRepository;
            _eventRepository = eventRepository;
            _preferenceRepository = preferenceRepository;
            _gameRepository = gameRepository;
        }

        public async Task<List<PreferenceTagObject>> GetPreferenceTagsAsync()
        {
            return (await _preferenceRepository.GetTagsAsync()).Select(ToTagObject).ToList();
        }

        // seen by the same players as the profile
        public async Task<PlayerPreferencesObject> GetPlayerPreferencesAsync(string viewerId, string playerId)
        {
            if (!await CanSeePlayerAsync(viewerId, playerId))
            {
                return null;
            }

            var preferences = await _preferenceRepository.GetUserPreferencesAsync(playerId);
            var aspects = await _preferenceRepository.GetUserAspectsAsync(playerId);
            return new PlayerPreferencesObject
            {
                Preferred = preferences.Where(p => !p.IsAvoided).Select(p => ToTagObject(p.PreferenceTag)).ToList(),
                Avoided = preferences.Where(p => p.IsAvoided).Select(p => ToTagObject(p.PreferenceTag)).ToList(),
                LikedAspects = aspects.Select(a => ToAspectObject(a.GameAspect)).ToList(),
            };
        }

        // seen by the same players as the profile
        public async Task<List<FavoriteGameObject>> GetFavoriteGamesAsync(string viewerId, string playerId)
        {
            if (!await CanSeePlayerAsync(viewerId, playerId))
            {
                return null;
            }

            return (await _gameRepository.GetUserFavoritesAsync(playerId))
                .Select(f => new FavoriteGameObject
                {
                    Game = new GameObject
                    {
                        Id = f.Game.Id,
                        Name = f.Game.Name,
                        ImageUrl = f.Game.ImageUrl,
                        SteamUrl = f.Game.SteamUrl,
                    },
                    ReleaseDate = f.Game.ReleaseDate,
                })
                .ToList();
        }

        // the list is saved whole; the request validation keeps it short and without repeats
        public async Task<SetFavoriteGamesResult> SetFavoriteGamesAsync(string userId, List<string> gameIds)
        {
            var savedIds = await _gameRepository.CustomToListAsync(_gameRepository.Where(g => gameIds.Contains(g.Id)).Select(g => g.Id));
            if (savedIds.Count != gameIds.Count)
            {
                return SetFavoriteGamesResult.UnknownGame;
            }

            await _gameRepository.SetUserFavoritesAsync(userId, gameIds
                .Select((id, index) => new UserFavoriteGame { UserId = userId, GameId = id, Order = index })
                .ToList());

            return SetFavoriteGamesResult.Saved;
        }

        public async Task<List<GameAspectObject>> GetGameAspectsAsync()
        {
            return (await _preferenceRepository.GetAspectsAsync()).Select(ToAspectObject).ToList();
        }

        // the list is saved whole; the request validation keeps it short and without repeats
        public async Task<SetPreferencesResult> SetLikedAspectsAsync(string userId, List<int> aspectIds)
        {
            var catalogIds = (await _preferenceRepository.GetAspectsAsync()).Select(a => a.Id).ToHashSet();
            if (!aspectIds.All(catalogIds.Contains))
            {
                return SetPreferencesResult.UnknownTag;
            }

            await _preferenceRepository.SetUserAspectsAsync(userId, aspectIds
                .Select((id, index) => new UserGameAspect { UserId = userId, GameAspectId = id, Order = index })
                .ToList());

            return SetPreferencesResult.Saved;
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

        // the Igrofest games of the player, seen by the same players as the profile;
        // the global events are common for the site, so the guilds don't narrow them
        public async Task<PlayerLadderObject> GetPlayerLadderAsync(string viewerId, string playerId)
        {
            if (!await CanSeePlayerAsync(viewerId, playerId))
            {
                return null;
            }

            // a passed game of any event or the game picked in the active one (an active event isn't finished, as in GlobalEventApplicationService)
            var games = await _participantRepository.CustomToListAsync(_participantRepository
                .Where(p => p.UserId == playerId && p.PickedGameId != null && (p.PassedAt != null || p.GlobalEvent.FinishedAt == null))
                .Select(p => new PlayerFestGameObject
                {
                    Game = new GameObject
                    {
                        Id = p.PickedGame.GameId,
                        Name = p.PickedGame.Game.Name,
                        ImageUrl = p.PickedGame.Game.ImageUrl,
                        SteamUrl = p.PickedGame.Game.SteamUrl,
                    },
                    GlobalEventId = p.GlobalEventId,
                    GlobalEventName = p.GlobalEvent.Name,
                    PickedAt = p.PickedAt,
                    PassedAt = p.PassedAt,
                    PassedPlatform = (AchievementPlatform?)p.PassedPlatform,
                }));

            return new PlayerLadderObject
            {
                Fest = new PlayerFestObject
                {
                    Current = games.Where(g => g.PassedAt == null).OrderByDescending(g => g.PickedAt).FirstOrDefault(),
                    Passed = games.Where(g => g.PassedAt != null).OrderByDescending(g => g.PassedAt).ToList(),
                },
            };
        }

        // the passed or the guessed games of the player from the guilds shared with the viewer, a page at a time;
        // the viewer is a member of the guild of every record here, so a review open to the guild is open to them
        public async Task<PlayerGamesObject> GetPlayerGamesAsync(string viewerId, string playerId, PlayerGamesKind kind, PlayerGamesSort sort, int startFrom)
        {
            if (!await CanSeePlayerAsync(viewerId, playerId))
            {
                return null;
            }

            var sharedRecords = _eventRepository
                .Where(e => e.Guild.UserGuilds.Any(ug => ug.UserId == viewerId))
                .SelectMany(e => e.EventRecords)
                .Where(er => er.GameId != null);

            var games = kind == PlayerGamesKind.Passed
                ? sharedRecords
                    .Where(er => er.ToUserId == playerId && er.SucceededAt != null)
                    .Select(er => new
                    {
                        Record = er,
                        IsReviewVisible = er.Review != null && (er.ToUserId == viewerId
                            || er.Review.Visibility >= ReviewVisibility.Guild
                            || (er.Review.Visibility == ReviewVisibility.AuthorAndGuesser && er.ParticipantId == viewerId)),
                    })
                    .Select(row => new PlayerGameObject
                    {
                        EventRecordId = row.Record.Id,
                        Game = new GameObject { Id = row.Record.GameId, Name = row.Record.Game.Name, ImageUrl = row.Record.Game.ImageUrl, SteamUrl = row.Record.Game.SteamUrl },
                        Partner = new PlayerShortObject { Id = row.Record.ParticipantId, UserName = row.Record.Participant.UserName, AvatarUrl = row.Record.Participant.AvatarUrl },
                        EventId = row.Record.EventId,
                        EventName = row.Record.Event.Name,
                        Date = row.Record.SucceededAt,
                        ReviewId = row.IsReviewVisible ? row.Record.Review.Id : null,
                        Score = row.IsReviewVisible ? row.Record.Review.Score : null,
                    })
                : sharedRecords
                    // the game of the pool that dropped to a newcomer isn't a guess of the player
                    .Where(er => er.ParticipantId == playerId && er.NewcomerPoolGameId == null)
                    .Select(er => new PlayerGameObject
                    {
                        EventRecordId = er.Id,
                        Game = new GameObject { Id = er.GameId, Name = er.Game.Name, ImageUrl = er.Game.ImageUrl, SteamUrl = er.Game.SteamUrl },
                        Partner = er.ToUserId == null ? null : new PlayerShortObject { Id = er.ToUserId, UserName = er.ToUser.UserName, AvatarUrl = er.ToUser.AvatarUrl },
                        EventId = er.EventId,
                        EventName = er.Event.Name,
                        Date = er.CreatedAt,
                    });

            // MySQL puts the games without a score after the others in a descending order
            var ordered = kind == PlayerGamesKind.Passed && sort == PlayerGamesSort.Score
                ? games.OrderByDescending(g => g.Score).ThenByDescending(g => g.Date).ThenBy(g => g.EventRecordId)
                : games.OrderByDescending(g => g.Date).ThenBy(g => g.EventRecordId);

            // one more game is taken to know whether the next page exists
            var page = await _eventRepository.CustomToListAsync(ordered.Skip(Math.Max(startFrom, 0)).Take(GamesPageSize + 1));

            return new PlayerGamesObject
            {
                Games = page.Take(GamesPageSize).ToList(),
                HasMore = page.Count > GamesPageSize,
            };
        }

        public async Task<PlayerProfileObject> GetPlayerProfileAsync(string viewerId, string playerId)
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
                    // a newcomer is only the one who got a game in the event
                    HasPlayed = ug.Guild.Events.Any(e => e.EventRecords.Any(er => er.ParticipantId == playerId || er.ToUserId == playerId)),
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
            if (sharedGuilds.Count == 0)
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

            // records of every guild of the player
            var records = await _eventRepository.CustomToListAsync(_eventRepository
                .Where(e => e.EventRecords.Any(er => er.ParticipantId == playerId || er.ToUserId == playerId))
                .SelectMany(e => e.EventRecords)
                .Where(er => er.ParticipantId == playerId || er.ToUserId == playerId)
                .Select(er => new PlayerRecordRow
                {
                    EventId = er.EventId,
                    GuildId = er.Event.GuildId,
                    EventStatusId = er.Event.StatusId,
                    EventStartDate = er.Event.StartDate,
                    ParticipantId = er.ParticipantId,
                    ToUserId = er.ToUserId,
                    GameId = er.GameId,
                    GamePlatforms = er.Game.Platforms,
                    Reward = er.Reward,
                    SucceededAt = er.SucceededAt,
                    IsNewcomer = er.NewcomerPoolGameId != null,
                }));

            // the same badge of two events is shown twice, each with its event
            var badges = await _participantRepository.CustomToListAsync(_participantRepository
                .Where(p => p.UserId == playerId && p.PassedAt != null && p.GlobalEvent.BadgeId != null)
                .OrderByDescending(p => p.PassedAt)
                .Select(p => new PlayerBadgeObject
                {
                    BadgeId = p.GlobalEvent.BadgeId,
                    GlobalEventId = p.GlobalEventId,
                    GlobalEventName = p.GlobalEvent.Name,
                    PassedAt = p.PassedAt,
                }));

            return new PlayerProfileObject
            {
                Id = player.Id,
                UserName = player.UserName,
                AvatarUrl = player.AvatarUrl,
                PlayingSince = player.CreatedAt,
                SteamId = player.SteamId,
                XboxGamertag = player.XboxGamertag,
                PsnOnlineId = player.PsnOnlineId,
                Guilds = playedGuilds,
                OtherGuildsCount = playerGuilds.Count(g => g.HasPlayed && !g.IsShared),
                Statistics = CalculateStatistics(playerId, playerGuilds.Select(g => g.Guild).ToList(), records),
                FavoritePlatforms = CalculateFavoritePlatforms(playerId, records),
                Badges = badges.ToList(),
            };
        }

        public async Task<PlayerHistoryObject> GetPlayerHistoryAsync(string viewerId, string playerId, int startFrom)
        {
            // one more event is taken to know whether the next page exists
            var events = (await _eventRepository.CustomToListAsync(_eventRepository
                .Where(e => e.EventRecords.Any(er => er.ParticipantId == playerId || er.ToUserId == playerId)
                    && e.Guild.UserGuilds.Any(ug => ug.UserId == viewerId))
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
                            IsNewcomer = er.NewcomerPoolGameId != null,
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
            // a participant has the record of the game they guess for someone else, a newcomer has the record of the game they rolled;
            // the author of a game of the pool has one more record in the event, so the events are counted once
            var playedEvents = records
                .Where(r => r.IsNewcomer ? r.ToUserId == playerId : r.ParticipantId == playerId)
                .DistinctBy(r => r.EventId)
                .ToList();
            // a game of the pool that dropped to a newcomer is suggested by its author too
            var suggestedGames = records.Where(r => r.ParticipantId == playerId && r.GameId != null).ToList();
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

        private static GameAspectObject ToAspectObject(GameAspect aspect) => new() { Id = aspect.Id, Name = aspect.Name, NameRu = aspect.NameRu };

        // the own profile and the players of a shared guild
        private Task<bool> CanSeePlayerAsync(string viewerId, string playerId)
        {
            return _userRepository.CustomAnyAsync(_userRepository.Where(u => u.Id == playerId
                && (u.Id == viewerId || u.UserGuilds.Any(ug => ug.Guild.UserGuilds.Any(viewerGuild => viewerGuild.UserId == viewerId)))));
        }

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
            // the game the player guessed in the chain, not the one of the pool that dropped to a newcomer
            var suggested = row.Records.FirstOrDefault(r => r.ParticipantId == playerId && !r.IsNewcomer);
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
                IsNewcomer = received?.IsNewcomer ?? false,
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
    }
}
