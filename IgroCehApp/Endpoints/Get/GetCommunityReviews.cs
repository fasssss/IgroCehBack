using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // every review the current user can read, from all guilds, the newest first
    public class GetCommunityReviews : Endpoint<GetCommunityReviewsRequest, Ok<GetCommunityReviewsResponse>>
    {
        private readonly IReviewApplicationService _reviewApplicationService;

        public GetCommunityReviews(IReviewApplicationService reviewApplicationService)
        {
            _reviewApplicationService = reviewApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getCommunityReviews");
        }

        public override async Task<Ok<GetCommunityReviewsResponse>> ExecuteAsync(GetCommunityReviewsRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var reviews = await _reviewApplicationService.GetCommunityReviewsAsync(stringId, request.Count);

            return TypedResults.Ok(new GetCommunityReviewsResponse { Reviews = reviews.Reviews, HasMore = reviews.HasMore });
        }
    }
}
