using API.Authorization;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the badges with the global events that give them, the administrator picks a badge for an event with them in sight
    public class GetBadges : EndpointWithoutRequest<Ok<BadgesResponse>>
    {
        private readonly IBadgeApplicationService _badgeApplicationService;

        public GetBadges(IBadgeApplicationService badgeApplicationService)
        {
            _badgeApplicationService = badgeApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getBadges");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Ok<BadgesResponse>> ExecuteAsync(CancellationToken ct)
        {
            return TypedResults.Ok(new BadgesResponse { Badges = await _badgeApplicationService.GetAllAsync() });
        }
    }
}
