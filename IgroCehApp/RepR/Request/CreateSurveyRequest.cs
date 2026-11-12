using Application.Consts;
using Application.DTO;
using Application.Enum;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    // the survey is published as it is created, there are no drafts
    public class CreateSurveyRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        // the respondents see how the community answered the questions with options
        public bool ShowsResultsToRespondents { get; set; }
        public List<NewSurveyQuestionObject> Questions { get; set; }
    }

    public class CreateSurveyValidator : Validator<CreateSurveyRequest>
    {
        public CreateSurveyValidator()
        {
            RuleFor(r => r.Title).Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage("The survey needs a title")
                .MaximumLength(SurveyLimits.TitleMaxLength);
            RuleFor(r => r.Description).MaximumLength(SurveyLimits.DescriptionMaxLength);
            RuleFor(r => r.Questions).Cascade(CascadeMode.Stop).NotNull()
                .Must(questions => questions.Count >= SurveyLimits.MinQuestions).WithMessage("The survey needs a question")
                .Must(questions => questions.Count <= SurveyLimits.MaxQuestions).WithMessage($"A survey can have up to {SurveyLimits.MaxQuestions} questions");
            RuleForEach(r => r.Questions).ChildRules(question =>
            {
                question.RuleFor(q => q.Text).Must(text => !string.IsNullOrWhiteSpace(text)).WithMessage("A question needs its text")
                    .MaximumLength(SurveyLimits.QuestionTextMaxLength);
                question.RuleFor(q => q.Type).IsInEnum();
                // without the list a text question and a question that lost its options would look the same
                question.RuleFor(q => q.Options).NotNull().WithMessage("A question needs the list of its options, empty for a text one");
                question.When(q => q.Options != null && q.Type != QuestionType.Text, () =>
                {
                    question.RuleFor(q => q.Options).Cascade(CascadeMode.Stop)
                        .Must(options => options.Count >= SurveyLimits.MinOptions).WithMessage($"A question with options needs at least {SurveyLimits.MinOptions}")
                        .Must(options => options.Count <= SurveyLimits.MaxOptions).WithMessage($"A question can have up to {SurveyLimits.MaxOptions} options")
                        .Must(options => options.All(text => !string.IsNullOrWhiteSpace(text))).WithMessage("An option needs its text")
                        .Must(options => options.All(text => text.Length <= SurveyLimits.OptionTextMaxLength)).WithMessage($"An option is up to {SurveyLimits.OptionTextMaxLength} characters")
                        .Must(options => options.Select(text => text.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == options.Count).WithMessage("An option is added once");
                });
                question.When(q => q.Options != null && q.Type == QuestionType.Text, () =>
                {
                    question.RuleFor(q => q.Options).Must(options => options.Count == 0).WithMessage("A text question has no options");
                    question.RuleFor(q => q.AllowsOther).Equal(false).WithMessage("Only a question with options takes an own answer");
                });
                question.RuleFor(q => q.MaxSelections).Null().WithMessage("Only a multiple choice has a limit of options")
                    .When(q => q.Type != QuestionType.MultipleChoice);
                // the own answer is one more thing to choose; a limit that lets everything be chosen limits nothing
                question.RuleFor(q => q.MaxSelections)
                    .Must((q, max) => max >= SurveyLimits.MinMaxSelections && max < q.Options.Count + (q.AllowsOther ? 1 : 0))
                    .WithMessage($"The limit of a multiple choice is at least {SurveyLimits.MinMaxSelections} and less than everything there is to choose")
                    .When(q => q.Type == QuestionType.MultipleChoice && q.MaxSelections != null && q.Options != null);
            });
        }
    }
}
