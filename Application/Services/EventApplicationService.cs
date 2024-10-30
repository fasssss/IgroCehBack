using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EventApplicationService: IEventApplicationService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;

        public EventApplicationService(IEventRepository eventRepository, IUserRepository userRepository) 
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
        }

        public async Task<EventObject> CreateEventAsync(EventObject eventObject)
        {
            var userHasRight = await _userRepository
                .CustomAnyAsync(_userRepository
                .Where(u => u.Id == eventObject.CreatorId && u.UserGuilds.Any(ug => ug.GuildId == eventObject.GuildId)));
            if (userHasRight)
            {
                var createdEvent = await _eventRepository.AddAsync(new Event()
                {
                    Name = eventObject.Name,
                    CreatorId = eventObject.CreatorId,
                    GuildId = eventObject.GuildId,
                    //StatusId = EventStatusId.PlayersRegistration
                });

                var saveResult = await _eventRepository.SaveAsync();

                if(createdEvent != null && saveResult > 0)
                {
                    var createdEventObject = new EventObject()
                    {
                        Id = createdEvent.Id,
                        Name= createdEvent.Name,
                        CreatorId = createdEvent.CreatorId,
                        GuildId = createdEvent.GuildId,
                        //StatusId = createdEvent.StatusId
                    };

                    return createdEventObject;
                }
            }

            return null;
        }
    }
}
