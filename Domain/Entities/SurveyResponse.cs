namespace Domain.Entities
{
    // the answers of one user to a survey; they are given once and don't change, so CreatedAt is the moment of the answer
    public class SurveyResponse : IBaseEntity
    {
        public string Id { get; set; }
        public string SurveyId { get; set; }
        public Survey Survey { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public List<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
