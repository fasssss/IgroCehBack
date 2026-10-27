using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository
{
    public class GlobalEventClaimRepository : BaseRepository<GlobalEventClaim>, IGlobalEventClaimRepository
    {
        private readonly IgroCehContext _context;

        public GlobalEventClaimRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }

        public async Task<GlobalEventClaim> GetWithParticipantAsync(string claimId)
        {
            return await _context.GlobalEventClaims
                .Include(c => c.Participant)
                .FirstOrDefaultAsync(c => c.Id == claimId);
        }

        // the status of a claim is its concurrency token, the second of two decisions fails
        public async Task<bool> TrySaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
