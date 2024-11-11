using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class JoinEvent: Endpoint<JoinEventRequest, Results<Ok, BadRequest>>
    {
        private readonly IEventApplicationService _eventApplicationService;

        public JoinEvent(IEventApplicationService eventApplicationService)
        {
            _eventApplicationService = eventApplicationService;
        }

        public override void Configure()
        {
            Post("/api/joinEvent");
        }

        public override async Task<Results<Ok, BadRequest>> ExecuteAsync(JoinEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;

            return TypedResults.Ok();
        }
    }
}
