using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class EventRecordObject
    {
        public string Id { get; set; }
        public UserObject Participant { get; set; }
        public UserObject ToUser { get; set; }
        public GameObject Game { get; set; }
        public int? Reward { get; set; }
    }
}
