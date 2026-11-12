using Application.Enum;

namespace Application.DTO
{
    // a question the administrator puts into a survey
    public class NewSurveyQuestionObject
    {
        public string Text { get; set; }
        public QuestionType Type { get; set; }
        public bool IsRequired { get; set; }
        // a question with options also takes an answer of the respondent's own
        public bool AllowsOther { get; set; }
        // the most options of a multiple choice, the own answer counts as one; empty when any number goes
        public int? MaxSelections { get; set; }
        // the texts of the options in their order, empty for a text question
        public List<string> Options { get; set; }
    }
}
