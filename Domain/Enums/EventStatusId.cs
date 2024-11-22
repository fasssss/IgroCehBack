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
        [Order(5)]
        PlayersRegistration,
        [Display(Name = "Players shuffle")]
        [Order(4)]
        PlayersShuffle,
        [Display(Name = "Guessing games")]
        [Order(3)]
        GamesGuessing,
        [Display(Name = "Revealing games")]
        [Order(2)]
        RevealingGames,
        [Display(Name = "Active")]
        [Order(1)]
        Active,
        [Display(Name = "Finished")]
        [Order(6)]
        Finished,
    }
}
