using API.Authorization;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the administrator of the site deletes a global event, it is only hidden and can be restored in the database
    public class DeleteGlobalEvent : Endpoint<GlobalEventRequest, Results<Ok, NotFound>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public DeleteGlobalEvent(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Post("/api/deleteGlobalEvent");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok, NotFound>> ExecuteAsync(GlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (await _globalEventApplicationService.DeleteAsync(stringId, request.GlobalEventId) == ChangeGlobalEventResult.NotFound)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok();
        }
    }
}
