using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    public class GetSearchedGuilds: Endpoint<GetSearchedGuildsRequest, Results<Ok<GetSearchedGuildsResponse>, BadRequest<string>>>
    {
        private readonly IGuildApplicationService _guildApplicationService;

        public GetSearchedGuilds(IGuildApplicationService guildApplicationService)
        {
            _guildApplicationService = guildApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getSearchedGuilds");
        }

        public override async Task<Results<Ok<GetSearchedGuildsResponse>, BadRequest<string>>> ExecuteAsync(GetSearchedGuildsRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.Request.Cookies["id"];
            if (long.TryParse(stringId, out long id))
            {
                var guilds = await _guildApplicationService.GetFilteredGuildsAsync(id, new GuildsFilter()
                {
                    SearchString = request.SearchString
                });

                return TypedResults.Ok(new GetSearchedGuildsResponse()
                {
                    GuildObjects = guilds.ToList(),
                });
            }

            return TypedResults.BadRequest("Couldn't retrieve guilds");
        }
    }
}
