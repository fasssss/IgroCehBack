﻿using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    public class GetEventsList: Endpoint<GetEventsListRequest, Results<Ok<GetEventsListResponse>, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;

        public GetEventsList(IEventApplicationService eventApplicationService)
        {
            _eventApplicationService = eventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getEventsByGuildId");
        }

        public override async Task<Results<Ok<GetEventsListResponse>, BadRequest<string>>> ExecuteAsync(GetEventsListRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;


            if (stringId != null)
            {
                var eventsList = await _eventApplicationService.GetEventsByGuildIdAsync(stringId, request.GuildId, request.startFrom);
                if (eventsList != null)
                {
                    return TypedResults.Ok(new GetEventsListResponse
                    {
                        EventsList = eventsList
                    });
                }
            }

            return TypedResults.BadRequest("Couldn't fetch events for this user and guild");
        }
    }
}
