using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IGameRepository: IBaseRepository<Game>
    {
        // the favorite games of the player with the games, in the order they were added
        public Task<List<UserFavoriteGame>> GetUserFavoritesAsync(string userId);
        // the player's favorite games become exactly the given ones
        public Task SetUserFavoritesAsync(string userId, List<UserFavoriteGame> favorites);
    }
}
