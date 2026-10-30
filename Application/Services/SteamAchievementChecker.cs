using Application.Enum;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    // Steam proves the passing by its achievements of a game, the catalog id of a game in Steam is its Steam app id
    public class SteamAchievementChecker : AchievementPlatformChecker
    {
        private readonly IUserRepository _userRepository;
        private readonly ISteamService _steamService;

        public SteamAchievementChecker(IUserRepository userRepository, ISteamService steamService)
        {
            _userRepository = userRepository;
            _steamService = steamService;
        }

        public override AchievementPlatform Platform => AchievementPlatform.Steam;

        public override async Task<string> GetAccountIdAsync(string userId) =>
            (await _userRepository.CustomToListAsync(_userRepository.Where(u => u.Id == userId).Select(u => u.SteamId))).FirstOrDefault();

        // Steam gives the owned games at once, so only an owned game with Steam achievements is asked about;
        // hidden game details tell nothing and every game drops, as without Steam
        public override async Task<HashSet<string>> GetPassedGameIdsAsync(string steamId, List<GlobalEventGame> games, CancellationToken ct = default)
        {
            var ownedAppIds = await _steamService.GetOwnedAppIdsAsync(steamId, ct);
            if (ownedAppIds == null)
            {
                return new HashSet<string>();
            }

            var checkedGames = games.Where(geg => AchievementsOf(geg).Count > 0 && ownedAppIds.Contains(geg.Game.VgcId ?? 0)).ToList();
            var unlocked = await Task.WhenAll(checkedGames.Select(geg => GetUnlockedAchievementsAsync(steamId, geg, ct)));
            if (unlocked.Any(achievements => achievements == null))
            {
                return new HashSet<string>();
            }

            return checkedGames
                .Where((geg, index) => AchievementsOf(geg).Any(a => unlocked[index].ContainsKey(a.ApiName)))
                .Select(geg => geg.Id)
                .ToHashSet();
        }

        protected override Task<Dictionary<string, DateTimeOffset>> GetUnlockedAchievementsAsync(string steamId, GlobalEventGame game, CancellationToken ct) =>
            _steamService.GetUnlockedAchievementsAsync(steamId, game.Game.VgcId ?? 0, ct);

        // Steam keeps the time of an unlock in seconds, so the pick is cut to seconds too
        protected override DateTimeOffset ToPlatformPrecision(DateTimeOffset pickedAt) =>
            DateTimeOffset.FromUnixTimeSeconds(pickedAt.ToUnixTimeSeconds());
    }
}
