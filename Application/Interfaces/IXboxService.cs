using Application.DTO;

namespace Application.Interfaces
{
    // Xbox Live through OpenXBL with the key of the site, methods throw XboxUnavailableException when OpenXBL can't be reached,
    // refuses the key or there is no key
    public interface IXboxService
    {
        // the games of the Microsoft Store whose names start with the query, the title id known only for some of them,
        // or the games the store search finds when none starts with it; empty for a query shorter than 2 characters
        public Task<List<XboxTitleObject>> SearchTitlesAsync(string query, CancellationToken ct = default);
        // the title id of a product of the Microsoft Store, null when it has none (a bundle, an add-on) or there is no such product
        public Task<string> GetProductTitleIdAsync(string productId, CancellationToken ct = default);
        // null when the title has no achievements or there is no such title
        public Task<XboxTitleAchievementsObject> GetTitleAchievementsAsync(string titleId, CancellationToken ct = default);
        // the unlocked achievements of a player in a title by their ids with the time of the unlock, empty when the player hasn't played it;
        // null when the privacy settings of the player hide them
        public Task<Dictionary<string, DateTimeOffset>> GetUnlockedAchievementsAsync(string xuid, string titleId, CancellationToken ct = default);
    }
}
