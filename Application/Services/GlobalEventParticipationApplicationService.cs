using Application.ApplicationInterfaces;
using Application.Consts;
using Application.DTO;
using Application.Enum;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class GlobalEventParticipationApplicationService : IGlobalEventParticipationApplicationService
    {
        private readonly IGlobalEventRepository _globalEventRepository;
        private readonly IGlobalEventParticipantRepository _participantRepository;
        // every platform whose achievements prove the passing, one per AchievementPlatform
        private readonly IEnumerable<IAchievementPlatformChecker> _platformCheckers;

        public GlobalEventParticipationApplicationService(IGlobalEventRepository globalEventRepository, IGlobalEventParticipantRepository participantRepository,
            IEnumerable<IAchievementPlatformChecker> platformCheckers)
        {
            _globalEventRepository = globalEventRepository;
            _participantRepository = participantRepository;
            _platformCheckers = platformCheckers;
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
        // the refused one isn't among them this time; a passed game or one with a claim under review stays
        public async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> RefuseAsync(string userId, string globalEventId,
            CancellationToken ct = default)
        {
            var (result, participant) = await GetPickedParticipantAsync(userId, globalEventId);
            if (result != ParticipateGlobalEventResult.Succeeded)
            {
                return (result, null);
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

        // the platform asked is asked about its chosen achievements of the picked game, one unlocked after the pick passes it;
        // a player checks on each platform once in GlobalEventLimits.PassingCheckDelayMinutes, a check the platform didn't answer doesn't count
        public async Task<(ParticipateGlobalEventResult Result, PassingCheckResult Check, GlobalEventParticipationObject Participation)> CheckPassingAsync(
            string userId, string globalEventId, AchievementPlatform platform, CancellationToken ct = default)
        {
            var (result, participant) = await GetPickedParticipantAsync(userId, globalEventId);
            if (result != ParticipateGlobalEventResult.Succeeded)
            {
                return (result, default, null);
            }

            if (participant.PickedGame.Achievements.Count == 0)
            {
                return (ParticipateGlobalEventResult.NoAchievements, default, null);
            }

            var achievementPlatform = (GlobalEventAchievementPlatform)platform;
            if (!participant.PickedGame.Achievements.Any(a => a.Platform == achievementPlatform))
            {
                return (ParticipateGlobalEventResult.NoPlatformAchievements, default, null);
            }

            var checker = _platformCheckers.Single(c => c.Platform == platform);
            var accountId = await checker.GetAccountIdAsync(userId);
            if (accountId == null)
            {
                return (ParticipateGlobalEventResult.PlatformNotLinked, default, null);
            }

            var passingCheck = participant.PassingChecks.FirstOrDefault(pc => pc.Platform == achievementPlatform);
            var lastCheckedAt = passingCheck?.CheckedAt;
            if (lastCheckedAt != null && DateTimeOffset.UtcNow < CheckableAt(lastCheckedAt.Value))
            {
                return (ParticipateGlobalEventResult.CheckedRecently, default, null);
            }

            // the check is saved before the platform is asked, so of two checks at once the second fails to save
            if (passingCheck == null)
            {
                passingCheck = new GlobalEventPassingCheck
                {
                    Id = Guid.NewGuid().ToString(),
                    Platform = achievementPlatform,
                    CreatedAt = DateTimeOffset.UtcNow,
                };
                participant.PassingChecks.Add(passingCheck);
            }

            passingCheck.CheckedAt = DateTimeOffset.UtcNow;
            if (!await _participantRepository.TrySaveAsync())
            {
                return (ParticipateGlobalEventResult.Conflict, default, null);
            }

            PassingCheckResult check;
            try
            {
                check = await checker.CheckAsync(accountId, participant.PickedGame, participant.PickedAt.Value, ct);
            }
            catch (AchievementPlatformUnavailableException)
            {
                if (lastCheckedAt == null)
                {
                    participant.PassingChecks.Remove(passingCheck);
                }
                else
                {
                    passingCheck.CheckedAt = lastCheckedAt.Value;
                }

                await _participantRepository.TrySaveAsync();
                throw;
            }

            if (check == PassingCheckResult.Passed)
            {
                participant.PassedAt = DateTimeOffset.UtcNow;
                participant.PassedPlatform = achievementPlatform;
                await _participantRepository.SaveAsync();
            }

            return (ParticipateGlobalEventResult.Succeeded, check, await GetParticipationAsync(userId, globalEventId));
        }

        // the profile shows the passing on a platform, an administrator of the site checks it by hand
        public async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> SubmitClaimAsync(string userId, string globalEventId,
            ClaimPlatform platform, string profile)
        {
            var (result, participant) = await GetPickedParticipantAsync(userId, globalEventId);
            if (result != ParticipateGlobalEventResult.Succeeded)
            {
                return (result, null);
            }

            participant.Claims.Add(new GlobalEventClaim
            {
                Id = Guid.NewGuid().ToString(),
                GlobalEventGameId = participant.PickedGameId,
                Platform = (GlobalEventClaimPlatform)platform,
                Profile = profile.Trim(),
                Status = GlobalEventClaimStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
            });
            await _participantRepository.SaveAsync();

            return (ParticipateGlobalEventResult.Succeeded, await GetParticipationAsync(userId, globalEventId));
        }

        private static DateTimeOffset RefusableAt(DateTimeOffset pickedAt) => pickedAt.AddDays(GlobalEventLimits.RefusalDelayDays);

        private static DateTimeOffset CheckableAt(DateTimeOffset checkedAt) => checkedAt.AddMinutes(GlobalEventLimits.PassingCheckDelayMinutes);

        // the participant of an active event whose picked game isn't passed and has no claim under review
        private async Task<(ParticipateGlobalEventResult Result, GlobalEventParticipant Participant)> GetPickedParticipantAsync(string userId, string globalEventId)
        {
            var (result, participant) = await GetParticipantAsync(userId, globalEventId);
            if (result != ParticipateGlobalEventResult.Succeeded)
            {
                return (result == ParticipateGlobalEventResult.NotRolled ? ParticipateGlobalEventResult.NotPicked : result, null);
            }

            if (participant.PickedGameId == null)
            {
                return (ParticipateGlobalEventResult.NotPicked, null);
            }

            if (participant.PassedAt != null)
            {
                return (ParticipateGlobalEventResult.AlreadyPassed, null);
            }

            return participant.Claims.Any(c => c.Status == GlobalEventClaimStatus.Pending)
                ? (ParticipateGlobalEventResult.ClaimPending, null)
                : (ParticipateGlobalEventResult.Succeeded, participant);
        }

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
            var passedGameIds = await GetPassedGameIdsAsync(userId, games, ct);
            var shuffled = games.Where(geg => !passedGameIds.Contains(geg.Id)).ToArray();
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

        // the games where the player has unlocked a chosen achievement already on any linked platform, they don't drop;
        // a platform that doesn't answer tells nothing, as if it weren't linked
        private async Task<HashSet<string>> GetPassedGameIdsAsync(string userId, List<GlobalEventGame> games, CancellationToken ct)
        {
            var passedGameIds = new HashSet<string>();
            foreach (var checker in _platformCheckers)
            {
                var accountId = await checker.GetAccountIdAsync(userId);
                if (accountId == null)
                {
                    continue;
                }

                try
                {
                    passedGameIds.UnionWith(await checker.GetPassedGameIdsAsync(accountId, games, ct));
                }
                catch (AchievementPlatformUnavailableException)
                {
                }
            }

            return passedGameIds;
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
                    PassedAt = p.PassedAt,
                    PassedPlatform = (AchievementPlatform?)p.PassedPlatform,
                    PassingChecks = p.PassingChecks
                        .OrderBy(pc => pc.Platform)
                        .Select(pc => new GlobalEventPassingCheckObject { Platform = (AchievementPlatform)pc.Platform, CheckedAt = pc.CheckedAt })
                        .ToList(),
                    Claim = p.Claims
                        .Where(c => c.GlobalEventGameId == p.PickedGameId)
                        .OrderByDescending(c => c.CreatedAt)
                        .Select(c => new GlobalEventClaimObject
                        {
                            Id = c.Id,
                            AppId = c.GlobalEventGame.Game.VgcId ?? 0,
                            Platform = (ClaimPlatform)c.Platform,
                            Profile = c.Profile,
                            Status = (ClaimStatus)c.Status,
                            Comment = c.Comment,
                            CreatedAt = c.CreatedAt,
                            DecidedAt = c.DecidedAt,
                        })
                        .FirstOrDefault(),
                }))).FirstOrDefault();
            if (participation?.PickedAt != null)
            {
                participation.RefusableAt = RefusableAt(participation.PickedAt.Value);
            }

            foreach (var passingCheck in participation?.PassingChecks ?? new List<GlobalEventPassingCheckObject>())
            {
                passingCheck.CheckableAt = CheckableAt(passingCheck.CheckedAt);
            }

            return participation;
        }
    }
}
