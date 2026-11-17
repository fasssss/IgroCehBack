namespace API.RepR.Request
{
    public class GetEventPeriodLimitsRequest
    {
        // the start the creator has picked, the bounds of the end are counted from it; without it from today
        public DateOnly? StartDate { get; set; }
    }
}
