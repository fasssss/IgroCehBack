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

        public async Task<int> AddWebSocketRoom(WebSocket socket, params string[] rooms)
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
