using Application.DTO;
using Application.Enum;
using Domain.Enums;
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
        public Task<(JoinEventResult Result, EventRecordObject EventRecord)> JoinEventAsync(string userId, string eventId);
        public Task<(RemoveFromEventResult Result, EventRecordObject EventRecord)> RemoveFromEventAsync(string userIdOperator, string userIdToRemove, string eventId);
        public Task<MoveEventToNextStageResult> MoveEventToNextStageAsync(string userId, string eventId, EventStatusId statusId);
        public Task<List<EventRecordObject>> ShuffleUsersAsync(string userId, string eventId);
        public Task<(SuggestGameResult Result, EventRecordObject EventRecord)> SuggestGameAsync(string userId, string eventRecordId, string gameId);
        public Task<ActivateEventResult> ActivateEventAsync(string userId, string eventId);
        public Task<(SubmitPassingResult Result, EventRecordObject EventRecord)> SubmitPassingAsync(string userId, string eventRecordId);
        public Task<EventObject> SummarizeEventAsync(string userId, string eventId);
        public Task<DeleteEventResult> DeleteEventAsync(string userId, string eventId);
    }
}
