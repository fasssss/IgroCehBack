using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository
{
    // tags have numeric ids and preferences have a composite key, so they don't fit the base repository
    public class PreferenceRepository : IPreferenceRepository
    {
        private readonly IgroCehContext _context;

        public PreferenceRepository(IgroCehContext context)
        {
            _context = context;
        }

        public async Task<List<PreferenceTag>> GetTagsAsync()
        {
            return await _context.PreferenceTags.AsNoTracking().OrderBy(t => t.Id).ToListAsync();
        }

        public async Task<List<UserPreference>> GetUserPreferencesAsync(string userId)
        {
            return await _context.UserPreferences
                .AsNoTracking()
                .Include(p => p.PreferenceTag)
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.Order)
                .ToListAsync();
        }

        public async Task SetUserPreferencesAsync(string userId, List<UserPreference> preferences)
        {
            // kept rows are updated in place: a removed and re-added row with the same key would clash in the change tracker
            var existing = await _context.UserPreferences.Where(p => p.UserId == userId).ToListAsync();
            _context.UserPreferences.RemoveRange(existing.Where(p => preferences.All(n => n.PreferenceTagId != p.PreferenceTagId)));
            foreach (var preference in preferences)
            {
                var kept = existing.Find(p => p.PreferenceTagId == preference.PreferenceTagId);
                if (kept == null)
                {
                    _context.UserPreferences.Add(preference);
                }
                else
                {
                    kept.IsAvoided = preference.IsAvoided;
                    kept.Order = preference.Order;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
