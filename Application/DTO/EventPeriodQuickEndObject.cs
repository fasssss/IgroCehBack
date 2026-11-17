namespace Application.DTO
{
    // a quick length of an event with its own dates: the end day counted by the server and the length for the label, in days or in months
    public class EventPeriodQuickEndObject
    {
        public int? Days { get; set; }
        public int? Months { get; set; }
        public DateOnly End { get; set; }
    }
}
