using API.Helpers;
using FastEndpoints;

namespace API.Endpoints.Get
{
    public class WebSocketConnection : EndpointWithoutRequest
    {
        private readonly WebSocketHelper _webSocketHelper;

        public WebSocketConnection(WebSocketHelper webSocketHelper)
        {
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            // over HTTP/2 browsers open websocket with extended CONNECT request instead of GET (RFC 8441),
            // a GET-only route answers it with 405
            Verbs("GET", "CONNECT");
            Routes("/api/ws/{room}");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            var room = Route<string>("room")!;
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await _webSocketHelper.ListenRoomAsync(room, socket, ct);
        }
    }
}
