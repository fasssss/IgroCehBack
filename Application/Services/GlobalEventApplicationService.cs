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

            // Steam is asked about every game before anything is saved
            var chosenAchievements = new List<List<SteamAchievementObject>>();
            foreach (var game in games)
            {
                var steamAchievements = await _steamService.GetAchievementsAsync(game.AppId, ct);
                var chosen = steamAchievements.Where(a => game.AchievementApiNames.Contains(a.ApiName)).ToList();
                if (chosen.Count != game.AchievementApiNames.Count)
                {
                    return (CreateGlobalEventResult.UnknownAchievement, null);
                }

                chosenAchievements.Add(chosen);
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
                Name = name.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim().Replace("\r\n", "\n"),
                CreatorId = userId,
                Games = games.Select((game, index) => new GlobalEventGame
                {
                    Id = Guid.NewGuid().ToString(),
                    GameId = gameIds[index],
                    Order = index,
                    CreatedAt = DateTimeOffset.UtcNow,
                    // in the order of Steam, not of the clicks
                    Achievements = chosenAchievements[index].Select((achievement, order) => new GlobalEventAchievement
                    {
                        Id = Guid.NewGuid().ToString(),
                        ApiName = achievement.ApiName,
                        Name = achievement.Name,
                        NameRu = achievement.NameRu,
                        Description = achievement.Description,
                        DescriptionRu = achievement.DescriptionRu,
                        IconUrl = achievement.IconUrl,
                        Order = order,
                        CreatedAt = DateTimeOffset.UtcNow,
                    }).ToList(),
                }).ToList(),
            });
            await _globalEventRepository.SaveAsync();

            return (CreateGlobalEventResult.Created, await GetAsync(globalEvent.Id));
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
