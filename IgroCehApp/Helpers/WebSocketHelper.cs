using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.WebSockets;
using System.Text;

namespace API.Helpers
{
    // Keeps websocket connections grouped by rooms. Every connection belongs to exactly one room
    // (client opens a separate socket per room), so rooms live and fail independently of each other.
    public class WebSocketHelper
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly Dictionary<string, HashSet<WebSocketRoomConnection>> _rooms = new();
        private readonly object _roomsLock = new();

        public async Task ListenRoomAsync(string room, WebSocket socket, CancellationToken ct)
        {
            var connection = new WebSocketRoomConnection(socket);
            AddConnection(room, connection);

            try
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                while (socket.State == WebSocketState.Open)
                {
                    // room is defined by the url, so incoming data is only drained until the close frame arrives
                    var result = await socket.ReceiveAsync(buffer, ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await connection.CloseAsync();
                    }
                }
            }
            catch (WebSocketException)
            {
                // client disappeared without close handshake
            }
            catch (OperationCanceledException)
            {
                // request was aborted or server is shutting down
            }
            finally
            {
                RemoveConnection(room, connection);
            }
        }

        public async Task SendToRoomAsync<T>(string room, string messageType, T payload)
        {
            WebSocketRoomConnection[] receivers;
            lock (_roomsLock)
            {
                if (!_rooms.TryGetValue(room, out var connections))
                {
                    return;
                }

                receivers = connections.ToArray();
            }

            var jsonString = JsonConvert.SerializeObject(new { Type = messageType, Payload = payload }, _serializerSettings);
            var message = Encoding.UTF8.GetBytes(jsonString);
            await Task.WhenAll(receivers.Select(receiver => receiver.SendAsync(message)));
        }

        private void AddConnection(string room, WebSocketRoomConnection connection)
        {
            lock (_roomsLock)
            {
                if (!_rooms.TryGetValue(room, out var connections))
                {
                    connections = new HashSet<WebSocketRoomConnection>();
                    _rooms[room] = connections;
                }

                connections.Add(connection);
            }
        }

        private void RemoveConnection(string room, WebSocketRoomConnection connection)
        {
            lock (_roomsLock)
            {
                if (_rooms.TryGetValue(room, out var connections) && connections.Remove(connection) && connections.Count == 0)
                {
                    _rooms.Remove(room);
                }
            }
        }
    }
}
