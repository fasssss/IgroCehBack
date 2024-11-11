using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    public class GetGuildById: Endpoint<GetGuildByIdRequest, Results<Ok<GetGuildByIdResponse>, BadRequest<string>>>
    {
        private readonly IGuildApplicationService _guildApplicationService;
        public GetGuildById(IGuildApplicationService guildApplicationService) 
        {
            _guildApplicationService = guildApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getGuildById");
        }

        public override async Task<Results<Ok<GetGuildByIdResponse>, BadRequest<string>>> ExecuteAsync(GetGuildByIdRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (stringId != null)
            {
                var guilds = await _guildApplicationService.GetGuildByIdAsync(stringId, request.GuildId);

                return TypedResults.Ok(new GetGuildByIdResponse()
                {
                    GuildObject = guilds,
                });
            }

            return TypedResults.BadRequest("Couldn't retrieve guilds");
        }
    }
}
