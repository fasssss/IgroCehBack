namespace API.RepR.Request
{
    public class MoveEventToNextStageRequest
    {
        public string EventId { get; set; }
        public int StatusId { get; set; }
    }
}
