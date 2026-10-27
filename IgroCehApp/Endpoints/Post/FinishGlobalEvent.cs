using API.Authorization;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the administrator of the site finishes the active global event, after that a new one can be created
    public class FinishGlobalEvent : Endpoint<GlobalEventRequest, Results<Ok, NotFound, BadRequest<string>>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public FinishGlobalEvent(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Post("/api/finishGlobalEvent");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok, NotFound, BadRequest<string>>> ExecuteAsync(GlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            switch (await _globalEventApplicationService.FinishAsync(stringId, request.GlobalEventId))
            {
                case ChangeGlobalEventResult.NotFound:
                    return TypedResults.NotFound();
                case ChangeGlobalEventResult.Unavailable:
                    return TypedResults.BadRequest("The event is finished already");
            }

            return TypedResults.Ok();
        }
    }
}
