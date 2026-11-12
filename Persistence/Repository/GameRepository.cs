using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repository
{
    public class GameRepository: BaseRepository<Game>, IGameRepository
    {
        private readonly IgroCehContext _context;

        public GameRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserFavoriteGame>> GetUserFavoritesAsync(string userId)
        {
            return await _context.UserFavoriteGames
                .AsNoTracking()
                .Include(f => f.Game)
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.Order)
                .ToListAsync();
        }

        public async Task SetUserFavoritesAsync(string userId, List<UserFavoriteGame> favorites)
        {
            // kept rows are updated in place, as the preferences
            var existing = await _context.UserFavoriteGames.Where(f => f.UserId == userId).ToListAsync();
            _context.UserFavoriteGames.RemoveRange(existing.Where(f => favorites.All(n => n.GameId != f.GameId)));
            foreach (var favorite in favorites)
            {
                var kept = existing.Find(f => f.GameId == favorite.GameId);
                if (kept == null)
                {
                    _context.UserFavoriteGames.Add(favorite);
                }
                else
                {
                    kept.Order = favorite.Order;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
