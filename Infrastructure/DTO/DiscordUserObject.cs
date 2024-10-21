using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class DiscordUserObject
    {
        public string id { get; set; }
        public string username { get; set; }
        public string discriminator { get; set; }
        public string global_name { get; set; }
        public string avatar { get; set; }
        public bool? bot{ get; set; }
        public bool? system { get; set; }
        public bool? mfa_enabled { get; set; }
        public string banner { get; set; }
        public int? accent_color { get; set; }
        public string locale { get; set; }
        public bool? verified { get; set; }
        public string email { get; set; }
        public int? flags { get; set; }
        public int? premium_type { get; set; }
        public int? public_flags { get; set; }
        // avatar_decoration_data can be added later (no need at that moment).
    }
}
