using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // profile is open only to players who share a guild with its owner,
    // for others it doesn't exist, so the answer doesn't tell whether the player is registered
    public class GetPlayerProfile : Endpoint<GetPlayerProfileRequest, Results<Ok<GetPlayerProfileResponse>, NotFound>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetPlayerProfile(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPlayerProfile");
        }

        public override async Task<Results<Ok<GetPlayerProfileResponse>, NotFound>> ExecuteAsync(GetPlayerProfileRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var guildId = string.IsNullOrEmpty(request.GuildId) ? null : request.GuildId;
            var playerProfile = await _playerApplicationService.GetPlayerProfileAsync(stringId, request.UserId, guildId);
            if (playerProfile == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GetPlayerProfileResponse { PlayerProfile = playerProfile });
        }
    }
}
