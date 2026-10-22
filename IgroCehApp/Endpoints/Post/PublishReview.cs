using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the player who passed the game publishes the review, it can't be changed after that
    public class PublishReview : Endpoint<PublishReviewRequest, Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>>>
    {
        private readonly IReviewApplicationService _reviewApplicationService;

        public PublishReview(IReviewApplicationService reviewApplicationService)
        {
            _reviewApplicationService = reviewApplicationService;
        }

        public override void Configure()
        {
            Post("/api/publishReview");
        }

        public override async Task<Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>>> ExecuteAsync(PublishReviewRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _reviewApplicationService.PublishReviewAsync(stringId, request.EventRecordId, request.IsRecommended.Value, request.Visibility.Value, request.Sections);
            switch (result)
            {
                case PublishReviewResult.NotFound:
                    return TypedResults.NotFound();
                case PublishReviewResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case PublishReviewResult.Unavailable:
                    return TypedResults.BadRequest("It can't be published now");
            }

            return TypedResults.Ok();
        }
    }
}
