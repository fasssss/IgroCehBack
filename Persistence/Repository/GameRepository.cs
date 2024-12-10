using Application.Interfaces;
using Domain.Entities;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repository
{
    public class GameRepository: BaseRepository<Game>, IGameRepository
    {
        private readonly IgroCehContext _context;

        public GameRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }
    }
}
