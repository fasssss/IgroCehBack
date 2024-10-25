using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class DiscordGuildObject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public string banner { get; set; }
        public bool owner { get; set; }
        public long owner_id { get; set; }
        public string permissions { get; set; }
        public List<string> features { get; set; }
        public int approximate_member_count { get; set; }
        public int approximate_presence_count { get; set; }
    }
}
