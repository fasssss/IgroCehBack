using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository
{
    public class SurveyRepository : BaseRepository<Survey>, ISurveyRepository
    {
        private readonly IgroCehContext _context;

        public SurveyRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Survey> GetWithQuestionsAsync(string surveyId)
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == surveyId);
        }

        // the unique index of the active survey, its concurrency token and the answers that hold their questions fail the second of two requests
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
