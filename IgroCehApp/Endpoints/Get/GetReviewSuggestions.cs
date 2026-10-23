using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // passed games the current user can review and guessed games whose reviews they can answer
    public class GetReviewSuggestions : EndpointWithoutRequest<Ok<GetReviewSuggestionsResponse>>
    {
        private readonly IReviewApplicationService _reviewApplicationService;

        public GetReviewSuggestions(IReviewApplicationService reviewApplicationService)
        {
            _reviewApplicationService = reviewApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getReviewSuggestions");
        }

        public override async Task<Ok<GetReviewSuggestionsResponse>> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var suggestions = await _reviewApplicationService.GetReviewSuggestionsAsync(stringId);

            return TypedResults.Ok(new GetReviewSuggestionsResponse { ToWrite = suggestions.ToWrite, ToAnswer = suggestions.ToAnswer });
        }
    }
}
