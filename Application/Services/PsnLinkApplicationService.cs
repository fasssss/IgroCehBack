using System.Security.Cryptography;
using Application.ApplicationInterfaces;
using Application.Consts;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;

namespace Application.Services
{
    public class PsnLinkApplicationService : IPsnLinkApplicationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPsnService _psnService;
        private readonly IPsnLinkStore _linkStore;

        public PsnLinkApplicationService(IUserRepository userRepository, IPsnService psnService, IPsnLinkStore linkStore)
        {
            _userRepository = userRepository;
            _psnService = psnService;
            _linkStore = linkStore;
        }

        // PSN isn't asked here, the account is proved only at the confirmation. The code stays while it lives, even for another online id:
        // the user may already have typed it on the console and only have mistyped the online id here
        public string Start(string userId, string onlineId)
        {
            var code = _linkStore.GetPending(userId)?.Code ?? GenerateCode();
            _linkStore.SetPending(userId, new PendingPsnLinkObject { OnlineId = onlineId, Code = code });
            return code;
        }

        // links the account that has the online id now, only its owner can put the code in its About me;
        // a linked account is replaced by the new one; the online id can change in PSN, so it is refreshed on every link
        public async Task<(LinkPsnResult Result, string OnlineId)> ConfirmAsync(string userId, CancellationToken ct = default)
        {
            var pending = _linkStore.GetPending(userId);
            if (pending == null)
            {
                return (LinkPsnResult.NotStarted, null);
            }

            if (!_linkStore.TryCountAttempt(userId))
            {
                return (LinkPsnResult.TooManyAttempts, null);
            }

            var profile = await _psnService.GetProfileAsync(pending.OnlineId, ct);
            if (profile == null)
            {
                return (LinkPsnResult.NotFound, null);
            }

            // a console may change the case of what is typed
            if (profile.AboutMe?.Contains(pending.Code, StringComparison.OrdinalIgnoreCase) != true)
            {
                return (LinkPsnResult.CodeNotFound, null);
            }

            var user = await _userRepository.GetByIdAsync(userId);
            user.PsnAccountId = profile.AccountId;
            user.PsnOnlineId = profile.OnlineId;
            await _userRepository.SaveAsync();
            _linkStore.RemovePending(userId);
            return (LinkPsnResult.Linked, profile.OnlineId);
        }

        public async Task UnlinkAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            user.PsnAccountId = null;
            user.PsnOnlineId = null;
            await _userRepository.SaveAsync();
        }

        private static string GenerateCode()
        {
            return PsnLimits.LinkCodePrefix + RandomNumberGenerator.GetString(PsnLimits.LinkCodeAlphabet, PsnLimits.LinkCodeLength);
        }
    }
}
