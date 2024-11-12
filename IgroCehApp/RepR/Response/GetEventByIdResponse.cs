using Application.DTO;
using Domain.Enums;

namespace API.RepR.Response
{
    public class GetEventByIdResponse
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public EventStatusId StatusId { get; set; }
        public List<EventRecordObject> EventRecords { get; set; }
    }
}
