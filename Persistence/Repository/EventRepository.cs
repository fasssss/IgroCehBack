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
    public class EventRepository: BaseRepository<Event>, IEventRepository
    {
        private readonly IgroCehContext _igroCehContext;

        public EventRepository(IgroCehContext igroCehContext): base(igroCehContext)
        {
            _igroCehContext = igroCehContext;
        }
    }
}
