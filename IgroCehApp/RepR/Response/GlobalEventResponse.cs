using Application.DTO;

namespace API.RepR.Response
{
    public class GlobalEventResponse
    {
        // empty when no global event is active
        public GlobalEventObject GlobalEvent { get; set; }
    }
}
