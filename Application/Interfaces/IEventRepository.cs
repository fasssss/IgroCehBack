using Domain.Entities;
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
        public Task<EventRecord> RemoveEventRecordAsync(string eventRecordId);
    }
}
