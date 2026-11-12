using Domain.Enums;

namespace Domain.Entities
{
    public class SurveyQuestion : IBaseEntity
    {
        public string Id { get; set; }
        public string SurveyId { get; set; }
        public Survey Survey { get; set; }
        public int Order { get; set; }
        public string Text { get; set; }
        public SurveyQuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        // a question with options also takes an answer of the respondent's own, typed as a text
        public bool AllowsOther { get; set; }
        // the most options of a multiple choice, the own answer counts as one; empty when any number goes
        public int? MaxSelections { get; set; }
        public List<SurveyOption> Options { get; set; } = new List<SurveyOption>();
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
