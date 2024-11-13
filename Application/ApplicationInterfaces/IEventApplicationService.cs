﻿using Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ApplicationInterfaces
{
    public interface IEventApplicationService
    {
        public Task<EventShortObject> CreateEventAsync(EventShortObject eventObject);
        public Task<List<EventShortObject>> GetEventsByGuildIdAsync(string userId, string guildId, int startFrom);
        public Task<EventObject> GetEventByIdAsync(string userId, string eventId);
        public Task<EventRecordObject> JoinEventAsync(string userId, string eventId);
    }
}
