using Application.ApplicationInterfaces;
using Application.Enum;
using Application.Interfaces;

namespace Application.Services
{
    public class XboxLinkApplicationService : IXboxLinkApplicationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IXboxSignInService _xboxSignInService;

        public XboxLinkApplicationService(IUserRepository userRepository, IXboxSignInService xboxSignInService)
        {
            _userRepository = userRepository;
            _xboxSignInService = xboxSignInService;
        }

        public string GetSignInUrl()
        {
            return _xboxSignInService.GetSignInUrl();
        }

        // a linked account is replaced by the new one; the gamertag can change in Xbox, so it is refreshed on every sign-in
        public async Task<(LinkXboxResult Result, string Gamertag)> LinkAsync(string userId, string code, CancellationToken ct = default)
        {
            var account = await _xboxSignInService.ClaimAsync(code, ct);
            if (account == null)
            {
                return (LinkXboxResult.InvalidCode, null);
            }

            if (await _userRepository.CustomAnyAsync(_userRepository.Where(u => u.XboxXuid == account.Xuid && u.Id != userId)))
            {
                return (LinkXboxResult.TakenByAnotherUser, null);
            }

            var user = await _userRepository.GetByIdAsync(userId);
            user.XboxXuid = account.Xuid;
            user.XboxGamertag = account.Gamertag;
            await _userRepository.SaveAsync();
            return (LinkXboxResult.Linked, account.Gamertag);
        }

        public async Task UnlinkAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            user.XboxXuid = null;
            user.XboxGamertag = null;
            await _userRepository.SaveAsync();
        }
    }
}
