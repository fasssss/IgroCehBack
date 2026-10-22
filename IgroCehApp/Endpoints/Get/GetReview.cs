using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // the page of a paired review, open to the members of the guild of its event
    public class GetReview : Endpoint<GetReviewRequest, Results<Ok<ReviewPageObject>, NotFound>>
    {
        private readonly IReviewApplicationService _reviewApplicationService;

        public GetReview(IReviewApplicationService reviewApplicationService)
        {
            _reviewApplicationService = reviewApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getReview");
        }

        public override async Task<Results<Ok<ReviewPageObject>, NotFound>> ExecuteAsync(GetReviewRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var page = await _reviewApplicationService.GetReviewPageAsync(stringId, request.EventRecordId);
            if (page == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(page);
        }
    }
}
