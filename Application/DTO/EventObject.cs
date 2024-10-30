using Domain.Entities;
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
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public string GuildId { get; set; }
        public EventStatusId StatusId { get; set; }
    }
}
