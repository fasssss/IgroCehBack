using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repository
{
    public class EventRepository: BaseRepository<Event>, IEventRepository
    {
        private readonly IgroCehContext _igroCehContext;

        public EventRepository(IgroCehContext igroCehContext): base(igroCehContext)
        {
            _igroCehContext = igroCehContext;
        }

        public async Task<EventRecord> AddEventRecordAsync(string userId, string eventId)
        {
            var addedRecordWithParticipant = _igroCehContext.EventRecords.Add(new EventRecord()
            {
                ParticipantId = userId,
                EventId = eventId
            });


            return addedRecordWithParticipant.Entity;
        }

        public async Task<EventRecord> GetEventRecordAsync(string eventRecordId)
        {
            var eventRecordEmpty = await _igroCehContext.EventRecords.FindAsync(eventRecordId);
            if(eventRecordEmpty != null)
            {
                var participant = await _igroCehContext.Users.FindAsync(eventRecordEmpty.ParticipantId);
                var toUser =  await _igroCehContext.Users.FindAsync(eventRecordEmpty.ToUserId);
                var game = await _igroCehContext.Games.FindAsync(eventRecordEmpty.GameId);
                var itsEvent = await _igroCehContext.Events.FindAsync(eventRecordEmpty.EventId);
                eventRecordEmpty.ToUser = toUser ?? new User();
                eventRecordEmpty.Participant = participant ?? new User();
                eventRecordEmpty.Game = game ?? new Game();
                eventRecordEmpty.Event = itsEvent ?? new Event();

                return eventRecordEmpty;
            }

            return null;
        }

        public async Task<List<EventRecord>> GetEventRecordsAsync(string eventId)
        {
            var eventRecords = await _igroCehContext.EventRecords
                .Where(er => er.EventId == eventId)
                .Include(er => er.Participant)
                .Include(er => er.ToUser)
                .Include(er => er.Game)
                .ToListAsync();

            return eventRecords;
        }

        public async Task<EventRecord> RemoveEventRecordAsync(string eventRecordId)
        {
            var eventRecord = await _igroCehContext.EventRecords.FindAsync(eventRecordId);
            if(eventRecord != null)
            {
                 _igroCehContext.EventRecords.Remove(eventRecord);

                return eventRecord;
            }

            return null;
        }
    }
}
