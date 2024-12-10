using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ApplicationInterfaces
{
    public interface IGameApplicationService
    {
        public Task<GameObject> FindGameByNameAsync(string name);
        public Task<GameObject> CreateGameAsync(GameObject gameObject);
        public Task SuggestGameAsync(int gameId);
    }
}
