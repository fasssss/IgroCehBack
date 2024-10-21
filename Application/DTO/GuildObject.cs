using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class GuildObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Banner { get; set; }
        public bool Owner { get; set; }
        public string Permissions { get; set; }
        public List<string> Features { get; set; }
        public int ApproximateMemberCount { get; set; }
        public int ApproximatePresenceCount { get; set; }
    }
}
