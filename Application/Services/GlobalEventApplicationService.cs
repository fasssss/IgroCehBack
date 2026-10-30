using Application.ApplicationInterfaces;
using Application.Consts;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System.Linq.Expressions;

namespace Application.Services
{
    public class GlobalEventApplicationService : IGlobalEventApplicationService
    {
        private readonly IGlobalEventRepository _globalEventRepository;
        private readonly IBadgeRepository _badgeRepository;
        private readonly IGameApplicationService _gameApplicationService;
        private readonly ISteamService _steamService;
        private readonly IXboxService _xboxService;

        public GlobalEventApplicationService(IGlobalEventRepository globalEventRepository, IBadgeRepository badgeRepository,
            IGameApplicationService gameApplicationService, ISteamService steamService, IXboxService xboxService)
        {
            _globalEventRepository = globalEventRepository;
            _badgeRepository = badgeRepository;
            _gameApplicationService = gameApplicationService;
            _steamService = steamService;
            _xboxService = xboxService;
        }

        // the achievements the administrator picks from: the catalog id of a game in Steam (Game.VgcId) is its Steam app id,
        // a game outside Steam has none there, so Steam isn't asked
        public async Task<List<SteamAchievementObject>> GetGameAchievementsAsync(int appId, CancellationToken ct = default)
        {
            return CatalogIds.IsSteamAppId(appId) ? await _steamService.GetAchievementsAsync(appId, ct) : new List<SteamAchievementObject>();
        }

        // the catalog knows no Xbox title ids, so the game is looked for in the Microsoft Store by its name
        public async Task<List<XboxTitleObject>> SearchXboxTitlesAsync(string query, CancellationToken ct = default)
        {
            return await _xboxService.SearchTitlesAsync(query, ct);
        }

        // the search gives no title ids for the games found by their names, only the chosen one is looked up
        public async Task<string> GetXboxTitleIdAsync(string productId, CancellationToken ct = default)
        {
            return await _xboxService.GetProductTitleIdAsync(productId, ct);
        }

        public async Task<XboxTitleAchievementsObject> GetXboxAchievementsAsync(string titleId, CancellationToken ct = default)
        {
            return await _xboxService.GetTitleAchievementsAsync(titleId, ct);
        }

        // the achievements are checked against Steam and Xbox and kept with their names, the catalog games are saved on the way;
        // the request validation keeps the games distinct and the lists within the limits
        public async Task<(CreateGlobalEventResult Result, GlobalEventObject GlobalEvent)> CreateAsync(string userId, string name, string description,
            string badgeId, List<NewGlobalEventGameObject> games, CancellationToken ct = default)
        {
            if (await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(IsActive)))
            {
                return (CreateGlobalEventResult.AlreadyActive, null);
            }

            if (!await IsBadgeAsync(badgeId))
            {
                return (CreateGlobalEventResult.UnknownBadge, null);
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
                BadgeId = badgeId,
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

        // the games come in full, in their new order; Steam and Xbox are asked only about the games whose achievements are new or changed,
        // so the name can be changed while they are down; once a player has rolled, the games and their achievements stay as they are,
        // once a player has passed, the badge stays too
        public async Task<(UpdateGlobalEventResult Result, GlobalEventObject GlobalEvent)> UpdateAsync(string userId, string globalEventId, string name,
            string description, string badgeId, List<NewGlobalEventGameObject> games, CancellationToken ct = default)
        {
            var globalEvent = await _globalEventRepository.GetWithGamesAsync(globalEventId);
            if (globalEvent == null)
            {
                return (UpdateGlobalEventResult.NotFound, null);
            }

            if (!await IsBadgeAsync(badgeId))
            {
                return (UpdateGlobalEventResult.UnknownBadge, null);
            }

            // the games of a global event always come from the catalog
            var keptGames = globalEvent.Games.ToDictionary(geg => geg.Game.VgcId.Value);
            var changedGames = games
                .Where(game => !keptGames.TryGetValue(game.AppId, out var geg) || !IsSameAchievements(geg, game))
                .ToList();
            // the games are distinct, so without new or changed ones a list of the same length is the same games, maybe in another order
            var isGamesChanged = changedGames.Count > 0 || games.Count != keptGames.Count;
            if (isGamesChanged && await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(ge => ge.Id == globalEventId && ge.Participants.Any())))
            {
                return (UpdateGlobalEventResult.GamesLocked, null);
            }

            if (badgeId != globalEvent.BadgeId
                && await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(ge => ge.Id == globalEventId && ge.Participants.Any(p => p.PassedAt != null))))
            {
                return (UpdateGlobalEventResult.BadgeLocked, null);
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
            globalEvent.BadgeId = badgeId;
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

        private async Task<bool> IsBadgeAsync(string badgeId) => await _badgeRepository.CustomAnyAsync(_badgeRepository.Where(b => b.Id == badgeId));

        private static string NormalizeDescription(string description) =>
            string.IsNullOrWhiteSpace(description) ? null : description.Trim().Replace("\r\n", "\n");

        // the Xbox achievements count only together with their game in Xbox
        private static bool IsSameAchievements(GlobalEventGame globalEventGame, NewGlobalEventGameObject game)
        {
            var xboxIds = game.XboxAchievementIds ?? new List<string>();
            return ApiNames(globalEventGame, GlobalEventAchievementPlatform.Steam).SetEquals(game.AchievementApiNames)
                && ApiNames(globalEventGame, GlobalEventAchievementPlatform.Xbox).SetEquals(xboxIds)
                && (xboxIds.Count == 0 || globalEventGame.XboxTitleId == game.XboxTitleId);
        }

        private static HashSet<string> ApiNames(GlobalEventGame globalEventGame, GlobalEventAchievementPlatform platform) =>
            globalEventGame.Achievements.Where(a => a.Platform == platform).Select(a => a.ApiName).ToHashSet();

        // the chosen achievements of a game in Steam and in Xbox, the Xbox ones with their game in Xbox
        private class ChosenAchievements
        {
            public List<SteamAchievementObject> Steam { get; set; } = new List<SteamAchievementObject>();
            // null without Xbox achievements, its list holds only the chosen ones
            public XboxTitleAchievementsObject XboxTitle { get; set; }
        }

        // Steam and Xbox are asked about every game with achievements before anything is saved: the chosen achievements of each game
        // in the order of the platform, null when a game has no such achievement or a game outside Steam is given Steam ones;
        // a game without achievements is passed only by a claim
        private async Task<Dictionary<int, ChosenAchievements>> GetChosenAchievementsAsync(List<NewGlobalEventGameObject> games, CancellationToken ct)
        {
            var chosenAchievements = new Dictionary<int, ChosenAchievements>();
            foreach (var game in games)
            {
                var chosen = new ChosenAchievements();
                if (game.AchievementApiNames.Count > 0)
                {
                    if (!CatalogIds.IsSteamAppId(game.AppId))
                    {
                        return null;
                    }

                    var steamAchievements = await _steamService.GetAchievementsAsync(game.AppId, ct);
                    chosen.Steam = steamAchievements.Where(a => game.AchievementApiNames.Contains(a.ApiName)).ToList();
                    if (chosen.Steam.Count != game.AchievementApiNames.Count)
                    {
                        return null;
                    }
                }

                var xboxIds = game.XboxAchievementIds ?? new List<string>();
                if (xboxIds.Count > 0)
                {
                    var xboxTitle = await _xboxService.GetTitleAchievementsAsync(game.XboxTitleId, ct);
                    var chosenXbox = xboxTitle?.Achievements.Where(a => xboxIds.Contains(a.ApiName)).ToList();
                    if (chosenXbox == null || chosenXbox.Count != xboxIds.Count)
                    {
                        return null;
                    }

                    chosen.XboxTitle = new XboxTitleAchievementsObject { TitleId = xboxTitle.TitleId, Name = xboxTitle.Name, Achievements = chosenXbox };
                }

                chosenAchievements[game.AppId] = chosen;
            }

            return chosenAchievements;
        }

        // the achievements that stay keep their rows, so an API name of a game is never deleted and added again in one save
        private static void SetAchievements(GlobalEventGame globalEventGame, ChosenAchievements chosen)
        {
            var all = chosen.Steam
                .Select(a => (Platform: GlobalEventAchievementPlatform.Steam, a.ApiName, a.Name, a.NameRu, a.Description, a.DescriptionRu, a.IconUrl))
                .Concat((chosen.XboxTitle?.Achievements ?? new List<XboxAchievementObject>())
                    .Select(a => (Platform: GlobalEventAchievementPlatform.Xbox, a.ApiName, a.Name, a.NameRu, a.Description, a.DescriptionRu, a.IconUrl)))
                .ToList();
            globalEventGame.XboxTitleId = chosen.XboxTitle?.TitleId;
            globalEventGame.XboxTitleName = chosen.XboxTitle?.Name;
            var keys = all.Select(a => (a.Platform, a.ApiName)).ToHashSet();
            globalEventGame.Achievements.RemoveAll(a => !keys.Contains((a.Platform, a.ApiName)));
            foreach (var (achievement, order) in all.Select((achievement, order) => (achievement, order)))
            {
                var kept = globalEventGame.Achievements.FirstOrDefault(a => a.Platform == achievement.Platform && a.ApiName == achievement.ApiName);
                if (kept == null)
                {
                    kept = new GlobalEventAchievement
                    {
                        Id = Guid.NewGuid().ToString(),
                        Platform = achievement.Platform,
                        ApiName = achievement.ApiName,
                        CreatedAt = DateTimeOffset.UtcNow,
                    };
                    globalEventGame.Achievements.Add(kept);
                }

                // the names come fresh from the platform, the order is the order of the platform, not of the clicks
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
                BadgeId = ge.BadgeId,
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
                        XboxTitleId = geg.XboxTitleId,
                        XboxTitleName = geg.XboxTitleName,
                        Achievements = geg.Achievements
                            .OrderBy(a => a.Order)
                            .Select(a => new GlobalEventAchievementObject
                            {
                                Platform = (AchievementPlatform)a.Platform,
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
