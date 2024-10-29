using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class EventStatus
    {
        public Enums.EventStatusId Id { get; set; }
        public string Name { get; set; }
        public string UserFriendlyName { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();
    }
}
