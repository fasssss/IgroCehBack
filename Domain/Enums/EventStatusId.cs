using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum EventStatusId
    {
        [Display(Name = "Players registration")]
        PlayersRegistration,
        [Display(Name = "Auction")]
        Auction,
        [Display(Name = "Active")]
        Active,
        [Display(Name = "Finished")]
        Finished,
    }
}
