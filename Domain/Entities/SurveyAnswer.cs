namespace Domain.Entities
{
    // one chosen option, the respondent's own answer to a question with options (no option, only the text) or the text of a text question;
    // a multiple choice gives a row per chosen option
    public class SurveyAnswer : IBaseEntity
    {
        public string Id { get; set; }
        public string ResponseId { get; set; }
        public SurveyResponse Response { get; set; }
        public string QuestionId { get; set; }
        public SurveyQuestion Question { get; set; }
        public string? OptionId { get; set; }
        public SurveyOption? Option { get; set; }
        public string? Text { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
