using Domain.Attributes;
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
        [Order(3)]
        PlayersRegistration,
        [Display(Name = "Auction")]
        [Order(2)]
        Auction,
        [Display(Name = "Active")]
        [Order(1)]
        Active,
        [Display(Name = "Finished")]
        [Order(4)]
        Finished,
    }
}
