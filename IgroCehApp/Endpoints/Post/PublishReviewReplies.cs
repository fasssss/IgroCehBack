using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the one who guessed the game publishes the answers to the review, they can't be changed after that
    public class PublishReviewReplies : Endpoint<PublishReviewRepliesRequest, Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>>>
    {
        private readonly IReviewApplicationService _reviewApplicationService;

        public PublishReviewReplies(IReviewApplicationService reviewApplicationService)
        {
            _reviewApplicationService = reviewApplicationService;
        }

        public override void Configure()
        {
            Post("/api/publishReviewReplies");
        }

        public override async Task<Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>>> ExecuteAsync(PublishReviewRepliesRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _reviewApplicationService.PublishRepliesAsync(stringId, request.EventRecordId, request.Replies);
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
