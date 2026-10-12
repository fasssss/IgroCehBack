using System.Net.WebSockets;

namespace API.Helpers
{
    public class WebSocketRoomConnection
    {
        private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(5);

        private readonly WebSocket _socket;

        // WebSocket allows only one outstanding send, while messages to the same room may come from parallel requests
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public WebSocketRoomConnection(WebSocket socket)
        {
            _socket = socket;
        }

        public async Task SendAsync(byte[] message)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (_socket.State == WebSocketState.Open)
                {
                    using var timeout = new CancellationTokenSource(SendTimeout);
                    await _socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, timeout.Token);
                }
            }
            catch (Exception e) when (e is WebSocketException or OperationCanceledException or ObjectDisposedException)
            {
                // broken connection must not fail the request that produced the message,
                // listening loop of this connection removes it from the room
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public async Task CloseAsync()
        {
            await _sendLock.WaitAsync();
            try
            {
                if (_socket.State == WebSocketState.CloseReceived)
                {
                    await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                }
            }
            catch (Exception e) when (e is WebSocketException or ObjectDisposedException)
            {
                // client is already gone
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }
}
