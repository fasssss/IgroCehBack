using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Post
{
    public class PostNewEvent: Endpoint<PostNewEventRequest, Results<Ok, BadRequest<string>>>
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

        public override async Task<Results<Ok, BadRequest<string>>> ExecuteAsync(PostNewEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.Request.Cookies["id"];
            var newEvent = new EventObject()
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
                    return TypedResults.Ok();
                }
            }

            return TypedResults.BadRequest("Couldn't create new event");
        }
    }
}
