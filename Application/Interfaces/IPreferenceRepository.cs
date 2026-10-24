using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPreferenceRepository
    {
        public Task<List<PreferenceTag>> GetTagsAsync();
        // the preferences of the player with their tags, in the order they were added
        public Task<List<UserPreference>> GetUserPreferencesAsync(string userId);
        // the player's preferences become exactly the given ones
        public Task SetUserPreferencesAsync(string userId, List<UserPreference> preferences);
    }
}
