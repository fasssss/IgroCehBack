namespace Application.Consts
{
    // the database columns and the validation of a survey share these limits
    public static class SurveyLimits
    {
        public const int TitleMaxLength = 150;
        public const int DescriptionMaxLength = 2000;
        public const int MinQuestions = 1;
        public const int MaxQuestions = 30;
        public const int QuestionTextMaxLength = 500;
        // a question with options has at least two, the respondent's own answer isn't one of them
        public const int MinOptions = 2;
        public const int MaxOptions = 20;
        public const int OptionTextMaxLength = 200;
        // a limit of a multiple choice is at least this and less than everything there is to choose, otherwise it limits nothing
        public const int MinMaxSelections = 2;
        // the answer to a text question
        public const int AnswerTextMaxLength = 2000;
        // the respondent's own answer to a question with options
        public const int OtherTextMaxLength = 200;
    }
}
