using API.Authorization;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the global events that give badges, the administrator picks a badge for an event with them in sight
    public class GetGlobalEventBadgeUses : EndpointWithoutRequest<Ok<GlobalEventBadgeUsesResponse>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetGlobalEventBadgeUses(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getGlobalEventBadgeUses");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Ok<GlobalEventBadgeUsesResponse>> ExecuteAsync(CancellationToken ct)
        {
            return TypedResults.Ok(new GlobalEventBadgeUsesResponse { BadgeUses = await _globalEventApplicationService.GetBadgeUsesAsync() });
        }
    }
}
