using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the administrator of the site changes the name, the description and the games of a global event
    public class UpdateGlobalEvent : Endpoint<UpdateGlobalEventRequest, Results<Ok<GlobalEventResponse>, NotFound, BadRequest<string>, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public UpdateGlobalEvent(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Post("/api/updateGlobalEvent");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GlobalEventResponse>, NotFound, BadRequest<string>, StatusCodeHttpResult>> ExecuteAsync(UpdateGlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var (result, globalEvent) = await _globalEventApplicationService.UpdateAsync(stringId, request.GlobalEventId, request.Name, request.Description,
                    request.Games, ct);
                switch (result)
                {
                    case UpdateGlobalEventResult.NotFound:
                        return TypedResults.NotFound();
                    case UpdateGlobalEventResult.UnknownGame:
                        return TypedResults.BadRequest("A game isn't in the catalog");
                    case UpdateGlobalEventResult.UnknownAchievement:
                        return TypedResults.BadRequest("A game has no such achievement in Steam");
                }

                return TypedResults.Ok(new GlobalEventResponse { GlobalEvent = globalEvent });
            }
            catch (Exception e) when (e is SteamUnavailableException or GameCatalogUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
