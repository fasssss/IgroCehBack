using Domain.Enums;

namespace IgroCehApp.Tests.Infrastructure
{
    // a test inside a guild: g1 of Sliva, Dayt and Nico with an administrator, a player who has left it and a stranger of another guild;
    // the games are saved by hand, without the catalog
    public abstract class GuildTest : ApiTest
    {
        protected const string G1 = "g1";
        protected const string G2 = "g2";
        protected const string Sliva = "sliva";
        protected const string Dayt = "dayt";
        protected const string Nico = "nico";
        protected const string Admin = "admin";
        protected const string Leaver = "leaver";
        protected const string Stranger = "stranger";
        protected const string Elden = "elden";
        protected const string Hollow = "hollow";
        protected const string Sekiro = "sekiro";
        protected const string Persona = "persona";

        protected GuildTest(ApiFactory factory) : base(factory)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await Seed(
                Make.User(Sliva, "Sliva"), Make.User(Dayt, "Dayt"), Make.User(Nico, "Nico"), Make.User(Admin, "Admin"), Make.User(Leaver, "Leaver"),
                Make.User(Stranger, "Stranger"),
                Make.Guild(G1, "Guild One"), Make.Guild(G2, "Guild Two"),
                Make.Member(Sliva, G1), Make.Member(Dayt, G1), Make.Member(Nico, G1), Make.Member(Admin, G1, isAdmin: true),
                Make.Member(Leaver, G1, isAdmin: true, hasLeft: true), Make.Member(Stranger, G2),
                Make.Game(Elden, Sliva, "Elden Ring"), Make.Game(Hollow, Sliva, "Hollow Knight"), Make.Game(Sekiro, Sliva, "Sekiro"),
                Make.Game(Persona, Sliva, "Persona 5"));
        }

        // an event at the given stage: every player has a record, the shuffle and the guessed games come in the order of the players
        // (each one guesses for the next, the last for the first)
        protected async Task SeedEvent(string eventId, EventStatusId status, string[] players, string[]? games = null, DateTime? start = null,
            bool allowsNewcomers = false, string guildId = G1, string? creator = null)
        {
            var isShuffled = status != EventStatusId.PlayersRegistration;
            var rows = new List<object> { Make.Event(eventId, guildId, creator ?? players[0], status, start, allowsNewcomers) };
            rows.AddRange(players.Select((player, index) => Make.Record($"{eventId}-r{index}", eventId, player,
                isShuffled && status != EventStatusId.PlayersShuffle ? players[(index + 1) % players.Length] : null,
                games?.ElementAtOrDefault(index))));
            await Seed(rows.ToArray());
        }

        protected async Task<string> CreateEvent(string name = "Event", bool? isNextSeason = false, bool allowsNewcomers = false, string creator = Sliva,
            string guildId = G1)
        {
            object request = isNextSeason == null
                ? new { eventName = name, guildId, allowsNewcomers }
                : new { eventName = name, guildId, allowsNewcomers, isNextSeason };
            return (await As(creator).Post("/api/postNewEvent", request)).Ok().Str("eventId")!;
        }

        protected async Task<System.Text.Json.Nodes.JsonNode> GetEvent(string eventId, string viewer = Sliva) =>
            (await As(viewer).Get($"/api/getEventById?eventId={eventId}")).Ok();
    }
}
