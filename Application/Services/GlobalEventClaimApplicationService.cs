using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Enums;

namespace Application.Services
{
    public class GlobalEventClaimApplicationService : IGlobalEventClaimApplicationService
    {
        private readonly IGlobalEventRepository _globalEventRepository;
        private readonly IGlobalEventClaimRepository _claimRepository;

        public GlobalEventClaimApplicationService(IGlobalEventRepository globalEventRepository, IGlobalEventClaimRepository claimRepository)
        {
            _globalEventRepository = globalEventRepository;
            _claimRepository = claimRepository;
        }

        public async Task<List<GlobalEventClaimObject>> GetClaimsAsync(string globalEventId)
        {
            if (!await _globalEventRepository.CustomAnyAsync(_globalEventRepository.Where(ge => ge.Id == globalEventId)))
            {
                return null;
            }

            var claims = await _claimRepository.CustomToListAsync(_claimRepository
                .Where(c => c.Participant.GlobalEventId == globalEventId)
                .Select(c => new GlobalEventClaimObject
                {
                    Id = c.Id,
                    Player = new PlayerShortObject { Id = c.Participant.User.Id, UserName = c.Participant.User.UserName, AvatarUrl = c.Participant.User.AvatarUrl },
                    AppId = c.GlobalEventGame.Game.VgcId ?? 0,
                    Platform = (ClaimPlatform)c.Platform,
                    Profile = c.Profile,
                    Status = (ClaimStatus)c.Status,
                    Comment = c.Comment,
                    CreatedAt = c.CreatedAt,
                    DecidedAt = c.DecidedAt,
                    DecidedBy = c.DecidedBy == null ? null : new PlayerShortObject { Id = c.DecidedBy.Id, UserName = c.DecidedBy.UserName, AvatarUrl = c.DecidedBy.AvatarUrl },
                }));

            return claims
                .OrderBy(c => c.Status != ClaimStatus.Pending)
                .ThenBy(c => c.Status == ClaimStatus.Pending ? c.CreatedAt : null)
                .ThenByDescending(c => c.DecidedAt)
                .ToList();
        }

        // an approved claim passes the picked game, after a rejected one the player may send a new claim; a claim is decided once,
        // also after the event is finished
        public async Task<DecideGlobalEventClaimResult> DecideAsync(string userId, string claimId, bool isApproved, string comment)
        {
            var claim = await _claimRepository.GetWithParticipantAsync(claimId);
            if (claim == null)
            {
                return DecideGlobalEventClaimResult.NotFound;
            }

            if (claim.Status != GlobalEventClaimStatus.Pending)
            {
                return DecideGlobalEventClaimResult.AlreadyDecided;
            }

            claim.Status = isApproved ? GlobalEventClaimStatus.Approved : GlobalEventClaimStatus.Rejected;
            // the reason is written only for a rejection
            claim.Comment = isApproved || string.IsNullOrWhiteSpace(comment) ? null : comment.Trim().Replace("\r\n", "\n");
            claim.DecidedById = userId;
            claim.DecidedAt = DateTimeOffset.UtcNow;
            if (isApproved)
            {
                claim.Participant.PassedAt ??= DateTimeOffset.UtcNow;
            }

            return await _claimRepository.TrySaveAsync() ? DecideGlobalEventClaimResult.Decided : DecideGlobalEventClaimResult.AlreadyDecided;
        }
    }
}
