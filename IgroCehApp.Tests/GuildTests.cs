using System.Net;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;

namespace IgroCehApp.Tests
{
    // the guilds of a player: the page of a guild, the search among them, the table of its scores and the guilds where the player has played
    public class GuildTests : GuildTest
    {
        public GuildTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Guild_IsGivenToItsMembers_WithTheirRightsInIt()
        {
            var forMember = (await As(Sliva).Get($"/api/getGuildById?guildId={G1}")).Ok().At("guildObject");
            var forAdmin = (await As(Admin).Get($"/api/getGuildById?guildId={G1}")).Ok().At("guildObject");

            Assert.Equal((G1, "Guild One", "https://cdn.test/icons/g1.png"), (forMember.Str("id"), forMember.Str("name"), forMember.Str("iconUrl")));
            Assert.False(forMember.Bool("isCurrentUserAdmin"));
            Assert.True(forAdmin.Bool("isCurrentUserAdmin"));
        }

        [Fact]
        public async Task Guild_OfOthers_IsEmpty()
        {
            var answer = (await As(Stranger).Get($"/api/getGuildById?guildId={G1}")).Ok();

            Assert.Null(answer.At("guildObject"));
        }

        [Fact]
        public async Task Guild_AnAdminWhoLeft_StillSeesIt_WithoutTheRights()
        {
            var answer = (await As(Leaver).Get($"/api/getGuildById?guildId={G1}")).Ok().At("guildObject");

            Assert.Equal(G1, answer.Str("id"));
            Assert.False(answer.Bool("isCurrentUserAdmin"));
        }

        [Fact]
        public async Task Search_AmongTheGuildsOfThePlayer_IgnoringTheCase()
        {
            await Seed(Make.Guild("g3", "Another Guild"), Make.Member(Sliva, "g3"), Make.Guild("g4", "Hidden"), Make.Member(Sliva, "g4", hasLeft: true));

            var guilds = (await As(Sliva).Get("/api/getSearchedGuilds?searchString=GUILD")).Ok();
            var all = (await As(Sliva).Get("/api/getSearchedGuilds?searchString=")).Ok();

            Assert.Equal(new[] { "g1", "g3" }, guilds.Strs("guildObjects", "id").OrderBy(id => id));
            // the guilds the player has left are found too (current behavior)
            Assert.Equal(new[] { "g1", "g3", "g4" }, all.Strs("guildObjects", "id").OrderBy(id => id));
        }

        [Fact]
        public async Task Scores_SumTheRewardsOfThePassedGames_ForEveryMember()
        {
            await SeedEvent("e1", EventStatusId.Finished, new[] { Sliva, Dayt, Nico }, new[] { Elden, Hollow, Sekiro });
            await SeedEvent("e2", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Persona, Elden });
            await Db(async db =>
            {
                // in e1 Sliva guessed for Dayt, Dayt for Nico, Nico for Sliva
                db.EventRecords.Single(er => er.Id == "e1-r0").Reward = 3;
                db.EventRecords.Single(er => er.Id == "e1-r1").Reward = 1;
                db.EventRecords.Single(er => er.Id == "e2-r0").Reward = 2;
                await db.SaveChangesAsync();
            });

            var scores = (await As(Sliva).Get($"/api/getScoresByGuild?guildId={G1}")).Ok().Arr("scores")
                .ToDictionary(s => s.Str("userId")!, s => (s.Int("score"), s.Int("eventsPlayed"), s.Str("userName")));

            Assert.Equal((5, 2, "Dayt"), scores[Dayt]);
            Assert.Equal((1, 1, "Nico"), scores[Nico]);
            Assert.Equal((0, 2, "Sliva"), scores[Sliva]);
            Assert.Equal((0, 0, "Admin"), scores[Admin]);
            // a player who left the guild stays in its table (current behavior)
            Assert.True(scores.ContainsKey(Leaver));
            Assert.False(scores.ContainsKey(Stranger));
        }

        [Fact]
        public async Task Scores_AreGivenToAnyoneSignedIn()
        {
            // the membership isn't checked (current behavior)
            var scores = (await As(Stranger).Get($"/api/getScoresByGuild?guildId={G1}")).Ok().Arr("scores");

            Assert.Equal(5, scores.Count);
        }

        [Fact]
        public async Task PlayedGuilds_TheOneWithTheLatestEventFirst()
        {
            await Seed(Make.Guild("g3", "Third"), Make.Member(Sliva, "g3"), Make.Member(Dayt, "g3"),
                Make.Event("old", G1, Sliva, EventStatusId.Finished, createdAt: DateTimeOffset.UtcNow.AddDays(-30)),
                Make.Record("old-r0", "old", Sliva, Dayt),
                Make.Event("active", G1, Sliva, EventStatusId.Active, createdAt: DateTimeOffset.UtcNow.AddDays(-20)),
                Make.Record("active-r0", "active", Dayt, Sliva),
                Make.Event("newest", "g3", Dayt, EventStatusId.PlayersRegistration, createdAt: DateTimeOffset.UtcNow.AddDays(-1)),
                Make.Record("newest-r0", "newest", Sliva),
                Make.Event("deleted", G1, Sliva, EventStatusId.Active, isDeleted: true), Make.Record("deleted-r0", "deleted", Sliva));

            var guilds = (await As(Sliva).Get("/api/getPlayedGuilds")).Ok().Arr("guilds");

            Assert.Equal(new[] { "g3", "g1" }, guilds.Select(g => g.Str("id")));
            Assert.Equal(("Third", 1, 1), (guilds[0].Str("name"), guilds[0].Int("eventsPlayed"), guilds[0].Int("eventsInProgress")));
            Assert.Equal(("Guild One", 2, 1), (guilds[1].Str("name"), guilds[1].Int("eventsPlayed"), guilds[1].Int("eventsInProgress")));
        }

        [Fact]
        public async Task PlayedGuilds_ANewcomerHasPlayedInTheEventOfTheirGame()
        {
            await Seed(Make.Event("e1", G1, Sliva, EventStatusId.Active, allowsNewcomers: true), Make.Record("e1-r0", "e1", Sliva),
                Make.PoolGame("p1", "e1", Elden, Sliva), Make.Record("e1-new", "e1", Sliva, Nico, Elden, poolGameId: "p1"));

            Assert.Equal(new[] { G1 }, (await As(Nico).Get("/api/getPlayedGuilds")).Ok().Strs("guilds", "id"));
            Assert.Empty((await As(Dayt).Get("/api/getPlayedGuilds")).Ok().Arr("guilds"));
        }
    }
}
