using Microsoft.AspNetCore.Mvc;

namespace API.RepR.Request
{
    public class SuggestGameRequest
    {
        public string GameId { get; set; }
        public string EventRecordId { get; set; }
        public string EventId { get; set; }
    }
}
