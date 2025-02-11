﻿using API.Helpers;
using API.RepR.Request;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Text;
using Newtonsoft.Json;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;

namespace API.Endpoints.Get
{
    public class WebSocketConnection: Endpoint<string, EmptyResult>
    {
        private WebSocketHelper _websocketHelper;

        public WebSocketConnection(WebSocketHelper webSocketHelper) 
        {
            _websocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Get("/api/ws");
        }

        public override async Task<EmptyResult> ExecuteAsync(string s, CancellationToken ct)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                while (socket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    byte[] requestBuffer = new byte[4096];
                    var buffer = new ArraySegment<byte>(requestBuffer);
                    var recievedMessage = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    var parsedRequest = JsonConvert.DeserializeObject<WebSocketRequest>(Encoding.ASCII.GetString(requestBuffer, 0, recievedMessage.Count));
                    if (parsedRequest?.Type == "join")
                    {
                        var roomsCount = await _websocketHelper.AddWebSocketRoom(socket, parsedRequest.Payload);
                    }

                    if (parsedRequest?.Type == "leave")
                    {
                        var roomsCount = await _websocketHelper.RemoveWebSocketRoom(socket, parsedRequest.Payload);
                        if (roomsCount == 0)
                        {
                            await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                            return new EmptyResult();
                        }
                    }
                }

                Console.WriteLine(socket.State);

                if(socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    null,
                    CancellationToken.None);
                    await _websocketHelper.RemoveWebSocketEntirely(socket);
                }

                socket.Abort();
                await _websocketHelper.RemoveWebSocketEntirely(socket);
            }

            return new EmptyResult();
        }
    }
}
