using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    public class GetEventById: Endpoint<GetEventByIdRequest, Results<Ok<GetEventByIdResponse>, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        public GetEventById(IEventApplicationService eventApplicationService)
        {
            _eventApplicationService = eventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getEventById");
        }

        public override async Task<Results<Ok<GetEventByIdResponse>, BadRequest<string>>> ExecuteAsync(GetEventByIdRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var eventObject = await _eventApplicationService.GetEventByIdAsync(stringId, request.EventId);
            var response = new GetEventByIdResponse
            {
                Id = eventObject.Id,
                EventName = eventObject.EventName,
                StatusId = eventObject.StatusId,
                EventRecords = eventObject.EventRecordObjects
            };

            return TypedResults.Ok(response);
        }
    }
}
