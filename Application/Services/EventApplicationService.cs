using Application.ApplicationInterfaces;
using Application.Consts;
using Application.DTO;
using Application.Enum;
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
        // nobody to guess a game for with fewer players
        private const int MinPlayersCount = 2;
        // the stages the creator moves the event through by hand; activation and summarizing have their own endpoints
        private static readonly Dictionary<EventStatusId, EventStatusId> ManualNextStages = new()
        {
            [EventStatusId.PlayersRegistration] = EventStatusId.PlayersShuffle,
            [EventStatusId.PlayersShuffle] = EventStatusId.GamesGuessing,
        };
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly INewcomerPoolRepository _newcomerPoolRepository;
        private readonly IMapper _mapper;

        public EventApplicationService(IEventRepository eventRepository, IUserRepository userRepository, IGameRepository gameRepository, INewcomerPoolRepository newcomerPoolRepository, IMapper mapper)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _newcomerPoolRepository = newcomerPoolRepository;
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
                    StatusId = EventStatusId.PlayersRegistration,
                    AllowsNewcomers = eventObject.AllowsNewcomers,
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
                    AllowsNewcomers = e.AllowsNewcomers,
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
                        NewcomerPoolGameId = er.NewcomerPoolGameId,
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
                    // Status.Order puts active events first, then the ones at earlier stages, finished ones last
                    _eventRepository.Where(e => e.GuildId == guildId)
                    .OrderBy(e => e.Status.Order)
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
                        AllowsNewcomers = e.AllowsNewcomers,
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
                        // a newcomer takes part as the one who got a game of the pool, its author is in the event anyway
                        ParticipantsIds = eventEntity.EventRecords.Select(x => x.NewcomerPoolGameId == null ? x.ParticipantId : x.ToUserId).Distinct().ToList(),
                        AllowsNewcomers = eventEntity.AllowsNewcomers,
                    });
                }

                return eventListObject;
            }

            return new List<EventShortObject>();
        }

        // a member of the guild joins the event once, while it gathers players; a player who left the guild doesn't
        public async Task<(JoinEventResult Result, EventRecordObject EventRecord)> JoinEventAsync(string userId, string eventId)
        {
            var eventEntity = await _eventRepository.FirstOrDefaultAsync(e => e.Id == eventId);
            if (eventEntity == null)
            {
                return (JoinEventResult.NotFound, null);
            }

            var isMember = await _userRepository.CustomAnyAsync(_userRepository
                .Where(u => u.Id == userId && u.UserGuilds.Any(ug => ug.GuildId == eventEntity.GuildId && !ug.IsDeleted)));
            if (!isMember)
            {
                return (JoinEventResult.Forbidden, null);
            }

            if (eventEntity.StatusId != EventStatusId.PlayersRegistration)
            {
                return (JoinEventResult.Unavailable, null);
            }

            if (await _eventRepository.CustomAnyAsync(_eventRepository.Where(e => e.Id == eventId && e.EventRecords.Any(er => er.ParticipantId == userId))))
            {
                return (JoinEventResult.AlreadyJoined, null);
            }

            var addedEventRecord = await _eventRepository.AddEventRecordAsync(userId, eventId);
            await _eventRepository.SaveAsync();

            return (JoinEventResult.Joined, _mapper.Map<EventRecordObject>(await _eventRepository.GetEventRecordAsync(addedEventRecord.Id)));
        }

        // a player leaves by themselves or is removed by the creator, only during the registration: later it would break the chain of guessed games;
        // the games the player put into the pool for newcomers leave with them
        public async Task<(RemoveFromEventResult Result, EventRecordObject EventRecord)> RemoveFromEventAsync(string userIdOperator, string userIdToRemove, string eventId)
        {
            var eventEntity = await _eventRepository.FirstOrDefaultAsync(e => e.Id == eventId);
            var eventRecord = eventEntity == null ? null : (await _eventRepository.GetEventRecordsAsync(eventId)).FirstOrDefault(er => er.ParticipantId == userIdToRemove);
            if (eventRecord == null)
            {
                return (RemoveFromEventResult.NotFound, null);
            }

            if (eventEntity.CreatorId != userIdOperator && userIdOperator != userIdToRemove)
            {
                return (RemoveFromEventResult.Forbidden, null);
            }

            if (eventEntity.StatusId != EventStatusId.PlayersRegistration)
            {
                return (RemoveFromEventResult.Unavailable, null);
            }

            _newcomerPoolRepository.RemoveRange(await _newcomerPoolRepository.CustomToListAsync(_newcomerPoolRepository
                .Where(pg => pg.EventId == eventId && pg.AuthorId == userIdToRemove)));
            var removedEventRecord = await _eventRepository.RemoveEventRecordAsync(eventRecord.Id);
            await _eventRepository.SaveAsync();

            return (RemoveFromEventResult.Removed, _mapper.Map<EventRecordObject>(removedEventRecord));
        }

        // the creator moves the event one stage forward; the requested stage has to be the next one, so a repeated request changes nothing
        public async Task<MoveEventToNextStageResult> MoveEventToNextStageAsync(string userId, string eventId, EventStatusId statusId)
        {
            var eventEntity = await _eventRepository.FirstOrDefaultAsync(e => e.Id == eventId);
            if (eventEntity == null)
            {
                return MoveEventToNextStageResult.NotFound;
            }

            if (eventEntity.CreatorId != userId)
            {
                return MoveEventToNextStageResult.Forbidden;
            }

            if (!ManualNextStages.TryGetValue(eventEntity.StatusId, out var nextStage) || nextStage != statusId)
            {
                return MoveEventToNextStageResult.Unavailable;
            }

            var records = await _eventRepository.GetEventRecordsAsync(eventId);
            // the shuffle needs someone to guess a game for and, if newcomers will join, a pool to roll from;
            // games are guessed for the players the shuffle gave
            var isReady = nextStage == EventStatusId.PlayersShuffle
                ? records.Count >= MinPlayersCount
                    && (!eventEntity.AllowsNewcomers || await _newcomerPoolRepository.CountAsync(eventId) >= NewcomerPoolLimits.MinGames)
                : records.All(er => er.ToUserId != null);
            if (!isReady)
            {
                return MoveEventToNextStageResult.Unavailable;
            }

            eventEntity.StatusId = nextStage;
            await _eventRepository.SaveAsync();

            return MoveEventToNextStageResult.Moved;
        }

        // the players are shuffled only on their own stage, later it would break the guessed games
        public async Task<List<EventRecordObject>> ShuffleUsersAsync(string userId, string eventId)
        {
            var userIsCreator = await _eventRepository.CustomAnyAsync(_eventRepository
                .Where(e => e.Id == eventId && e.CreatorId == userId && e.StatusId == EventStatusId.PlayersShuffle));
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

        // the participant guesses a game for the player they were shuffled to and can change it until the creator activates the event
        public async Task<(SuggestGameResult Result, EventRecordObject EventRecord)> SuggestGameAsync(string userId, string eventRecordId, string gameId)
        {
            var eventRecord = await _eventRepository.GetEventRecordAsync(eventRecordId);
            var suggestedGame = await _gameRepository.GetByIdAsync(gameId);
            if (eventRecord == null || suggestedGame == null)
            {
                return (SuggestGameResult.NotFound, null);
            }

            if (eventRecord.ParticipantId != userId)
            {
                return (SuggestGameResult.Forbidden, null);
            }

            if (eventRecord.Event.StatusId != EventStatusId.GamesGuessing)
            {
                return (SuggestGameResult.Unavailable, null);
            }

            // a player never gets the same game twice; games of deleted events don't count
            var isAlreadyReceived = eventRecord.ToUserId != null && await _eventRepository.CustomAnyAsync(_eventRepository
                .Where(e => e.EventRecords.Any(er => er.ToUserId == eventRecord.ToUserId && er.GameId == gameId && er.Id != eventRecord.Id)));
            if (isAlreadyReceived)
            {
                return (SuggestGameResult.AlreadyReceived, null);
            }

            eventRecord.GameId = gameId;
            eventRecord.Game = suggestedGame;
            await _eventRepository.SaveAsync();

            return (SuggestGameResult.Suggested, _mapper.Map<EventRecordObject>(eventRecord));
        }

        // the creator starts the event once every participant has guessed a game, after that the games can't be changed
        public async Task<ActivateEventResult> ActivateEventAsync(string userId, string eventId)
        {
            var eventEntity = await _eventRepository.FirstOrDefaultAsync(e => e.Id == eventId);
            if (eventEntity == null)
            {
                return ActivateEventResult.NotFound;
            }

            if (eventEntity.CreatorId != userId)
            {
                return ActivateEventResult.Forbidden;
            }

            var eventRecords = await _eventRepository.GetEventRecordsAsync(eventId);
            if (eventEntity.StatusId != EventStatusId.GamesGuessing || eventRecords.Count == 0 || eventRecords.Any(er => er.GameId == null))
            {
                return ActivateEventResult.Unavailable;
            }

            eventEntity.StatusId = EventStatusId.Active;
            await _eventRepository.SaveAsync();

            return ActivateEventResult.Activated;
        }

        // the one who guessed the game confirms that it is passed, an admin of the guild can do it for any game, their own too;
        // once and while the event is active
        public async Task<(SubmitPassingResult Result, EventRecordObject EventRecord)> SubmitPassingAsync(string userId, string eventRecordId)
        {
            var eventRecord = await _eventRepository.GetEventRecordAsync(eventRecordId);
            if (eventRecord == null)
            {
                return (SubmitPassingResult.NotFound, null);
            }

            if (eventRecord.ParticipantId != userId && !await IsGuildAdminAsync(userId, eventRecord.Event.GuildId))
            {
                return (SubmitPassingResult.Forbidden, null);
            }

            if (eventRecord.Event.StatusId != EventStatusId.Active || eventRecord.GameId == null || eventRecord.SucceededAt != null)
            {
                return (SubmitPassingResult.Unavailable, null);
            }

            // months are counted from the start of the season the event belongs to (Event.StartDate)
            var countFrom = new DateTimeOffset(eventRecord.Event.StartDate, TimeSpan.Zero);
            eventRecord.SucceededAt = DateTimeOffset.UtcNow;
            eventRecord.Reward = CalculatePassingReward(countFrom, eventRecord.SucceededAt.Value);

            await _eventRepository.SaveAsync();

            return (SubmitPassingResult.Submitted, _mapper.Map<EventRecordObject>(eventRecord));
        }

        public async Task<EventObject> SummarizeEventAsync(string userId, string eventId)
        {
            var hasRights = await _eventRepository
                .CustomAnyAsync(_eventRepository
                .Where(e => e.Id == eventId && (e.CreatorId == userId || e.Guild.UserGuilds.Any(ug => ug.UserId == userId && ug.IsAdmin && !ug.IsDeleted))));
            // rewards are already in the scores since their passing, summarizing only finishes the event
            if (hasRights)
            {
                var completedEvent = await _eventRepository.GetByIdAsync(eventId);
                completedEvent.StatusId = EventStatusId.Finished;
                await _eventRepository.SaveAsync();
            }

            return new EventObject();
        }

        // soft delete: the event with its records is only hidden, so it can be restored by unsetting IsDeleted;
        // scores are summed from the visible records, so its rewards leave them and come back with it
        public async Task<DeleteEventResult> DeleteEventAsync(string userId, string eventId)
        {
            var eventEntity = await _eventRepository.FirstOrDefaultAsync(e => e.Id == eventId);
            if (eventEntity == null)
            {
                return DeleteEventResult.NotFound;
            }

            if (!await IsGuildAdminAsync(userId, eventEntity.GuildId))
            {
                return DeleteEventResult.Forbidden;
            }

            eventEntity.IsDeleted = true;
            eventEntity.DeletedAt = DateTimeOffset.UtcNow;
            eventEntity.DeletedById = userId;
            await _eventRepository.SaveAsync();

            return DeleteEventResult.Deleted;
        }

        // an admin who left the guild has no rights in it
        private async Task<bool> IsGuildAdminAsync(string userId, string guildId)
        {
            return await _userRepository.CustomAnyAsync(_userRepository
                .Where(u => u.Id == userId && u.UserGuilds.Any(ug => ug.GuildId == guildId && ug.IsAdmin && !ug.IsDeleted)));
        }

        // 3 points for passing within the first full month after countFrom, 2 within the second, 1 within the third, 0 later
        public static int CalculatePassingReward(DateTimeOffset countFrom, DateTimeOffset passedAt)
        {
            countFrom = countFrom.ToUniversalTime();
            passedAt = passedAt.ToUniversalTime();

            var fullMonths = (passedAt.Year - countFrom.Year) * 12 + passedAt.Month - countFrom.Month;
            if (countFrom.AddMonths(fullMonths) > passedAt)
            {
                fullMonths--;
            }

            return fullMonths switch
            {
                int month when month <= 0 => 3,
                1 => 2,
                2 => 1,
                _ => 0
            };
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
