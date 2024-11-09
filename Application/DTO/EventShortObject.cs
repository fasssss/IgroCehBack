using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class EventShortObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUserName { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string GuildId { get; set; }
        public string GuildName { get; set; }
        public EventStatusId StatusId { get; set; }
        public string StatusDisplayName { get; set; }
        public List<string> ParticipantsIds { get; set; }
    }
}
