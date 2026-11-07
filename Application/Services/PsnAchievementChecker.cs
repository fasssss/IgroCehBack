using Application.Enum;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    // PSN proves the passing by the trophies of the game in PSN the administrator typed for the event (with its service it is the ref of GlobalEventGameTitle)
    public class PsnAchievementChecker : AchievementPlatformChecker
    {
        private readonly IUserRepository _userRepository;
        private readonly IPsnService _psnService;

        public PsnAchievementChecker(IUserRepository userRepository, IPsnService psnService)
        {
            _userRepository = userRepository;
            _psnService = psnService;
        }

        public override AchievementPlatform Platform => AchievementPlatform.Psn;

        public override async Task<string> GetAccountIdAsync(string userId) =>
            (await _userRepository.CustomToListAsync(_userRepository.Where(u => u.Id == userId).Select(u => u.PsnAccountId))).FirstOrDefault();

        // PSN is asked about one game per request and the whole site shares one limit of requests, so a roll doesn't ask it
        public override Task<HashSet<string>> GetPassedGameIdsAsync(string accountId, List<GlobalEventGame> games, CancellationToken ct = default) =>
            Task.FromResult(new HashSet<string>());

        protected override Task<Dictionary<string, DateTimeOffset>> GetUnlockedAchievementsAsync(string accountId, GlobalEventGame game, CancellationToken ct) =>
            _psnService.GetEarnedTrophiesAsync(accountId, TitleRefOf(game), ct);

        // PSN keeps the time of an earned trophy in seconds, so the pick is cut to seconds too
        protected override DateTimeOffset ToPlatformPrecision(DateTimeOffset pickedAt) =>
            DateTimeOffset.FromUnixTimeSeconds(pickedAt.ToUnixTimeSeconds());
    }
}
