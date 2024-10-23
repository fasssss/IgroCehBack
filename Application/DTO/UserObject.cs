using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class UserObject
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Discriminator { get; set; }
        public string GlobalName { get; set; }
        public string Avatar { get; set; }
        public string AvatarUrl { get; set; }
        public bool? Bot { get; set; }
        public bool? System { get; set; }
        public bool? MfaEnabled { get; set; }
        public string Banner { get; set; }
        public int? AccentColor { get; set; }
        public string Locale { get; set; }
        public bool? Verified { get; set; }
        public string Email { get; set; }
        public int? Flags { get; set; }
        public int? PremiumType { get; set; }
        public int? PublicFlags { get; set; }
    }
}
