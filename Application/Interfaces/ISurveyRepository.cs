using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISurveyRepository : IBaseRepository<Survey>
    {
        // the survey with its questions and their options, tracked for a change; null when there is no such survey
        public Task<Survey> GetWithQuestionsAsync(string surveyId);
        // false when another request changed the same first: the active survey was closed, or the survey was answered
        public Task<bool> TrySaveAsync();
    }
}
