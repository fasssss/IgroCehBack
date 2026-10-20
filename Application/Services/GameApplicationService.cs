using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Exceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
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
        private readonly IGameCatalogService _gameCatalogService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GameApplicationService(
            IUserRepository userRepository, 
            IMapper mapper,
            IGameRepository gameRepository,
            IGameCatalogService gameCatalogService) 
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _gameRepository = gameRepository;
            _gameCatalogService = gameCatalogService;
        }

        public async Task<GameObject> FindGameByNameAsync(string name)
        {
            var game = (await _gameRepository.CustomToListAsync(_gameRepository.OrderByCreationDate().Where(g => g.Name == name))).LastOrDefault();
            if(game != null)
            return new GameObject 
            { 
                Id = game.Id,
                Name = game.Name,
                ImageUrl = game.ImageUrl,
            };

            return null;
        }

        public async Task<GameObject> CreateGameAsync(GameObject gameObject)
        {
            var addedGame = await _gameRepository.AddAsync(new Game 
            {
                Id = Guid.NewGuid().ToString(),
                Name = gameObject.Name,
                ImageUrl = gameObject.ImageUrl,
                CreatorId = gameObject.CreatorId,
            });

            var result = await _gameRepository.SaveAsync();
            if (result > 0)
            {
                gameObject.Id = addedGame.Id;
                return gameObject;
            }

            return null;
        }

        public Task<List<CatalogGameObject>> SearchCatalogGamesAsync(string query, CancellationToken ct = default)
        {
            return _gameCatalogService.SearchGamesAsync(query, ct);
        }

        // returns id of the game in our database, a catalog game is saved on its first pick and reused after that
        public async Task<string> CreateGameFromCatalogAsync(string userId, int vgcId, CancellationToken ct = default)
        {
            var existingGame = await _gameRepository.FirstOrDefaultAsync(g => g.VgcId == vgcId);
            if (existingGame != null)
            {
                await FillMissingPlatformsAsync(existingGame, ct);
                return existingGame.Id;
            }

            var catalogGame = await _gameCatalogService.GetGameAsync(vgcId, ct);
            if (catalogGame == null)
            {
                return null;
            }

            var addedGame = await _gameRepository.AddAsync(new Game
            {
                Id = Guid.NewGuid().ToString(),
                Name = catalogGame.Name,
                ImageUrl = catalogGame.CoverUrl,
                SteamUrl = catalogGame.SteamUrl,
                VgcId = vgcId,
                Platforms = catalogGame.Platforms,
                CreatorId = userId,
            });

            try
            {
                await _gameRepository.SaveAsync();
            }
            catch
            {
                // another user could save the same catalog game at the same moment, VgcId is unique
                var concurrentlyAddedGame = await _gameRepository.FirstOrDefaultAsync(g => g.VgcId == vgcId);
                if (concurrentlyAddedGame == null)
                {
                    throw;
                }

                return concurrentlyAddedGame.Id;
            }

            return addedGame.Id;
        }

        // games saved before platforms were stored get them on the next pick,
        // the game was just found by search, so the catalog answers from its cache
        private async Task FillMissingPlatformsAsync(Game game, CancellationToken ct)
        {
            if (game.Platforms.Count > 0)
            {
                return;
            }

            try
            {
                var catalogGame = await _gameCatalogService.GetGameAsync(game.VgcId.Value, ct);
                if (catalogGame?.Platforms.Count > 0)
                {
                    game.Platforms = catalogGame.Platforms;
                    await _gameRepository.SaveAsync();
                }
            }
            catch (GameCatalogUnavailableException)
            {
                // platforms are optional, the game can be suggested without them
            }
        }
    }
}
