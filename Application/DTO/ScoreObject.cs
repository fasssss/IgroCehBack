using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class ScoreObject
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public int Score { get; set; }
        public int EventsPlayed { get; set; }
    }
}
