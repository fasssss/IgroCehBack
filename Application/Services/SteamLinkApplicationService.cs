using Application.ApplicationInterfaces;
using Application.Enum;
using Application.Exceptions;
using Application.Interfaces;

namespace Application.Services
{
    public class SteamLinkApplicationService : ISteamLinkApplicationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISteamSignInService _steamSignInService;
        private readonly ISteamService _steamService;

        public SteamLinkApplicationService(IUserRepository userRepository, ISteamSignInService steamSignInService, ISteamService steamService)
        {
            _userRepository = userRepository;
            _steamSignInService = steamSignInService;
            _steamService = steamService;
        }

        public string GetSignInUrl(string returnTo, string realm)
        {
            return _steamSignInService.GetSignInUrl(returnTo, realm);
        }

        // a linked account is replaced by the new one; the link stays even with hidden game details,
        // the user only learns that Steam won't give their achievements
        public async Task<(LinkSteamResult Result, bool? IsGameDataPublic)> LinkAsync(string userId, IReadOnlyDictionary<string, string> parameters,
            string returnTo, CancellationToken ct = default)
        {
            var steamId = await _steamSignInService.VerifyAsync(parameters, returnTo, ct);
            if (steamId == null)
            {
                return (LinkSteamResult.InvalidAnswer, null);
            }

            if (await _userRepository.CustomAnyAsync(_userRepository.Where(u => u.SteamId == steamId && u.Id != userId)))
            {
                return (LinkSteamResult.TakenByAnotherUser, null);
            }

            var user = await _userRepository.GetByIdAsync(userId);
            user.SteamId = steamId;
            await _userRepository.SaveAsync();

            try
            {
                return (LinkSteamResult.Linked, await _steamService.IsGameDataPublicAsync(steamId, ct));
            }
            catch (SteamUnavailableException)
            {
                return (LinkSteamResult.Linked, null);
            }
        }

        public async Task UnlinkAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            user.SteamId = null;
            await _userRepository.SaveAsync();
        }

        public async Task<bool?> IsGameDataPublicAsync(string userId, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user?.SteamId == null)
            {
                return null;
            }

            return await _steamService.IsGameDataPublicAsync(user.SteamId, ct);
        }
    }
}
