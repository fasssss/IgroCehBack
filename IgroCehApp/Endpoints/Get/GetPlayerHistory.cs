using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // events of the player from the guilds shared with the viewer, newest first
    public class GetPlayerHistory : Endpoint<GetPlayerHistoryRequest, Ok<GetPlayerHistoryResponse>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetPlayerHistory(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPlayerHistory");
        }

        public override async Task<Ok<GetPlayerHistoryResponse>> ExecuteAsync(GetPlayerHistoryRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var guildId = string.IsNullOrEmpty(request.GuildId) ? null : request.GuildId;
            var history = await _playerApplicationService.GetPlayerHistoryAsync(stringId, request.UserId, guildId, request.StartFrom);

            return TypedResults.Ok(new GetPlayerHistoryResponse
            {
                HistoryItems = history.Items,
                HasMore = history.HasMore,
            });
        }
    }
}
