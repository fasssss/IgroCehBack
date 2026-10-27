namespace Application.DTO
{
    // profile of a player as seen by another player: totals cover every guild of the player,
    // guilds themselves are listed only when the viewer is in them too
    public class PlayerProfileObject
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public DateTimeOffset? PlayingSince { get; set; }
        // the linked Steam account, its profile is linked from the page
        public string SteamId { get; set; }
        // shared guilds where the player took part in at least one event
        public List<PlayerGuildObject> Guilds { get; set; }
        // guilds where the player took part in events, but the viewer is not a member
        public int OtherGuildsCount { get; set; }
        public PlayerStatisticsObject Statistics { get; set; }
        // platforms of completed games from every guild of the player, most frequent first
        public List<PlatformCountObject> FavoritePlatforms { get; set; }
        // the badges of the passed global events, the latest first; they don't depend on the guilds
        public List<PlayerBadgeObject> Badges { get; set; }
    }

    public class PlayerGuildObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public int Score { get; set; }
        // players with equal score share the place
        public int Place { get; set; }
        public int PlayersCount { get; set; }
        public bool HasLeft { get; set; }
    }

    // statistics of the selected guild or of every guild of the player
    public class PlayerStatisticsObject
    {
        public int Score { get; set; }
        // unfinished events are counted too, as in the guild leaderboard
        public int EventsPlayed { get; set; }
        public int EventsInProgress { get; set; }
        public int GamesCompleted { get; set; }
        // games that were not completed before the end of their event
        public int GamesFailed { get; set; }
        // days from the season start to the passing, the same start the reward is counted from
        public double? AverageDaysToComplete { get; set; }
        public int GamesSuggested { get; set; }
        public int SuggestedGamesCompleted { get; set; }
        public int SuggestedGamesFailed { get; set; }
        public List<RewardCountObject> Rewards { get; set; }
    }

    public class RewardCountObject
    {
        public int Reward { get; set; }
        public int Count { get; set; }
    }

    public class PlatformCountObject
    {
        public string Platform { get; set; }
        public int Count { get; set; }
    }
}
