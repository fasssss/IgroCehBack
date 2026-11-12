using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // the surveys of the whole site as their administrator makes them: a new survey is published at once and closes the active one
    public interface ISurveyAdminApplicationService
    {
        // ClosedSurveyId is the survey that was active until this one, empty when there was none
        public Task<(CreateSurveyResult Result, SurveyObject Survey, string ClosedSurveyId)> CreateAsync(string userId, string title, string description,
            bool showsResultsToRespondents, List<NewSurveyQuestionObject> questions);
        // the survey comes in full, the questions in their new order
        public Task<(UpdateSurveyResult Result, SurveyObject Survey)> UpdateAsync(string surveyId, string title, string description,
            bool showsResultsToRespondents, List<NewSurveyQuestionObject> questions);
        public Task<ChangeSurveyResult> CloseAsync(string userId, string surveyId);
        public Task<ChangeSurveyResult> DeleteAsync(string userId, string surveyId);
        // every survey, the latest first
        public Task<List<SurveyShortObject>> GetAllAsync();
        // null when there is no such survey or it is deleted
        public Task<SurveyObject> GetAsync(string surveyId);
    }
}
