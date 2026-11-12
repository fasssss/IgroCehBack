using Application.Enum;

namespace Application.DTO
{
    public class SurveyObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        // the respondents see how the community answered the questions with options
        public bool ShowsResultsToRespondents { get; set; }
        // one survey is active at a time, the others are closed for good
        public bool IsActive { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        // empty while the survey is active
        public DateTimeOffset? ClosedAt { get; set; }
        // the users who answered; with any of them the questions can't change
        public int ResponseCount { get; set; }
        public List<SurveyQuestionObject> Questions { get; set; }
    }

    public class SurveyQuestionObject
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        public bool AllowsOther { get; set; }
        public int? MaxSelections { get; set; }
        // empty for a text question
        public List<SurveyOptionObject> Options { get; set; }
    }

    public class SurveyOptionObject
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }

    // a survey in the list of the administrator, without its questions
    public class SurveyShortObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public int QuestionCount { get; set; }
        public int ResponseCount { get; set; }
    }
}
