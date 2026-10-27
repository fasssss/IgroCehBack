using Application.ApplicationInterfaces;
using Application.Consts;
using Application.DTO;
using Application.Enum;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class GlobalEventParticipationApplicationService : IGlobalEventParticipationApplicationService
    {
        private readonly IGlobalEventRepository _globalEventRepository;
        private readonly IGlobalEventParticipantRepository _participantRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISteamService _steamService;

        public GlobalEventParticipationApplicationService(IGlobalEventRepository globalEventRepository, IGlobalEventParticipantRepository participantRepository,
            IUserRepository userRepository, ISteamService steamService)
        {
            _globalEventRepository = globalEventRepository;
            _participantRepository = participantRepository;
            _userRepository = userRepository;
            _steamService = steamService;
        }

        public async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> GetAsync(string userId, string globalEventId)
        {
            if (!await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(ge => ge.Id == globalEventId)))
            {
                return (ParticipateGlobalEventResult.NotFound, null);
            }

            return (ParticipateGlobalEventResult.Succeeded, await GetParticipationAsync(userId, globalEventId));
        }

        // three games drop at once and the player picks one of them; the games stay in the event, other players get them too
        public async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> RollAsync(string userId, string globalEventId,
            CancellationToken ct = default)
        {
            var globalEvent = await _globalEventRepository.GetWithGamesAsync(globalEventId);
            if (globalEvent == null)
            {
                return (ParticipateGlobalEventResult.NotFound, null);
            }

            if (globalEvent.FinishedAt != null)
            {
                return (ParticipateGlobalEventResult.Finished, null);
            }

            if (await _participantRepository.CustomAnyAsync(_participantRepository.Where(p => p.GlobalEventId == globalEventId && p.UserId == userId)))
            {
                return (ParticipateGlobalEventResult.AlreadyRolled, null);
            }

            var rolledGames = await RollGamesAsync(userId, globalEvent, null, ct);
            if (rolledGames.Count == 0)
            {
                return (ParticipateGlobalEventResult.NoGames, null);
            }

            await _participantRepository.AddAsync(new GlobalEventParticipant
            {
                Id = Guid.NewGuid().ToString(),
                GlobalEventId = globalEventId,
                UserId = userId,
                RolledAt = DateTimeOffset.UtcNow,
                RolledGames = rolledGames,
            });
            if (!await _participantRepository.TrySaveAsync())
            {
                return (ParticipateGlobalEventResult.AlreadyRolled, null);
            }

            return (ParticipateGlobalEventResult.Succeeded, await GetParticipationAsync(userId, globalEventId));
        }

        public async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> PickAsync(string userId, string globalEventId, int appId)
        {
            var (result, participant) = await GetParticipantAsync(userId, globalEventId);
            if (result != ParticipateGlobalEventResult.Succeeded)
            {
                return (result, null);
            }

            if (participant.PickedGameId != null)
            {
                return (ParticipateGlobalEventResult.AlreadyPicked, null);
            }

            var rolledGame = participant.RolledGames.FirstOrDefault(rg => rg.GlobalEventGame.Game.VgcId == appId);
            if (rolledGame == null)
            {
                return (ParticipateGlobalEventResult.NotRolledGame, null);
            }

            participant.PickedGameId = rolledGame.GlobalEventGameId;
            participant.PickedAt = DateTimeOffset.UtcNow;
            if (!await _participantRepository.TrySaveAsync())
            {
                return (ParticipateGlobalEventResult.Conflict, null);
            }

            return (ParticipateGlobalEventResult.Succeeded, await GetParticipationAsync(userId, globalEventId));
        }

        // a week after the pick the player may refuse the game, e.g. it costs too much: three new games drop at once,
        // the refused one isn't among them this time
        public async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> RefuseAsync(string userId, string globalEventId,
            CancellationToken ct = default)
        {
            var (result, participant) = await GetParticipantAsync(userId, globalEventId);
            if (result != ParticipateGlobalEventResult.Succeeded)
            {
                return (result == ParticipateGlobalEventResult.NotRolled ? ParticipateGlobalEventResult.NotPicked : result, null);
            }

            if (participant.PickedAt == null)
            {
                return (ParticipateGlobalEventResult.NotPicked, null);
            }

            if (DateTimeOffset.UtcNow < RefusableAt(participant.PickedAt.Value))
            {
                return (ParticipateGlobalEventResult.TooEarly, null);
            }

            var rolledGames = await RollGamesAsync(userId, await _globalEventRepository.GetWithGamesAsync(globalEventId), participant.PickedGameId, ct);
            if (rolledGames.Count == 0)
            {
                return (ParticipateGlobalEventResult.NoGames, null);
            }

            participant.RolledGames.Clear();
            participant.RolledGames.AddRange(rolledGames);
            participant.RolledAt = DateTimeOffset.UtcNow;
            participant.PickedGameId = null;
            participant.PickedAt = null;
            if (!await _participantRepository.TrySaveAsync())
            {
                return (ParticipateGlobalEventResult.Conflict, null);
            }

            return (ParticipateGlobalEventResult.Succeeded, await GetParticipationAsync(userId, globalEventId));
        }

        private static DateTimeOffset RefusableAt(DateTimeOffset pickedAt) => pickedAt.AddDays(GlobalEventLimits.RefusalDelayDays);

        // the participant of an event that is still active
        private async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipant Participant)> GetParticipantAsync(string userId, string globalEventId)
        {
            var participant = await _participantRepository.GetWithGamesAsync(globalEventId, userId);
            if (participant == null)
            {
                return (await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(ge => ge.Id == globalEventId))
                    ? ParticipateGlobalEventResult.NotRolled
                    : ParticipateGlobalEventResult.NotFound, null);
            }

            return participant.GlobalEvent.FinishedAt != null
                ? (ParticipateGlobalEventResult.Finished, null)
                : (ParticipateGlobalEventResult.Succeeded, participant);
        }

        // up to three random games of the event, the refused one left out
        private async Task<List<GlobalEventRolledGame>> RollGamesAsync(string userId, GlobalEvent globalEvent, string refusedGameId, CancellationToken ct)
        {
            var games = globalEvent.Games.Where(geg => geg.Id != refusedGameId).ToList();
            var steamId = (await _userRepository.CustomToListAsync(_userRepository.Where(u => u.Id == userId).Select(u => u.SteamId))).FirstOrDefault();
            if (steamId != null)
            {
                var passedGameIds = await GetPassedGameIdsAsync(steamId, games, ct);
                games = games.Where(geg => !passedGameIds.Contains(geg.Id)).ToList();
            }

            var shuffled = games.ToArray();
            Random.Shared.Shuffle(shuffled);
            return shuffled
                .Take(GlobalEventLimits.RolledGames)
                .Select((geg, order) => new GlobalEventRolledGame
                {
                    Id = Guid.NewGuid().ToString(),
                    GlobalEventGameId = geg.Id,
                    Order = order,
                    CreatedAt = DateTimeOffset.UtcNow,
                })
                .ToList();
        }

        // the games where the player has unlocked a chosen achievement already, they don't drop; when Steam doesn't answer
        // or hides the game details, nothing is known and every game drops, as without Steam
        private async Task<HashSet<string>> GetPassedGameIdsAsync(string steamId, List<GlobalEventGame> games, CancellationToken ct)
        {
            try
            {
                var ownedAppIds = await _steamService.GetOwnedAppIdsAsync(steamId, ct);
                if (ownedAppIds == null)
                {
                    return new HashSet<string>();
                }

                // only an owned game with achievements can have one unlocked, Steam is asked about all of them at once
                var checkedGames = games.Where(geg => geg.Achievements.Count > 0 && ownedAppIds.Contains(geg.Game.VgcId ?? 0)).ToList();
                var unlocked = await Task.WhenAll(checkedGames.Select(geg => _steamService.GetUnlockedAchievementsAsync(steamId, geg.Game.VgcId ?? 0, ct)));
                if (unlocked.Any(achievements => achievements == null))
                {
                    return new HashSet<string>();
                }

                return checkedGames
                    .Where((geg, index) => geg.Achievements.Any(a => unlocked[index].ContainsKey(a.ApiName)))
                    .Select(geg => geg.Id)
                    .ToHashSet();
            }
            catch (SteamUnavailableException)
            {
                return new HashSet<string>();
            }
        }

        private async Task<GlobalEventParticipationObject> GetParticipationAsync(string userId, string globalEventId)
        {
            var participation = (await _participantRepository.CustomToListAsync(_participantRepository
                .Where(p => p.GlobalEventId == globalEventId && p.UserId == userId)
                .Select(p => new GlobalEventParticipationObject
                {
                    RolledAppIds = p.RolledGames.OrderBy(rg => rg.Order).Select(rg => rg.GlobalEventGame.Game.VgcId ?? 0).ToList(),
                    RolledAt = p.RolledAt,
                    PickedAppId = p.PickedGame.Game.VgcId,
                    PickedAt = p.PickedAt,
                }))).FirstOrDefault();
            if (participation?.PickedAt != null)
            {
                participation.RefusableAt = RefusableAt(participation.PickedAt.Value);
            }

            return participation;
        }
    }
}
