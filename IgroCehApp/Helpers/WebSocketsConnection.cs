using System.Net.WebSockets;

namespace API.Helpers
{
    public class WebSocketsConnection
    {
        public string? UniqueIdentifier { get; set; }  // need only for excluding sender from recievers of this message

        public WebSocket WebSocket { get; set; }

        public HashSet<string> Rooms { get; set; } = new HashSet<string>();
    }
}
