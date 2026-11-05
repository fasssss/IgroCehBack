using Application.DTO;

namespace Application.Interfaces
{
    // PSN through the API of the PlayStation App with the service account of the site, methods throw PsnUnavailableException when PSN can't be reached,
    // refuses the sign-in or there is no NPSSO, and when the requests of the site ran over their limit
    public interface IPsnService
    {
        // the account by its online id with its "About me", read anew every time because a player proves the account by a code written there;
        // null when there is no such account
        public Task<PsnProfileObject> GetProfileAsync(string onlineId, CancellationToken ct = default);
        // the trophies of a game with its name in the order of the game, TitleRef has the service found (see PsnLimits); titleRef may be the id alone;
        // null when the ref is wrong or there is no such trophy list
        public Task<PlatformTitleAchievementsObject> GetTitleTrophiesAsync(string titleRef, CancellationToken ct = default);
        // the earned trophies of a player in a game by their ids with the time they were earned, titleRef as the site saved it;
        // empty when the player hasn't played the game or it hasn't synced; null when the privacy settings of the player hide them
        public Task<Dictionary<string, DateTimeOffset>> GetEarnedTrophiesAsync(string accountId, string titleRef, CancellationToken ct = default);
    }
}
