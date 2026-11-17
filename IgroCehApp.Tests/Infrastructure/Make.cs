using Domain.Entities;
using Domain.Enums;

namespace IgroCehApp.Tests.Infrastructure
{
    // rows for arranging a test straight in the database, with the fields the app requires filled in
    public static class Make
    {
        public static User User(string id, string? name = null, bool isAppAdmin = false, DateTimeOffset? createdAt = null) => new()
        {
            Id = id,
            UserName = name ?? id,
            Email = $"{id}@mail.test",
            AvatarUrl = $"https://cdn.test/avatars/{id}.png",
            IsAppAdmin = isAppAdmin,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
        };

        public static Guild Guild(string id, string? name = null) => new()
        {
            Id = id,
            Name = name ?? id,
            AvatarUrl = $"https://cdn.test/icons/{id}.png",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        public static UserGuild Member(string userId, string guildId, bool isAdmin = false, bool hasLeft = false) => new()
        {
            UserId = userId,
            GuildId = guildId,
            IsAdmin = isAdmin,
            IsDeleted = hasLeft,
        };

        public static Game Game(string id, string creatorId, string? name = null, int? vgcId = null, List<string>? platforms = null, string? imageUrl = null) => new()
        {
            Id = id,
            Name = name ?? id,
            CreatorId = creatorId,
            VgcId = vgcId,
            Platforms = platforms ?? new List<string>(),
            ImageUrl = imageUrl,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        public static Event Event(string id, string guildId, string creatorId, EventStatusId status = EventStatusId.PlayersRegistration,
            DateTime? start = null, bool allowsNewcomers = false, string? name = null, DateTimeOffset? createdAt = null, bool isDeleted = false)
        {
            var startDate = start ?? Seasons.CurrentStart();
            return new Event
            {
                Id = id,
                Name = name ?? id,
                GuildId = guildId,
                CreatorId = creatorId,
                StatusId = status,
                StartDate = startDate,
                EndDate = startDate.AddMonths(3),
                AllowsNewcomers = allowsNewcomers,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
                IsDeleted = isDeleted,
            };
        }

        public static EventRecord Record(string id, string eventId, string participantId, string? toUserId = null, string? gameId = null,
            int? reward = null, DateTimeOffset? succeededAt = null, string? poolGameId = null) => new()
        {
            Id = id,
            EventId = eventId,
            ParticipantId = participantId,
            ToUserId = toUserId,
            GameId = gameId,
            Reward = reward,
            SucceededAt = succeededAt,
            NewcomerPoolGameId = poolGameId,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        public static NewcomerPoolGame PoolGame(string id, string eventId, string gameId, string authorId, DateTimeOffset? createdAt = null) => new()
        {
            Id = id,
            EventId = eventId,
            GameId = gameId,
            AuthorId = authorId,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
        };

        public static Review Review(string id, string eventRecordId, bool? isRecommended = true, ReviewVisibility visibility = ReviewVisibility.Guild,
            DateTimeOffset? createdAt = null, DateTimeOffset? repliesPublishedAt = null, params (string Title, string Text, string? Reply)[] sections) => new()
        {
            Id = id,
            EventRecordId = eventRecordId,
            IsRecommended = isRecommended,
            Visibility = visibility,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            RepliesPublishedAt = repliesPublishedAt,
            Sections = (sections.Length == 0 ? new[] { ("Story", "Good", (string?)null) } : sections)
                .Select((section, index) => new ReviewSection
                {
                    Id = $"{id}-s{index}",
                    ReviewId = id,
                    Order = index,
                    Title = section.Item1,
                    Text = section.Item2,
                    Reply = section.Item3,
                    CreatedAt = DateTimeOffset.UtcNow,
                })
                .ToList(),
        };
    }

    // the seasons as the site counts them: the calendar ones, from the 1st of December, March, June and September at 00:00 UTC
    public static class Seasons
    {
        public static DateTime CurrentStart(DateTime? now = null)
        {
            var utcNow = now ?? DateTime.UtcNow;
            var monthsIntoSeason = utcNow.Month % 3;
            return new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).AddMonths(-monthsIntoSeason);
        }

        public static DateTime NextStart(DateTime? now = null) => CurrentStart(now).AddMonths(3);

        // February, May, August and November
        public static bool IsLastMonth(DateTime? now = null) => (now ?? DateTime.UtcNow).Month % 3 == 2;
    }
}
