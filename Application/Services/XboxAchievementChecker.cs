using Application.Enum;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    // Xbox proves the passing by its achievements of the game in Xbox the administrator found for the event (GlobalEventGame.XboxTitleId);
    // it keeps the time of an unlock to a fraction of a second, so the pick is compared as it is
    public class XboxAchievementChecker : AchievementPlatformChecker
    {
        private readonly IUserRepository _userRepository;
        private readonly IXboxService _xboxService;

        public XboxAchievementChecker(IUserRepository userRepository, IXboxService xboxService)
        {
            _userRepository = userRepository;
            _xboxService = xboxService;
        }

        public override AchievementPlatform Platform => AchievementPlatform.Xbox;

        public override async Task<string> GetAccountIdAsync(string userId) =>
            (await _userRepository.CustomToListAsync(_userRepository.Where(u => u.Id == userId).Select(u => u.XboxXuid))).FirstOrDefault();

        // OpenXBL is asked about one game per request and its free tier gives 150 requests an hour, so a roll doesn't ask it
        public override Task<HashSet<string>> GetPassedGameIdsAsync(string xuid, List<GlobalEventGame> games, CancellationToken ct = default) =>
            Task.FromResult(new HashSet<string>());

        protected override Task<Dictionary<string, DateTimeOffset>> GetUnlockedAchievementsAsync(string xuid, GlobalEventGame game, CancellationToken ct) =>
            _xboxService.GetUnlockedAchievementsAsync(xuid, game.XboxTitleId, ct);
    }
}
