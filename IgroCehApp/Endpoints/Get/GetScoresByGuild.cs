using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    public class GetScoresByGuild : Endpoint<GetScoresByGuildRequest, Results<Ok<GetScoresByGuildResponse>, BadRequest<string>>>
    {
        private readonly IGuildApplicationService _guildApplicationService;

        public GetScoresByGuild(IGuildApplicationService guildApplicationService)
        {
            _guildApplicationService = guildApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getScoresByGuild");
        }

        public override async Task<Results<Ok<GetScoresByGuildResponse>, BadRequest<string>>> ExecuteAsync(GetScoresByGuildRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (stringId != null)
            {
                var scores = (List<ScoreObject>)await _guildApplicationService.GetScoreByGuildIdAsync(request.GuildId);
                return TypedResults.Ok(new GetScoresByGuildResponse() { Scores = scores });
            }

            return TypedResults.BadRequest("Couldn't retrieve guilds");
        }
    }
}
