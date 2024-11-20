using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace API.Helpers
{
    public class WebSocketHelper
    {
        private List<WebSocketsConnection> _webSocketConnections;
        private IHttpContextAccessor _httpContextAccessor;

        public WebSocketHelper(IHttpContextAccessor httpContextAccessor) 
        {
            _webSocketConnections = new List<WebSocketsConnection>();
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddWebSocketRoom(WebSocket socket, params string[] rooms)
        {
            var connection = _webSocketConnections.FirstOrDefault(wc => wc.WebSocket == socket);
            var applicationCookieAsId = _httpContextAccessor.HttpContext?.Request.Cookies["application_token"];

            if (connection == null && rooms != null) 
            {
                connection = new WebSocketsConnection { UniqueIdentifier = applicationCookieAsId, WebSocket = socket };
                _webSocketConnections.Add(connection);
            }

            foreach (var room in rooms)
            {
                connection.Rooms.Add(room);
            }

            return connection?.Rooms?.Count ?? 0;
        }

        public async Task<int> RemoveWebSocketRoom(WebSocket socket, params string[] rooms)
        {
            var connection = _webSocketConnections.FirstOrDefault(wc => wc.WebSocket == socket);
            if (connection == null)
            {
                return 0;
            }

            foreach (var room in rooms)
            {
                connection.Rooms.Remove(room);
            }

            if (connection.Rooms.Count == 0)
            {
                _webSocketConnections.Remove(connection);
            }

            return connection.Rooms.Count;
        }

        public async Task SendToRoomAsync<T>(string room, T message, string messageType = "", bool sendSelf = false)
        {
            var applicationCookieAsId = _httpContextAccessor.HttpContext?.Request.Cookies["application_token"];
            var connections = _webSocketConnections.Where(x => x.Rooms.Contains(room));
            if (!sendSelf)
            {
                connections = connections.Where(x => x.UniqueIdentifier != applicationCookieAsId);
            }

            string jsonString = JsonConvert.SerializeObject(new { Type = messageType, Payload = message }, new JsonSerializerSettings 
            { 
                ContractResolver = new CamelCasePropertyNamesContractResolver() 
            });
            byte[] requestBuffer = Encoding.UTF8.GetBytes(jsonString);
            var buffer = new ArraySegment<byte>(requestBuffer);
            foreach (var connection in connections)
            {
                await connection.WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task<bool> RemoveWebSocketEntirely(WebSocket socket)
        {
            var connection = _webSocketConnections.FirstOrDefault(wc => wc.WebSocket == socket);
            if(_webSocketConnections.Remove(connection))
            {
                return true;
            }

            return false;
        }
    }
}
