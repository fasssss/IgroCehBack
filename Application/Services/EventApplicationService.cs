using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Interfaces;
using AutoMapper;
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
        private static Random randomGenerator = new Random();
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IMapper _mapper;

        public EventApplicationService(IEventRepository eventRepository, IUserRepository userRepository, IGameRepository gameRepository, IMapper mapper) 
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _mapper = mapper;
        }

        public async Task<EventShortObject> CreateEventAsync(EventShortObject eventObject)
        {
            var userHasRight = await _userRepository
                .CustomAnyAsync(_userRepository
                .Where(u => u.Id == eventObject.CreatorId && u.UserGuilds.Any(ug => ug.GuildId == eventObject.GuildId)));
            var userAlreadyHasEvent = await _eventRepository
                .CustomAnyAsync(_eventRepository
                .Where(e => e.CreatorId == eventObject.CreatorId 
                    && e.StatusId != EventStatusId.Finished 
                    && e.GuildId == eventObject.GuildId));
            if (userHasRight) // TODO use this statement instead "userHasRight && !userAlreadyHasEvent"
            {
                var startSeasonDate = GetStartSeasonDate();
                var createdEvent = await _eventRepository.AddAsync(new Event()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = eventObject.Name,
                    CreatorId = eventObject.CreatorId,
                    StartDate = startSeasonDate,
                    EndDate = startSeasonDate.AddMonths(3),
                    GuildId = eventObject.GuildId,
                    StatusId = EventStatusId.PlayersRegistration
                });

                var addedEventRecord = await _eventRepository.AddEventRecordAsync(eventObject.CreatorId, createdEvent.Id);

                var saveResult = await _eventRepository.SaveAsync();

                if(createdEvent != null && addedEventRecord != null && saveResult >= 2)
                {
                    var createdEventObject = new EventShortObject()
                    {
                        Id = createdEvent.Id,
                        Name= createdEvent.Name,
                        CreatorId = createdEvent.CreatorId,
                        GuildId = createdEvent.GuildId,
                        StatusId = createdEvent.StatusId,
                        ParticipantsIds = new List<string>() { addedEventRecord.ParticipantId }
                    };

                    return createdEventObject;
                }
            }

            return null;
        }

        public async Task<EventObject> GetEventByIdAsync(string userId, string eventId)
        {
            var eventEntity = (await _eventRepository.CustomToListAsync(_eventRepository
                .Where(e => e.Id == eventId && e.Guild.UserGuilds.Any(ug => ug.UserId == userId)).Select(e => new Event
                {
                    Id = e.Id,
                    Name = e.Name,
                    CreatorId = e.CreatorId,
                    GuildId = e.GuildId,
                    StatusId = e.StatusId,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    EventRecords = e.EventRecords.Select(er => new EventRecord
                    {
                        Id = er.Id,
                        Participant = er.Participant,
                        ParticipantId = er.ParticipantId,
                        ToUser = er.ToUser,
                        ToUserId = er.ToUserId,
                        Game = er.Game,
                        GameId = er.GameId,
                        Reward = er.Reward,
                    }).ToList(),
                }))).FirstOrDefault();
            var eventObject = _mapper.Map<EventObject>(eventEntity);

            return eventObject;
        }

        public async Task<List<EventShortObject>> GetEventsByGuildIdAsync(string userId, string guildId, int startFrom)
        {
            var userIsInGuild = await _userRepository
                .CustomAnyAsync(_userRepository
                .Where(u => u.Id == userId && u.UserGuilds.Any(ug => ug.GuildId == guildId)));

            if (userIsInGuild)
            {
                var eventList = await _eventRepository.CustomToListAsync(
                    _eventRepository.Where(e => e.GuildId == guildId)
                    .OrderByDescending(u => u.Status.Order)
                    .ThenByDescending(e => e.StartDate)
                    .ThenBy(e => e.Id)
                    .Skip(startFrom)
                    .Take(10)
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
                        },
                        EventRecords = e.EventRecords,
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
                        StatusDisplayName = eventEntity.Status.UserFriendlyName,
                        ParticipantsIds = eventEntity.EventRecords.Select(x => x.ParticipantId).ToList(),
                    });
                }

                await Task.Delay(1000);
                Task.Delay(1000);
                return eventListObject;
            }

            return new List<EventShortObject>();
        }

        public async Task<EventRecordObject> JoinEventAsync(string userId, string eventId)
        {
            var guildId = (await _eventRepository.GetByIdAsync(eventId)).GuildId;
            var userHasRight = await _userRepository
                .CustomAnyAsync(_userRepository
                .Where(u => u.UserGuilds.Any(ug => ug.GuildId == guildId)));

            if (userHasRight)
            {
                var addedEventRecord = await _eventRepository.AddEventRecordAsync(userId, eventId);
                var saveResult = await _eventRepository.SaveAsync();

                if (saveResult >= 1)
                {
                    var addedEventRecordWithNavProps = await _eventRepository.GetEventRecordAsync(addedEventRecord.Id);
                    return _mapper.Map<EventRecordObject>(addedEventRecord);
                }
            }

            return null;
        }

        public async Task<EventRecordObject> RemoveFromEventAsync(string userIdOperator, string eventRecordId)
        {
            var userHasRight = (await _eventRepository.FirstOrDefaultAsync(e => e.EventRecords.Any(er => er.Id == eventRecordId))).CreatorId == userIdOperator;
            if (userHasRight)
            {
                var removedEventRecord = await _eventRepository.RemoveEventRecordAsync(eventRecordId);
                var removedEventRecordObject = _mapper.Map<EventRecordObject>(removedEventRecord);
                await _eventRepository.SaveAsync();

                return removedEventRecordObject;
            }

            return null;
        }

        public async Task<EventRecordObject> RemoveFromEventAsync(string userIdOperator, string userIdToRemove, string eventId)
        {
            var eventRecord = (await _eventRepository.CustomToListAsync(_eventRepository
                .Where(e => e.Id == eventId)
                .Select(e => e.EventRecords.FirstOrDefault(er => er.ParticipantId == userIdToRemove))))
                .FirstOrDefault();

            var userHasRight = (await _eventRepository.GetByIdAsync(eventId)).CreatorId == userIdOperator || userIdOperator == userIdToRemove;
            if (userHasRight)
            {
                var removedEventRecord = await _eventRepository.RemoveEventRecordAsync(eventRecord.Id);
                var removedEventRecordObject = _mapper.Map<EventRecordObject>(removedEventRecord);
                await _eventRepository.SaveAsync();

                return removedEventRecordObject;
            }

            return null;
        }

        public async Task<int> MoveEventToNextStageAsync(string userId, string eventId, int statusId)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId);
            var userHasRight = eventEntity.CreatorId == userId;
            if (userHasRight)
            {
                eventEntity.StatusId = (EventStatusId)statusId;
                var saveResult = await _eventRepository.SaveAsync();
                if (saveResult >= 0)
                {
                    return (int)eventEntity.StatusId;
                }
            }
            return -1;
        }

        public async Task<List<EventRecordObject>> ShuffleUsersAsync(string userId, string eventId)
        {
            var userIsCreator = await _eventRepository.CustomAnyAsync(_eventRepository.Where(e => e.Id == eventId && e.CreatorId == userId));
            if (userIsCreator)
            {
                var eventRecords = await _eventRepository.GetEventRecordsAsync(eventId);
                int n = eventRecords.Count;
                while (n > 0)
                {
                    n--;
                    int k = randomGenerator.Next(n + 1);
                    var value = eventRecords[k];
                    eventRecords[k] = eventRecords[n];
                    eventRecords[n] = value;
                    if (n != eventRecords.Count - 1)
                    {
                        eventRecords[n].ToUserId = eventRecords[n + 1].ParticipantId;
                        eventRecords[n].ToUser = eventRecords[n + 1].Participant;
                    }

                    if(n == 0)
                    {
                        eventRecords[eventRecords.Count - 1].ToUserId = eventRecords[n].ParticipantId;
                        eventRecords[eventRecords.Count - 1].ToUser = eventRecords[n].Participant;
                    }
                }

                await _eventRepository.SaveAsync();

                return _mapper.Map<List<EventRecordObject>>(eventRecords);
            }

            return new List<EventRecordObject>();
        }

        public async Task<EventRecordObject> SuggestGameAsync(string eventRecordId, string gameId)
        {
            var eventRecord = await _eventRepository.GetEventRecordAsync(eventRecordId);
            var suggestedGame = await _gameRepository.GetByIdAsync(gameId);
            eventRecord.GameId = gameId;
            eventRecord.Game = suggestedGame;
            await _eventRepository.SaveAsync();
            return _mapper.Map<EventRecordObject>(eventRecord);
        }

        public async Task<EventRecordObject> SubmitPassingAsync(string userId, string eventRecordId)
        {
            var eventRecord = await _eventRepository.GetEventRecordAsync(eventRecordId);
            var monthesFromStart = (DateTimeOffset.UtcNow.Month - eventRecord.Event.StartDate.Month) + 
                12 * (DateTimeOffset.UtcNow.Year - eventRecord.Event.StartDate.Year);
            eventRecord.SucceededAt = DateTimeOffset.UtcNow;
            eventRecord.Reward = monthesFromStart switch
            {
                0 => 3,
                1 => 2,
                2 => 1,
                _ => 0
            };

            await _eventRepository.SaveAsync();

            return _mapper.Map<EventRecordObject>(eventRecord);
        }

        public async Task<EventObject> SummarizeEventAsync(string userId, string eventId)
        {
            var hasRights = await _eventRepository
                .CustomAnyAsync(_eventRepository
                .Where(e => e.Id == eventId && (e.CreatorId == userId || e.Guild.UserGuilds.Any(ug => ug.UserId == userId && ug.IsAdmin))));
            if (hasRights)
            {
                var eventRecords = await _eventRepository.CustomToListAsync(
                    _eventRepository.Where(x => x.Id == eventId).SelectMany(x => x.EventRecords));
                var userGuilds = await _userRepository.GetUserGuildsByEventIdAsync(eventId);
                foreach (var eventRecord in eventRecords)
                {
                    var userGuild = userGuilds.FirstOrDefault(x => x.UserId == eventRecord.ToUserId);
                    if(userGuild != null)
                    {
                        userGuild.Score += eventRecord.Reward ?? 0;
                    }
                }

                var completedEvent = await _eventRepository.GetByIdAsync(eventId);
                completedEvent.StatusId = EventStatusId.Finished;
                await _eventRepository.SaveAsync();
            }

            return new EventObject();
        }

        private DateTime GetStartSeasonDate()
        {
            DateTime currentDate = DateTime.UtcNow.Date;
            DateTime startDate;

            if(currentDate.Month == 1) //exceptional if because it has to be winter of previous year
            {
                startDate = new DateTime(currentDate.Year - 1, 12, 1);
                return startDate;
            }

            if (currentDate.Month >= 11)  //if Now last month of autmn or any of two first months of winter - set "winter season" start date
            {
                startDate = new DateTime(currentDate.Year, 12, 1);
                return startDate;
            }

            if(currentDate.Month >= 8) //if Now at least last month of summer - set "autumn season" start date
            {
                startDate = new DateTime(currentDate.Year, 9, 1);
                return startDate;
            }

            if (currentDate.Month >= 5) //if Now at least last month of spring - set "summer season" start date
            {
                startDate = new DateTime(currentDate.Year, 6, 1);
                return startDate;
            }

            startDate = new DateTime(currentDate.Year, 3, 1);
            return startDate;
        }
    }
}
