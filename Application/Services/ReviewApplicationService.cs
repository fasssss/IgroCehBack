using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System.Linq.Expressions;

namespace Application.Services
{
    public class ReviewApplicationService : IReviewApplicationService
    {
        // the community list is read in growing portions, this keeps a single request bounded
        private const int CommunityReviewsMaxCount = 500;

        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;

        public ReviewApplicationService(IReviewRepository reviewRepository, IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
        }

        // the author picks who sees the review; until it is written the page belongs to the guild of the event
        public async Task<ReviewPageObject> GetReviewPageAsync(string viewerId, string eventRecordId)
        {
            var record = await _reviewRepository.GetEventRecordWithReviewAsync(eventRecordId);
            if (record == null || record.GameId == null || record.ToUserId == null || !await CanSeeAsync(viewerId, record))
            {
                return null;
            }

            return new ReviewPageObject
            {
                EventRecordId = record.Id,
                EventId = record.EventId,
                EventName = record.Event.Name,
                GuildId = record.Event.GuildId,
                Game = new GameObject { Id = record.Game.Id, Name = record.Game.Name, ImageUrl = record.Game.ImageUrl, SteamUrl = record.Game.SteamUrl },
                Author = ToPlayer(record.ToUser),
                Guesser = ToPlayer(record.Participant),
                IsPassed = record.Reward != null,
                Review = record.Review == null ? null : new ReviewObject
                {
                    IsRecommended = record.Review.IsRecommended,
                    Visibility = record.Review.Visibility,
                    PublishedAt = record.Review.CreatedAt,
                    RepliesPublishedAt = record.Review.RepliesPublishedAt,
                    Sections = record.Review.Sections
                        .OrderBy(s => s.Order)
                        .Select(s => new ReviewSectionObject { Id = s.Id, Title = s.Title, Text = s.Text, Reply = s.Reply })
                        .ToList(),
                },
            };
        }

        // the player who passed the game publishes the whole review at once, there are no drafts
        public async Task<PublishReviewResult> PublishReviewAsync(string userId, string eventRecordId, bool isRecommended, ReviewVisibility visibility, List<NewReviewSectionObject> sections)
        {
            var record = await _reviewRepository.GetEventRecordWithReviewAsync(eventRecordId);
            if (record == null)
            {
                return PublishReviewResult.NotFound;
            }

            if (record.ToUserId != userId)
            {
                return PublishReviewResult.Forbidden;
            }

            if (record.Reward == null || record.Review != null)
            {
                return PublishReviewResult.Unavailable;
            }

            var reviewId = Guid.NewGuid().ToString();
            var now = DateTimeOffset.UtcNow;
            await _reviewRepository.AddAsync(new Review
            {
                Id = reviewId,
                EventRecordId = record.Id,
                IsRecommended = isRecommended,
                Visibility = visibility,
                Sections = sections.Select((section, index) => new ReviewSection
                {
                    Id = Guid.NewGuid().ToString(),
                    ReviewId = reviewId,
                    Order = index,
                    Title = section.Title.Trim(),
                    // browsers send line breaks of a form as \r\n
                    Text = section.Text.Trim().Replace("\r\n", "\n"),
                    CreatedAt = now,
                }).ToList(),
            });
            await _reviewRepository.SaveAsync();

            return PublishReviewResult.Published;
        }

        // the one who guessed the game publishes the answers to the sections at once, a section may stay without an answer
        public async Task<PublishReviewResult> PublishRepliesAsync(string userId, string eventRecordId, List<ReviewReplyObject> replies)
        {
            var record = await _reviewRepository.GetEventRecordWithReviewAsync(eventRecordId);
            if (record == null)
            {
                return PublishReviewResult.NotFound;
            }

            if (record.ParticipantId != userId)
            {
                return PublishReviewResult.Forbidden;
            }

            var answered = replies.Where(reply => !string.IsNullOrWhiteSpace(reply.Text)).ToList();
            var review = record.Review;
            // a review the author keeps to themselves isn't shown to the one who guessed the game
            if (review != null && review.Visibility == ReviewVisibility.OnlyAuthor)
            {
                return PublishReviewResult.Forbidden;
            }

            if (review == null || review.RepliesPublishedAt != null || answered.Count == 0
                || answered.Any(reply => review.Sections.All(section => section.Id != reply.SectionId)))
            {
                return PublishReviewResult.Unavailable;
            }

            foreach (var reply in answered)
            {
                review.Sections.Single(section => section.Id == reply.SectionId).Reply = reply.Text.Trim().Replace("\r\n", "\n");
            }
            review.RepliesPublishedAt = DateTimeOffset.UtcNow;
            await _reviewRepository.SaveAsync();

            return PublishReviewResult.Published;
        }

        // reviews the player wrote that the viewer sees, the newest first
        public async Task<List<PlayerReviewObject>> GetPlayerReviewsAsync(string viewerId, string playerId, string guildId)
        {
            var reviews = await _reviewRepository.CustomToListAsync(_reviewRepository
                .Where(IsVisibleTo(viewerId))
                .Where(r => r.EventRecord.ToUserId == playerId && (guildId == null || r.EventRecord.Event.GuildId == guildId))
                .OrderByDescending(r => r.CreatedAt)
                .ThenBy(r => r.Id)
                .Select(r => new PlayerReviewObject
                {
                    EventRecordId = r.EventRecordId,
                    EventName = r.EventRecord.Event.Name,
                    GuildName = r.EventRecord.Event.Guild.Name,
                    GuildIconUrl = r.EventRecord.Event.Guild.AvatarUrl,
                    Game = new GameObject { Id = r.EventRecord.Game.Id, Name = r.EventRecord.Game.Name, ImageUrl = r.EventRecord.Game.ImageUrl },
                    Guesser = new PlayerShortObject { Id = r.EventRecord.Participant.Id, UserName = r.EventRecord.Participant.UserName, AvatarUrl = r.EventRecord.Participant.AvatarUrl },
                    IsRecommended = r.IsRecommended,
                    Visibility = r.Visibility,
                    PublishedAt = r.CreatedAt,
                    HasReplies = r.RepliesPublishedAt != null,
                }));

            return reviews.ToList();
        }

        // what the player can do on the reviews page: review the passed games and answer the reviews of the guessed ones, the latest first
        public async Task<ReviewSuggestionsObject> GetReviewSuggestionsAsync(string userId)
        {
            var toWrite = await _reviewRepository.CustomToListAsync(_reviewRepository
                .WhereEventRecords(er => er.ToUserId == userId && er.Reward != null && er.GameId != null && er.Review == null)
                .OrderByDescending(er => er.SucceededAt)
                .ThenBy(er => er.Id)
                .Select(er => new ReviewSuggestionObject
                {
                    EventRecordId = er.Id,
                    EventName = er.Event.Name,
                    Game = new GameObject { Id = er.Game.Id, Name = er.Game.Name, ImageUrl = er.Game.ImageUrl },
                }));
            // a review kept to its author can't be answered
            var toAnswer = await _reviewRepository.CustomToListAsync(_reviewRepository
                .Where(r => r.EventRecord.ParticipantId == userId && r.RepliesPublishedAt == null && r.Visibility != ReviewVisibility.OnlyAuthor)
                .OrderByDescending(r => r.CreatedAt)
                .ThenBy(r => r.Id)
                .Select(r => new ReviewSuggestionObject
                {
                    EventRecordId = r.EventRecordId,
                    EventName = r.EventRecord.Event.Name,
                    Game = new GameObject { Id = r.EventRecord.Game.Id, Name = r.EventRecord.Game.Name, ImageUrl = r.EventRecord.Game.ImageUrl },
                }));

            return new ReviewSuggestionsObject { ToWrite = toWrite.ToList(), ToAnswer = toAnswer.ToList() };
        }

        // every review the viewer can read, the newest first; the list grows by asking for more of them,
        // so a newly published review shows up at the top instead of shifting the pages
        public async Task<CommunityReviewsObject> GetCommunityReviewsAsync(string viewerId, int count)
        {
            var take = Math.Clamp(count, 1, CommunityReviewsMaxCount);
            // one more review is taken to know whether there are more of them
            var reviews = (await _reviewRepository.CustomToListAsync(_reviewRepository
                .Where(IsVisibleTo(viewerId))
                .OrderByDescending(r => r.CreatedAt)
                .ThenBy(r => r.Id)
                .Take(take + 1)
                .Select(r => new CommunityReviewObject
                {
                    EventRecordId = r.EventRecordId,
                    Game = new GameObject { Id = r.EventRecord.Game.Id, Name = r.EventRecord.Game.Name, ImageUrl = r.EventRecord.Game.ImageUrl },
                    Author = new PlayerShortObject { Id = r.EventRecord.ToUser.Id, UserName = r.EventRecord.ToUser.UserName, AvatarUrl = r.EventRecord.ToUser.AvatarUrl },
                    IsRecommended = r.IsRecommended,
                    PublishedAt = r.CreatedAt,
                })))
                .ToList();

            return new CommunityReviewsObject { Reviews = reviews.Take(take).ToList(), HasMore = reviews.Count > take };
        }

        // the author changes who sees the review at any time
        public async Task<SetReviewVisibilityResult> SetVisibilityAsync(string userId, string eventRecordId, ReviewVisibility visibility)
        {
            var record = await _reviewRepository.GetEventRecordWithReviewAsync(eventRecordId);
            if (record?.Review == null)
            {
                return SetReviewVisibilityResult.NotFound;
            }

            if (record.ToUserId != userId)
            {
                return SetReviewVisibilityResult.Forbidden;
            }

            record.Review.Visibility = visibility;
            await _reviewRepository.SaveAsync();

            return SetReviewVisibilityResult.Changed;
        }

        // the rule of CanSeeAsync for the lists of reviews
        private static Expression<Func<Review, bool>> IsVisibleTo(string viewerId) => r =>
            r.EventRecord.ToUserId == viewerId
            || r.Visibility == ReviewVisibility.Everyone
            || (r.Visibility >= ReviewVisibility.AuthorAndGuesser && r.EventRecord.ParticipantId == viewerId)
            || (r.Visibility >= ReviewVisibility.Guild && r.EventRecord.Event.Guild.UserGuilds.Any(ug => ug.UserId == viewerId));

        private async Task<bool> CanSeeAsync(string viewerId, EventRecord record)
        {
            var visibility = record.Review?.Visibility ?? ReviewVisibility.Guild;
            return record.ToUserId == viewerId
                || visibility == ReviewVisibility.Everyone
                || (visibility >= ReviewVisibility.AuthorAndGuesser && record.ParticipantId == viewerId)
                || (visibility >= ReviewVisibility.Guild && await IsGuildMemberAsync(viewerId, record.Event.GuildId));
        }

        private async Task<bool> IsGuildMemberAsync(string userId, string guildId)
        {
            return await _userRepository.CustomAnyAsync(_userRepository.Where(u => u.Id == userId && u.UserGuilds.Any(ug => ug.GuildId == guildId)));
        }

        private static PlayerShortObject ToPlayer(User user) => new() { Id = user.Id, UserName = user.UserName, AvatarUrl = user.AvatarUrl };
    }
}
