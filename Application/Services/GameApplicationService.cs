using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GameApplicationService: IGameApplicationService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GameApplicationService(
            IUserRepository userRepository, 
            IMapper mapper,
            IGameRepository gameRepository) 
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _gameRepository = gameRepository;
        }

        public async Task<GameObject> FindGameByNameAsync(string name)
        {
            return new GameObject();
        }

        public async Task<GameObject> CreateGameAsync(GameObject gameObject)
        {

            return new GameObject();
        }

        public async Task SuggestGameAsync(int gameId)
        {
            return;
        }
    }
}
