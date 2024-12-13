using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO
{
    public class GameObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte[] ImageContent { get; set; }
        public string ImageType { get; set; }
        public string SteamUrl { get; set; }
        public string EventRecordId { get; set; }
        public string CreatorId { get; set; }
    }
}
