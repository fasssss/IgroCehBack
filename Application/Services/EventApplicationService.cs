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

        public async Task<EventShortObject> CreateEventAsync(EventShortObject eventObject)
        {
            var userHasRight = await _userRepository
                .CustomAnyAsync(_userRepository
                .Where(u => u.Id == eventObject.CreatorId && u.UserGuilds.Any(ug => ug.GuildId == eventObject.GuildId)));
            var userAlreadyHasEvent = await _eventRepository
                .CustomAnyAsync(_eventRepository
                .Where(e => e.CreatorId == eventObject.CreatorId && e.StatusId != EventStatusId.Finished));
            if (userHasRight && !userAlreadyHasEvent)
            {
                var startSeasonDate = GetStartSeasonDate();
                var createdEvent = await _eventRepository.AddAsync(new Event()
                {
                    Name = eventObject.Name,
                    CreatorId = eventObject.CreatorId,
                    StartDate = startSeasonDate,
                    EndDate = startSeasonDate.AddMonths(3),
                    GuildId = eventObject.GuildId,
                    StatusId = EventStatusId.PlayersRegistration
                });

                var saveResult = await _eventRepository.SaveAsync();

                if(createdEvent != null && saveResult > 0)
                {
                    var createdEventObject = new EventShortObject()
                    {
                        Id = createdEvent.Id,
                        Name= createdEvent.Name,
                        CreatorId = createdEvent.CreatorId,
                        GuildId = createdEvent.GuildId,
                        StatusId = createdEvent.StatusId,
                    };

                    return createdEventObject;
                }
            }

            return null;
        }

        public async Task<List<EventShortObject>> GetEventsByGuildIdAsync(string userId, string guildId)
        {
            var userIsInGuild = await _userRepository
                .CustomAnyAsync(_userRepository
                .Where(u => u.Id == userId && u.UserGuilds.Any(ug => ug.GuildId == guildId)));

            if (userIsInGuild)
            {
                var eventList = await _eventRepository.CustomToListAsync(
                    _eventRepository.Where(e => e.GuildId == guildId)
                    .OrderByDescending(e => e.StartDate)
                    .Select(e => new Event
                    {
                        Id = e.Id,
                        Name = e.Name,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        CreatorId = e.CreatorId,
                        Creator = new User
                        {
                            Id = e.CreatorId,
                            UserName = e.Creator.UserName
                        },
                        GuildId = e.GuildId,
                        Guild = new Guild
                        {
                            Id = e.GuildId,
                            Name = e.Guild.Name,
                        },
                        StatusId = e.StatusId,
                        Status = new EventStatus
                        {
                            Id = e.StatusId,
                            UserFriendlyName = e.Status.UserFriendlyName
                        }
                    }));

                var eventListObject = new List<EventShortObject>();

                foreach (var eventEntity in eventList)
                {
                    eventListObject.Add(new EventShortObject
                    {
                        Id = eventEntity.Id,
                        Name = eventEntity.Name,
                        StartDate = eventEntity.StartDate,
                        EndDate = eventEntity.EndDate,
                        CreatorId = eventEntity.CreatorId,
                        CreatorUserName = eventEntity.Creator.UserName,
                        GuildId = eventEntity.GuildId,
                        GuildName = eventEntity.Guild.Name,
                        StatusId = eventEntity.StatusId,
                        StatusDisplayName = eventEntity.Status.UserFriendlyName
                    });
                }

                return eventListObject;
            }

            return new List<EventShortObject>();
        }

        private DateTime GetStartSeasonDate()
        {
            DateTime currentDate = DateTime.UtcNow.Date;
            DateTime startDate;

            if (currentDate.Month >= 11 || currentDate.Month == 1)  //if Now last month of autmn or any of two first months of winter - set "winter season" start date
            {
                startDate = new DateTime(currentDate.Year, 12, 1);
                startDate = startDate.AddDays(-1).Date;
                return startDate;
            }

            if(currentDate.Month >= 8) //if Now at least last month of summer - set "autumn season" start date
            {
                startDate = new DateTime(currentDate.Year, 9, 1);
                startDate = startDate.AddDays(-1).Date;
                return startDate;
            }

            if (currentDate.Month >= 5) //if Now at least last month of spring - set "summer season" start date
            {
                startDate = new DateTime(currentDate.Year, 6, 1);
                startDate = startDate.AddDays(-1).Date;
                return startDate;
            }

            startDate = new DateTime(currentDate.Year, 3, 1);
            startDate = startDate.AddDays(-1).Date;
            return startDate;
        }
    }
}
