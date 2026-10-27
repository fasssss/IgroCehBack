using API.Authorization;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // an administrator of the site approves a claim, which passes the picked game, or rejects it with an optional reason
    public class DecideGlobalEventClaim : Endpoint<DecideGlobalEventClaimRequest, Results<Ok, NotFound, Conflict<string>>>
    {
        private readonly IGlobalEventClaimApplicationService _claimApplicationService;

        public DecideGlobalEventClaim(IGlobalEventClaimApplicationService claimApplicationService)
        {
            _claimApplicationService = claimApplicationService;
        }

        public override void Configure()
        {
            Post("/api/decideGlobalEventClaim");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok, NotFound, Conflict<string>>> ExecuteAsync(DecideGlobalEventClaimRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            return await _claimApplicationService.DecideAsync(stringId, request.ClaimId, request.IsApproved.Value, request.Comment) switch
            {
                DecideGlobalEventClaimResult.NotFound => TypedResults.NotFound(),
                DecideGlobalEventClaimResult.AlreadyDecided => TypedResults.Conflict("The claim is decided already"),
                _ => TypedResults.Ok(),
            };
        }
    }
}
