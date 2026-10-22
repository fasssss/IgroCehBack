using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // reviews the player wrote, for the profile; only the guilds shared with the viewer, like the history
    public class GetPlayerReviews : Endpoint<GetPlayerReviewsRequest, Ok<GetPlayerReviewsResponse>>
    {
        private readonly IReviewApplicationService _reviewApplicationService;

        public GetPlayerReviews(IReviewApplicationService reviewApplicationService)
        {
            _reviewApplicationService = reviewApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPlayerReviews");
        }

        public override async Task<Ok<GetPlayerReviewsResponse>> ExecuteAsync(GetPlayerReviewsRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var guildId = string.IsNullOrEmpty(request.GuildId) ? null : request.GuildId;
            var reviews = await _reviewApplicationService.GetPlayerReviewsAsync(stringId, request.UserId, guildId);

            return TypedResults.Ok(new GetPlayerReviewsResponse { Reviews = reviews });
        }
    }
}
