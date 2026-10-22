using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the author of a review changes who sees it
    public class SetReviewVisibility : Endpoint<SetReviewVisibilityRequest, Results<Ok, NotFound, StatusCodeHttpResult>>
    {
        private readonly IReviewApplicationService _reviewApplicationService;

        public SetReviewVisibility(IReviewApplicationService reviewApplicationService)
        {
            _reviewApplicationService = reviewApplicationService;
        }

        public override void Configure()
        {
            Post("/api/setReviewVisibility");
        }

        public override async Task<Results<Ok, NotFound, StatusCodeHttpResult>> ExecuteAsync(SetReviewVisibilityRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _reviewApplicationService.SetVisibilityAsync(stringId, request.EventRecordId, request.Visibility.Value);
            switch (result)
            {
                case SetReviewVisibilityResult.NotFound:
                    return TypedResults.NotFound();
                case SetReviewVisibilityResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
            }

            return TypedResults.Ok();
        }
    }
}
