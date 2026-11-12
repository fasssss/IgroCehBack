namespace Domain.Entities
{
    public class SurveyOption : IBaseEntity
    {
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public SurveyQuestion Question { get; set; }
        public int Order { get; set; }
        public string Text { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
