using System.Net.WebSockets;

namespace API.Helpers
{
    public class WebSocketHelper
    {
        private List<WebSocketsConnection> _webSocketConnections;
        
        public WebSocketHelper() 
        {
            _webSocketConnections = new List<WebSocketsConnection>();
        }

        public async Task<int> AddWebSocketConnection(WebSocket socket, params string[] rooms)
        {
            var connection = _webSocketConnections.FirstOrDefault(wc => wc.WebSocket == socket);
            if (connection == null && rooms != null) 
            {
                connection = new WebSocketsConnection { WebSocket = socket };
                _webSocketConnections.Add(connection);
            }

            foreach (var room in rooms)
            {
                connection.Rooms.Add(room);
            }

            return connection?.Rooms?.Count ?? 0;
        }

        public async Task<int> RemoveWebSocketConnection(WebSocket socket, params string[] rooms)
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
    }
}
