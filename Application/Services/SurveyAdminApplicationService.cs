using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class SurveyAdminApplicationService : ISurveyAdminApplicationService
    {
        private readonly ISurveyRepository _surveyRepository;

        public SurveyAdminApplicationService(ISurveyRepository surveyRepository)
        {
            _surveyRepository = surveyRepository;
        }

        // the survey is published at once and closes the one that was active, both in one save, so there is never a moment with two
        // active surveys; the request validation keeps the questions and their options within the limits and the rules of their types
        public async Task<(CreateSurveyResult Result, SurveyObject Survey, string ClosedSurveyId)> CreateAsync(string userId, string title, string description,
            bool showsResultsToRespondents, List<NewSurveyQuestionObject> questions)
        {
            var now = DateTimeOffset.UtcNow;
            var activeSurvey = await _surveyRepository.FirstOrDefaultAsync(s => s.IsActive == true);
            if (activeSurvey != null)
            {
                Close(activeSurvey, userId, now);
            }

            var survey = await _surveyRepository.AddAsync(new Survey
            {
                Id = Guid.NewGuid().ToString(),
                Title = NormalizeText(title),
                Description = NormalizeDescription(description),
                ShowsResultsToRespondents = showsResultsToRespondents,
                CreatedById = userId,
                IsActive = true,
                Questions = ToQuestions(questions, now),
            });
            if (!await _surveyRepository.TrySaveAsync())
            {
                return (CreateSurveyResult.ChangedMeanwhile, null, null);
            }

            return (CreateSurveyResult.Created, await GetAsync(survey.Id), activeSurvey?.Id);
        }

        // only the active survey changes; once a user has answered, the questions and their options stay as they are,
        // so every answer is to the question its respondent saw
        public async Task<(UpdateSurveyResult Result, SurveyObject Survey)> UpdateAsync(string surveyId, string title, string description,
            bool showsResultsToRespondents, List<NewSurveyQuestionObject> questions)
        {
            var survey = await _surveyRepository.GetWithQuestionsAsync(surveyId);
            if (survey == null)
            {
                return (UpdateSurveyResult.NotFound, null);
            }

            if (survey.IsActive != true)
            {
                return (UpdateSurveyResult.Closed, null);
            }

            if (!IsSameQuestions(survey, questions))
            {
                if (await _surveyRepository.CustomAnyAsync(_surveyRepository.Where(s => s.Id == surveyId && s.Responses.Any())))
                {
                    return (UpdateSurveyResult.QuestionsLocked, null);
                }

                // nobody has answered, so nothing holds the old questions: they are replaced as a whole
                survey.Questions.Clear();
                survey.Questions.AddRange(ToQuestions(questions, DateTimeOffset.UtcNow));
            }

            survey.Title = NormalizeText(title);
            survey.Description = NormalizeDescription(description);
            survey.ShowsResultsToRespondents = showsResultsToRespondents;
            if (!await _surveyRepository.TrySaveAsync())
            {
                return (UpdateSurveyResult.ChangedMeanwhile, null);
            }

            return (UpdateSurveyResult.Updated, await GetAsync(survey.Id));
        }

        // a closed survey takes no answers and is never opened again
        public async Task<ChangeSurveyResult> CloseAsync(string userId, string surveyId)
        {
            var survey = await _surveyRepository.FirstOrDefaultAsync(s => s.Id == surveyId);
            if (survey == null)
            {
                return ChangeSurveyResult.NotFound;
            }

            if (survey.IsActive != true)
            {
                return ChangeSurveyResult.Unavailable;
            }

            Close(survey, userId, DateTimeOffset.UtcNow);

            return await _surveyRepository.TrySaveAsync() ? ChangeSurveyResult.Changed : ChangeSurveyResult.Unavailable;
        }

        // soft delete: the survey with its answers is only hidden, so it can be restored by unsetting IsDeleted;
        // the active one is closed too, otherwise it would stay the active survey nobody sees and no new one could be made
        public async Task<ChangeSurveyResult> DeleteAsync(string userId, string surveyId)
        {
            var survey = await _surveyRepository.FirstOrDefaultAsync(s => s.Id == surveyId);
            if (survey == null)
            {
                return ChangeSurveyResult.NotFound;
            }

            var now = DateTimeOffset.UtcNow;
            if (survey.IsActive == true)
            {
                Close(survey, userId, now);
            }

            survey.IsDeleted = true;
            survey.DeletedAt = now;
            survey.DeletedById = userId;

            return await _surveyRepository.TrySaveAsync() ? ChangeSurveyResult.Changed : ChangeSurveyResult.Unavailable;
        }

        public async Task<List<SurveyShortObject>> GetAllAsync()
        {
            return (await _surveyRepository.CustomToListAsync(_surveyRepository.Where(s => true)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SurveyShortObject
                {
                    Id = s.Id,
                    Title = s.Title,
                    IsActive = s.IsActive == true,
                    CreatedAt = s.CreatedAt,
                    ClosedAt = s.ClosedAt,
                    QuestionCount = s.Questions.Count(),
                    ResponseCount = s.Responses.Count(),
                }))).ToList();
        }

        public async Task<SurveyObject> GetAsync(string surveyId)
        {
            return (await _surveyRepository.CustomToListAsync(_surveyRepository.Where(s => s.Id == surveyId)
                .Select(s => new SurveyObject
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    ShowsResultsToRespondents = s.ShowsResultsToRespondents,
                    IsActive = s.IsActive == true,
                    CreatedAt = s.CreatedAt,
                    ClosedAt = s.ClosedAt,
                    ResponseCount = s.Responses.Count(),
                    Questions = s.Questions
                        .OrderBy(q => q.Order)
                        .Select(q => new SurveyQuestionObject
                        {
                            Id = q.Id,
                            Text = q.Text,
                            Type = (QuestionType)q.Type,
                            IsRequired = q.IsRequired,
                            AllowsOther = q.AllowsOther,
                            MaxSelections = q.MaxSelections,
                            Options = q.Options
                                .OrderBy(o => o.Order)
                                .Select(o => new SurveyOptionObject { Id = o.Id, Text = o.Text })
                                .ToList(),
                        })
                        .ToList(),
                }))).FirstOrDefault();
        }

        private static void Close(Survey survey, string userId, DateTimeOffset now)
        {
            survey.IsActive = null;
            survey.ClosedAt = now;
            survey.ClosedById = userId;
        }

        private static string NormalizeText(string text) => text.Trim();

        private static string NormalizeDescription(string description) =>
            string.IsNullOrWhiteSpace(description) ? null : description.Trim().Replace("\r\n", "\n");

        private static List<SurveyQuestion> ToQuestions(List<NewSurveyQuestionObject> questions, DateTimeOffset now) => questions
            .Select((question, order) => new SurveyQuestion
            {
                Id = Guid.NewGuid().ToString(),
                Order = order,
                Text = NormalizeText(question.Text),
                Type = (SurveyQuestionType)question.Type,
                IsRequired = question.IsRequired,
                AllowsOther = question.AllowsOther,
                MaxSelections = question.MaxSelections,
                CreatedAt = now,
                Options = question.Options
                    .Select((text, optionOrder) => new SurveyOption
                    {
                        Id = Guid.NewGuid().ToString(),
                        Order = optionOrder,
                        Text = NormalizeText(text),
                        CreatedAt = now,
                    })
                    .ToList(),
            })
            .ToList();

        // the same questions with the same options in the same order; then their rows stay, and a survey with answers can still
        // change its title and description
        private static bool IsSameQuestions(Survey survey, List<NewSurveyQuestionObject> questions)
        {
            var kept = survey.Questions.OrderBy(q => q.Order).ToList();
            return kept.Count == questions.Count && kept.Zip(questions).All(pair => IsSameQuestion(pair.First, pair.Second));
        }

        private static bool IsSameQuestion(SurveyQuestion kept, NewSurveyQuestionObject question) =>
            kept.Text == NormalizeText(question.Text)
            && kept.Type == (SurveyQuestionType)question.Type
            && kept.IsRequired == question.IsRequired
            && kept.AllowsOther == question.AllowsOther
            && kept.MaxSelections == question.MaxSelections
            && kept.Options.OrderBy(o => o.Order).Select(o => o.Text).SequenceEqual(question.Options.Select(NormalizeText));
    }
}
