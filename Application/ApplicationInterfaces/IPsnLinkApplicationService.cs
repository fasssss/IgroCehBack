using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // the PSN account of a user. PSN has no sign-in for other sites, so the user proves the account by putting the code given here in About me
    public interface IPsnLinkApplicationService
    {
        // the code for About me; PSN isn't asked
        public string Start(string userId, string onlineId);
        // the online id of the linked account, null unless Linked; asks PSN, so it throws PsnUnavailableException
        public Task<(LinkPsnResult Result, string OnlineId)> ConfirmAsync(string userId, CancellationToken ct = default);
        public Task UnlinkAsync(string userId);
    }
}
