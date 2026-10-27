using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Services
{
    public class GlobalEventApplicationService : IGlobalEventApplicationService
    {
        private readonly IGlobalEventRepository _globalEventRepository;
        private readonly IGameApplicationService _gameApplicationService;
        private readonly ISteamService _steamService;

        public GlobalEventApplicationService(IGlobalEventRepository globalEventRepository, IGameApplicationService gameApplicationService, ISteamService steamService)
        {
            _globalEventRepository = globalEventRepository;
            _gameApplicationService = gameApplicationService;
            _steamService = steamService;
        }

        // the achievements the administrator picks from: the catalog id of a game (Game.VgcId) is its Steam app id
        public async Task<List<SteamAchievementObject>> GetGameAchievementsAsync(int appId, CancellationToken ct = default)
        {
            return await _steamService.GetAchievementsAsync(appId, ct);
        }

        // the achievements are checked against Steam and kept with their names, the catalog games are saved on the way;
        // the request validation keeps the games distinct and the lists within the limits
        public async Task<(CreateGlobalEventResult Result, GlobalEventObject GlobalEvent)> CreateAsync(string userId, string name, string description,
            List<NewGlobalEventGameObject> games, CancellationToken ct = default)
        {
            if (await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(IsActive)))
            {
                return (CreateGlobalEventResult.AlreadyActive, null);
            }

            var chosenAchievements = await GetChosenAchievementsAsync(games, ct);
            if (chosenAchievements == null)
            {
                return (CreateGlobalEventResult.UnknownAchievement, null);
            }

            var gameIds = new List<string>();
            foreach (var game in games)
            {
                var gameId = await _gameApplicationService.CreateGameFromCatalogAsync(userId, game.AppId, ct);
                if (gameId == null)
                {
                    return (CreateGlobalEventResult.UnknownGame, null);
                }

                gameIds.Add(gameId);
            }

            var globalEvent = await _globalEventRepository.AddAsync(new GlobalEvent
            {
                Id = Guid.NewGuid().ToString(),
                Name = NormalizeName(name),
                Description = NormalizeDescription(description),
                CreatorId = userId,
                Games = games.Select((game, index) =>
                {
                    var globalEventGame = new GlobalEventGame
                    {
                        Id = Guid.NewGuid().ToString(),
                        GameId = gameIds[index],
                        Order = index,
                        CreatedAt = DateTimeOffset.UtcNow,
                    };
                    SetAchievements(globalEventGame, chosenAchievements[game.AppId]);
                    return globalEventGame;
                }).ToList(),
            });
            await _globalEventRepository.SaveAsync();

            return (CreateGlobalEventResult.Created, await GetAsync(globalEvent.Id));
        }

        // the games come in full, in their new order; Steam is asked only about the games whose achievements are new or changed,
        // so the name can be changed while Steam is down; once a player has rolled, the games and their achievements stay as they are
        public async Task<(UpdateGlobalEventResult Result, GlobalEventObject GlobalEvent)> UpdateAsync(string userId, string globalEventId, string name,
            string description, List<NewGlobalEventGameObject> games, CancellationToken ct = default)
        {
            var globalEvent = await _globalEventRepository.GetWithGamesAsync(globalEventId);
            if (globalEvent == null)
            {
                return (UpdateGlobalEventResult.NotFound, null);
            }

            // the games of a global event always come from the catalog
            var keptGames = globalEvent.Games.ToDictionary(geg => geg.Game.VgcId.Value);
            var changedGames = games
                .Where(game => !keptGames.TryGetValue(game.AppId, out var geg)
                    || !geg.Achievements.Select(a => a.ApiName).ToHashSet().SetEquals(game.AchievementApiNames))
                .ToList();
            // the games are distinct, so without new or changed ones a list of the same length is the same games, maybe in another order
            var isGamesChanged = changedGames.Count > 0 || games.Count != keptGames.Count;
            if (isGamesChanged && await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(ge => ge.Id == globalEventId && ge.Participants.Any())))
            {
                return (UpdateGlobalEventResult.GamesLocked, null);
            }

            var chosenAchievements = await GetChosenAchievementsAsync(changedGames, ct);
            if (chosenAchievements == null)
            {
                return (UpdateGlobalEventResult.UnknownAchievement, null);
            }

            var newGameIds = new Dictionary<int, string>();
            foreach (var game in games.Where(game => !keptGames.ContainsKey(game.AppId)))
            {
                var gameId = await _gameApplicationService.CreateGameFromCatalogAsync(userId, game.AppId, ct);
                if (gameId == null)
                {
                    return (UpdateGlobalEventResult.UnknownGame, null);
                }

                newGameIds[game.AppId] = gameId;
            }

            globalEvent.Name = NormalizeName(name);
            globalEvent.Description = NormalizeDescription(description);
            var appIds = games.Select(game => game.AppId).ToHashSet();
            globalEvent.Games.RemoveAll(geg => !appIds.Contains(geg.Game.VgcId.Value));
            foreach (var (game, index) in games.Select((game, index) => (game, index)))
            {
                if (!keptGames.TryGetValue(game.AppId, out var globalEventGame))
                {
                    globalEventGame = new GlobalEventGame
                    {
                        Id = Guid.NewGuid().ToString(),
                        GameId = newGameIds[game.AppId],
                        CreatedAt = DateTimeOffset.UtcNow,
                    };
                    globalEvent.Games.Add(globalEventGame);
                }

                globalEventGame.Order = index;
                if (chosenAchievements.TryGetValue(game.AppId, out var achievements))
                {
                    SetAchievements(globalEventGame, achievements);
                }
            }

            await _globalEventRepository.SaveAsync();

            return (UpdateGlobalEventResult.Updated, await GetAsync(globalEvent.Id));
        }

        // the image is uploaded apart from the event and can be replaced at any time
        public async Task<ChangeGlobalEventResult> SetImageAsync(string globalEventId, string imageUrl)
        {
            var globalEvent = await _globalEventRepository.FirstOrDefaultAsync(ge => ge.Id == globalEventId);
            if (globalEvent == null)
            {
                return ChangeGlobalEventResult.NotFound;
            }

            globalEvent.ImageUrl = imageUrl;
            await _globalEventRepository.SaveAsync();

            return ChangeGlobalEventResult.Changed;
        }

        public async Task<GlobalEventObject> GetActiveAsync()
        {
            return (await _globalEventRepository.CustomToListAsync(ToObjects(_globalEventRepository.Where(IsActive)))).FirstOrDefault();
        }

        public async Task<GlobalEventObject> GetAsync(string globalEventId)
        {
            return (await _globalEventRepository.CustomToListAsync(ToObjects(_globalEventRepository.Where(ge => ge.Id == globalEventId)))).FirstOrDefault();
        }

        public async Task<ChangeGlobalEventResult> FinishAsync(string userId, string globalEventId)
        {
            var globalEvent = await _globalEventRepository.FirstOrDefaultAsync(ge => ge.Id == globalEventId);
            if (globalEvent == null)
            {
                return ChangeGlobalEventResult.NotFound;
            }

            if (globalEvent.FinishedAt != null)
            {
                return ChangeGlobalEventResult.Unavailable;
            }

            globalEvent.FinishedAt = DateTimeOffset.UtcNow;
            globalEvent.FinishedById = userId;
            await _globalEventRepository.SaveAsync();

            return ChangeGlobalEventResult.Changed;
        }

        // soft delete: the event with its games is only hidden, so it can be restored by unsetting IsDeleted
        public async Task<ChangeGlobalEventResult> DeleteAsync(string userId, string globalEventId)
        {
            var globalEvent = await _globalEventRepository.FirstOrDefaultAsync(ge => ge.Id == globalEventId);
            if (globalEvent == null)
            {
                return ChangeGlobalEventResult.NotFound;
            }

            globalEvent.IsDeleted = true;
            globalEvent.DeletedAt = DateTimeOffset.UtcNow;
            globalEvent.DeletedById = userId;
            await _globalEventRepository.SaveAsync();

            return ChangeGlobalEventResult.Changed;
        }

        private static readonly Expression<Func<GlobalEvent, bool>> IsActive = ge => ge.FinishedAt == null;

        private static string NormalizeName(string name) => name.Trim();

        private static string NormalizeDescription(string description) =>
            string.IsNullOrWhiteSpace(description) ? null : description.Trim().Replace("\r\n", "\n");

        // Steam is asked about every game with achievements before anything is saved: the chosen achievements of each game in the order of Steam,
        // null when a game has no such achievement; a game without achievements is passed only by a claim checked by hand
        private async Task<Dictionary<int, List<SteamAchievementObject>>> GetChosenAchievementsAsync(List<NewGlobalEventGameObject> games, CancellationToken ct)
        {
            var chosenAchievements = new Dictionary<int, List<SteamAchievementObject>>();
            foreach (var game in games)
            {
                if (game.AchievementApiNames.Count == 0)
                {
                    chosenAchievements[game.AppId] = new List<SteamAchievementObject>();
                    continue;
                }

                var steamAchievements = await _steamService.GetAchievementsAsync(game.AppId, ct);
                var chosen = steamAchievements.Where(a => game.AchievementApiNames.Contains(a.ApiName)).ToList();
                if (chosen.Count != game.AchievementApiNames.Count)
                {
                    return null;
                }

                chosenAchievements[game.AppId] = chosen;
            }

            return chosenAchievements;
        }

        // the achievements that stay keep their rows, so an API name of a game is never deleted and added again in one save
        private static void SetAchievements(GlobalEventGame globalEventGame, List<SteamAchievementObject> chosen)
        {
            var apiNames = chosen.Select(a => a.ApiName).ToHashSet();
            globalEventGame.Achievements.RemoveAll(a => !apiNames.Contains(a.ApiName));
            foreach (var (achievement, order) in chosen.Select((achievement, order) => (achievement, order)))
            {
                var kept = globalEventGame.Achievements.FirstOrDefault(a => a.ApiName == achievement.ApiName);
                if (kept == null)
                {
                    kept = new GlobalEventAchievement
                    {
                        Id = Guid.NewGuid().ToString(),
                        ApiName = achievement.ApiName,
                        CreatedAt = DateTimeOffset.UtcNow,
                    };
                    globalEventGame.Achievements.Add(kept);
                }

                // the names come fresh from Steam, the order is the order of Steam, not of the clicks
                kept.Name = achievement.Name;
                kept.NameRu = achievement.NameRu;
                kept.Description = achievement.Description;
                kept.DescriptionRu = achievement.DescriptionRu;
                kept.IconUrl = achievement.IconUrl;
                kept.Order = order;
            }
        }

        private static IQueryable<GlobalEventObject> ToObjects(IQueryable<GlobalEvent> globalEvents) => globalEvents
            .OrderByDescending(ge => ge.CreatedAt)
            .Select(ge => new GlobalEventObject
            {
                Id = ge.Id,
                Name = ge.Name,
                Description = ge.Description,
                ImageUrl = ge.ImageUrl,
                CreatedAt = ge.CreatedAt,
                FinishedAt = ge.FinishedAt,
                ParticipantCount = ge.Participants.Count(),
                PassedCount = ge.Participants.Count(p => p.PassedAt != null),
                Games = ge.Games
                    .OrderBy(geg => geg.Order)
                    .Select(geg => new GlobalEventGameObject
                    {
                        Game = new GameObject { Id = geg.Game.Id, Name = geg.Game.Name, ImageUrl = geg.Game.ImageUrl, SteamUrl = geg.Game.SteamUrl },
                        AppId = geg.Game.VgcId ?? 0,
                        Achievements = geg.Achievements
                            .OrderBy(a => a.Order)
                            .Select(a => new GlobalEventAchievementObject
                            {
                                ApiName = a.ApiName,
                                Name = a.Name,
                                NameRu = a.NameRu,
                                Description = a.Description,
                                DescriptionRu = a.DescriptionRu,
                                IconUrl = a.IconUrl,
                            })
                            .ToList(),
                    })
                    .ToList(),
            });
    }
}
