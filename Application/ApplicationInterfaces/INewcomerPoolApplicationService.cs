using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    public interface INewcomerPoolApplicationService
    {
        // null when the event has no pool or the viewer isn't a member of its guild
        public Task<NewcomerPoolObject> GetPoolAsync(string userId, string eventId);
        public Task<(ChangeNewcomerPoolResult Result, NewcomerPoolGameObject PoolGame)> AddGameAsync(string userId, string eventId, string gameId);
        public Task<(ChangeNewcomerPoolResult Result, NewcomerPoolGameObject PoolGame)> ReplaceGameAsync(string userId, string eventId, string poolGameId, string gameId);
        public Task<ChangeNewcomerPoolResult> RemoveGameAsync(string userId, string eventId, string poolGameId);
        public Task<(NewcomerJoinResult Result, NewcomerRollObject Roll)> GetRollAsync(string userId, string eventId);
        public Task<(NewcomerJoinResult Result, EventRecordObject EventRecord)> JoinAsync(string userId, string eventId, List<string> excludedPoolGameIds);
    }
}
