﻿using Application.ApplicationInterfaces;
using Application.DTO;
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

        public async Task SuggestGameAsync(int gameId)
        {
            return;
        }
    }
}
