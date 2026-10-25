using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    // participants fill the pool of an event during the registration, newcomers roll games from it once the event is active
    public class NewcomerPoolApplicationService : INewcomerPoolApplicationService
    {
        private readonly INewcomerPoolRepository _poolRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IGameRepository _gameRepository;

        public NewcomerPoolApplicationService(INewcomerPoolRepository poolRepository, IEventRepository eventRepository, IGameRepository gameRepository)
        {
            _poolRepository = poolRepository;
            _eventRepository = eventRepository;
            _gameRepository = gameRepository;
        }

        // seen by the members of the event's guild, like the event itself; the authors stay hidden
        public async Task<NewcomerPoolObject> GetPoolAsync(string userId, string eventId)
        {
            var canSee = await _eventRepository.CustomAnyAsync(_eventRepository
                .Where(e => e.Id == eventId && e.AllowsNewcomers && e.Guild.UserGuilds.Any(ug => ug.UserId == userId)));
            if (!canSee)
            {
                return null;
            }

            var games = await _poolRepository.CustomToListAsync(_poolRepository
                .Where(pg => pg.EventId == eventId)
                .OrderBy(pg => pg.CreatedAt)
                .ThenBy(pg => pg.Id)
                .Select(pg => new NewcomerPoolGameObject
                {
                    Id = pg.Id,
                    Game = new GameObject { Id = pg.Game.Id, Name = pg.Game.Name, ImageUrl = pg.Game.ImageUrl },
                    IsMine = pg.AuthorId == userId,
                }));

            return new NewcomerPoolObject
            {
                Games = games.ToList(),
                MinGames = NewcomerPoolLimits.MinGames,
                GamesPerPlayer = NewcomerPoolLimits.GamesPerPlayer,
            };
        }

        // a participant puts a game while the event gathers players, as many as one may; a game is in the pool once
        public async Task<(ChangeNewcomerPoolResult Result, NewcomerPoolGameObject PoolGame)> AddGameAsync(string userId, string eventId, string gameId)
        {
            var eventEntity = await _eventRepository.FirstOrDefaultAsync(e => e.Id == eventId);
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (eventEntity == null || game == null)
            {
                return (ChangeNewcomerPoolResult.NotFound, null);
            }

            if (!await IsParticipantAsync(userId, eventId))
            {
                return (ChangeNewcomerPoolResult.Forbidden, null);
            }

            if (!IsOpen(eventEntity))
            {
                return (ChangeNewcomerPoolResult.Unavailable, null);
            }

            if (await IsInPoolAsync(eventId, gameId))
            {
                return (ChangeNewcomerPoolResult.AlreadyInPool, null);
            }

            if (await _poolRepository.CountAsync(eventId, userId) >= NewcomerPoolLimits.GamesPerPlayer)
            {
                return (ChangeNewcomerPoolResult.LimitReached, null);
            }

            var poolGame = await _poolRepository.AddAsync(new NewcomerPoolGame { Id = Guid.NewGuid().ToString(), EventId = eventId, GameId = gameId, AuthorId = userId });
            await _poolRepository.SaveAsync();

            return (ChangeNewcomerPoolResult.Changed, ToObject(poolGame, game));
        }

        // the author changes their game while the event gathers players
        public async Task<(ChangeNewcomerPoolResult Result, NewcomerPoolGameObject PoolGame)> ReplaceGameAsync(string userId, string eventId, string poolGameId, string gameId)
        {
            var (result, poolGame) = await FindOwnChangeableAsync(userId, eventId, poolGameId);
            if (result != ChangeNewcomerPoolResult.Changed)
            {
                return (result, null);
            }

            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                return (ChangeNewcomerPoolResult.NotFound, null);
            }

            if (poolGame.GameId != gameId && await IsInPoolAsync(eventId, gameId))
            {
                return (ChangeNewcomerPoolResult.AlreadyInPool, null);
            }

            poolGame.GameId = gameId;
            await _poolRepository.SaveAsync();

            return (ChangeNewcomerPoolResult.Changed, ToObject(poolGame, game));
        }

        public async Task<ChangeNewcomerPoolResult> RemoveGameAsync(string userId, string eventId, string poolGameId)
        {
            var (result, poolGame) = await FindOwnChangeableAsync(userId, eventId, poolGameId);
            if (result != ChangeNewcomerPoolResult.Changed)
            {
                return result;
            }

            _poolRepository.Delete(poolGame);
            await _poolRepository.SaveAsync();

            return ChangeNewcomerPoolResult.Changed;
        }

        // the game of the pool, if its author may change it now
        private async Task<(ChangeNewcomerPoolResult Result, NewcomerPoolGame PoolGame)> FindOwnChangeableAsync(string userId, string eventId, string poolGameId)
        {
            var poolGame = await _poolRepository.FirstOrDefaultAsync(pg => pg.Id == poolGameId && pg.EventId == eventId);
            if (poolGame == null)
            {
                return (ChangeNewcomerPoolResult.NotFound, null);
            }

            if (poolGame.AuthorId != userId)
            {
                return (ChangeNewcomerPoolResult.Forbidden, null);
            }

            if (!IsOpen(await _eventRepository.FirstOrDefaultAsync(e => e.Id == eventId)))
            {
                return (ChangeNewcomerPoolResult.Unavailable, null);
            }

            if (await _poolRepository.HasDroppedAsync(poolGameId))
            {
                return (ChangeNewcomerPoolResult.Dropped, null);
            }

            return (ChangeNewcomerPoolResult.Changed, poolGame);
        }

        private static bool IsOpen(Event eventEntity) => eventEntity.AllowsNewcomers && eventEntity.StatusId == EventStatusId.PlayersRegistration;

        private async Task<bool> IsParticipantAsync(string userId, string eventId) =>
            await _eventRepository.CustomAnyAsync(_eventRepository.Where(e => e.Id == eventId && e.EventRecords.Any(er => er.ParticipantId == userId)));

        private async Task<bool> IsInPoolAsync(string eventId, string gameId) =>
            await _poolRepository.CustomAnyAsync(_poolRepository.Where(pg => pg.EventId == eventId && pg.GameId == gameId));

        private static NewcomerPoolGameObject ToObject(NewcomerPoolGame poolGame, Game game) => new()
        {
            Id = poolGame.Id,
            Game = new GameObject { Id = game.Id, Name = game.Name, ImageUrl = game.ImageUrl },
            IsMine = true,
        };
    }
}
