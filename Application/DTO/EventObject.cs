using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class EventObject
    {
        public string Id { get; set; }
        public string EventName { get; set; }
        public string EventCreatorId { get; set; }
        public EventStatusId StatusId { get; set; }
        // participants fill a pool of games for newcomers who join after the start
        public bool AllowsNewcomers { get; set; }
        public List<EventRecordObject> EventRecordObjects { get; set; }
    }
}
