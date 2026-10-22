using Domain.Entities;
using Domain.Enums;
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
        public bool HasReview { get; set; }
        public bool HasReviewReplies { get; set; }
        // the event page offers the review only to those who see it
        public ReviewVisibility? ReviewVisibility { get; set; }
    }
}
