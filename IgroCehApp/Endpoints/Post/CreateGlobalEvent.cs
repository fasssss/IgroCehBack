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
    // the administrator of the site creates a global event from catalog games and their Steam achievements
    public class CreateGlobalEvent : Endpoint<CreateGlobalEventRequest, Results<Ok<GlobalEventResponse>, BadRequest<string>, Conflict<string>, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public CreateGlobalEvent(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Post("/api/createGlobalEvent");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GlobalEventResponse>, BadRequest<string>, Conflict<string>, StatusCodeHttpResult>> ExecuteAsync(CreateGlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var (result, globalEvent) = await _globalEventApplicationService.CreateAsync(stringId, request.Name, request.Description, request.Games, ct);
                switch (result)
                {
                    case CreateGlobalEventResult.AlreadyActive:
                        return TypedResults.Conflict("Another global event is active");
                    case CreateGlobalEventResult.UnknownGame:
                        return TypedResults.BadRequest("A game isn't in the catalog");
                    case CreateGlobalEventResult.UnknownAchievement:
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
