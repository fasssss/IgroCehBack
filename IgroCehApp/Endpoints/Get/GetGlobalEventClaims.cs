using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the claims of a global event for the administrators of the site, the ones under review first
    public class GetGlobalEventClaims : Endpoint<GlobalEventRequest, Results<Ok<GlobalEventClaimsResponse>, NotFound>>
    {
        private readonly IGlobalEventClaimApplicationService _claimApplicationService;

        public GetGlobalEventClaims(IGlobalEventClaimApplicationService claimApplicationService)
        {
            _claimApplicationService = claimApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getGlobalEventClaims");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GlobalEventClaimsResponse>, NotFound>> ExecuteAsync(GlobalEventRequest request, CancellationToken ct)
        {
            var claims = await _claimApplicationService.GetClaimsAsync(request.GlobalEventId);
            return claims == null ? TypedResults.NotFound() : TypedResults.Ok(new GlobalEventClaimsResponse { Claims = claims });
        }
    }
}
