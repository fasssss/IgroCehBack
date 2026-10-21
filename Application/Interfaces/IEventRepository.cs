using Application.DTO;
using Domain.Entities;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEventRepository: IBaseRepository<Event>
    {
        public Task<EventRecord> AddEventRecordAsync(string userId, string eventId);
        public Task<EventRecord> GetEventRecordAsync(string eventRecordId);
        public Task<List<EventRecord>> GetEventRecordsAsync(string eventId);
        public Task<EventRecord> RemoveEventRecordAsync(string eventRecordId);
        // scores of players in the guilds of the matching events, hidden (deleted) events are left out by the query filter
        public Task<List<RewardScore>> GetRewardScoresAsync(Expression<Func<Event, bool>> eventFilter);
    }
}
