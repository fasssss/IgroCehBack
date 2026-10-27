using Application.Interfaces;
using Domain.Entities;
using Persistence.Context;

namespace Persistence.Repository
{
    public class GlobalEventRepository : BaseRepository<GlobalEvent>, IGlobalEventRepository
    {
        public GlobalEventRepository(IgroCehContext context) : base(context)
        {
        }
    }
}
