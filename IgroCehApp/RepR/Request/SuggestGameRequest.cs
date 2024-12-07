using Microsoft.AspNetCore.Mvc;

namespace API.RepR.Request
{
    public class SuggestGameRequest
    {
        public string EventRecordId { get; set; }
        public string GameName { get; set; }
        public IFormFile Image { get; set; }
    }
}
