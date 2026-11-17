using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;

namespace IgroCehApp.Tests
{
    // the page of an event listens to its room: the changes others make come as messages {type, payload}
    public class WebSocketTests : GuildTest
    {
        public WebSocketTests(ApiFactory factory) : base(factory)
        {
        }

        private async Task<WebSocket> Listen(string room, string userId = Nico)
        {
            var client = Factory.Server.CreateWebSocketClient();
            client.ConfigureRequest = request => request.Headers["Cookie"] = $"application_token={Token(userId)}";
            return await client.ConnectAsync(new Uri($"wss://localhost/api/ws/{room}"), CancellationToken.None);
        }

        private static async Task<JsonNode> Next(WebSocket socket)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var buffer = new byte[64 * 1024];
            var result = await socket.ReceiveAsync(buffer, timeout.Token);
            return JsonNode.Parse(Encoding.UTF8.GetString(buffer, 0, result.Count))!;
        }

        [Fact]
        public async Task Room_GetsTheChangesOfItsEvent()
        {
            var eventId = await CreateEvent();
            using var socket = await Listen($"event{eventId}");

            (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();
            var joined = await Next(socket);
            (await As(Sliva).Post("/api/moveEventToNextStage", new { eventId, statusId = 1 })).Ok();
            var moved = await Next(socket);
            (await As(Sliva).Post("/api/shuffleUsers", new { eventId })).Ok();
            var shuffled = await Next(socket);

            Assert.Equal(("addUserToEvent", Dayt), (joined.Str("type"), joined.Str("payload.participant.id")));
            Assert.Equal(("updateEventStage", 1), (moved.Str("type"), moved.Int("payload.moveToStage")));
            Assert.Equal("shuffleUsers", shuffled.Str("type"));
            Assert.Equal(2, shuffled.Arr("payload").Count);
        }

        [Fact]
        public async Task Room_GetsTheGuessesThePassingsAndTheDeletion()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { null!, Hollow });
            using var socket = await Listen("evente1");

            (await As(Sliva).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "e1-r0", gameId = Elden })).Ok();
            var guessed = await Next(socket);
            (await As(Sliva).Post("/api/activateEvent", new { eventId = "e1" })).Ok();
            var activated = await Next(socket);
            (await As(Sliva).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" })).Ok();
            var passed = await Next(socket);
            (await As(Admin).Post("/api/deleteEvent", new { eventId = "e1" })).Ok();
            var deleted = await Next(socket);

            Assert.Equal(("suggestGame", "e1-r0", Elden), (guessed.Str("type"), guessed.Str("payload.id"), guessed.Str("payload.game.id")));
            Assert.Equal(("updateEventStage", (int)EventStatusId.Active), (activated.Str("type"), activated.Int("payload.moveToStage")));
            Assert.Equal(("submitPassing", "e1-r0"), (passed.Str("type"), passed.Str("payload.id")));
            Assert.NotNull(passed.IntOrNull("payload.reward"));
            Assert.Equal(("deleteEvent", "e1"), (deleted.Str("type"), deleted.Str("payload.eventId")));
        }

        [Fact]
        public async Task Room_GetsTheChangesOfThePool()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);
            using var socket = await Listen("evente1");

            (await As(Sliva).Post("/api/addNewcomerPoolGame", new { eventId = "e1", gameId = Elden })).Ok();
            var changed = await Next(socket);

            Assert.Equal(("updateNewcomerPool", "e1"), (changed.Str("type"), changed.Str("payload.eventId")));
        }

        [Fact]
        public async Task Room_NeedsASignedInUser_AndAWebSocket()
        {
            var client = Factory.Server.CreateWebSocketClient();

            await Assert.ThrowsAnyAsync<Exception>(() => client.ConnectAsync(new Uri("wss://localhost/api/ws/evente1"), CancellationToken.None));
            (await As(Nico).Get("/api/ws/evente1")).Is(HttpStatusCode.BadRequest);
        }
    }
}
