using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class PostNewEvent: Endpoint<PostNewEventRequest, Results<Ok<PostNewEventResponse>, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;

        public PostNewEvent(IEventApplicationService eventApplicationService)
        {
            _eventApplicationService = eventApplicationService;
        }

        public override void Configure()
        {
            Post("/api/postNewEvent");
        }

        public override async Task<Results<Ok<PostNewEventResponse>, BadRequest<string>>> ExecuteAsync(PostNewEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var newEvent = new EventShortObject()
            {
                GuildId = request.GuildId,
                Name = request.EventName,
                CreatorId = stringId
            };

            if (stringId != null)
            {
                var userObject = await _eventApplicationService.CreateEventAsync(newEvent);
                if(userObject != null)
                {
                    return TypedResults.Ok(new PostNewEventResponse
                    {
                        EventId = userObject.Id,
                    });
                }
            }

            return TypedResults.BadRequest("Couldn't create new event");
        }
    }
}
