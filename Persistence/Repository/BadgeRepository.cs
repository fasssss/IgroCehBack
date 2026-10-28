using Application.Interfaces;
using Domain.Entities;
using Persistence.Context;

namespace Persistence.Repository
{
    public class BadgeRepository : BaseRepository<Badge>, IBadgeRepository
    {
        public BadgeRepository(IgroCehContext context) : base(context)
        {
        }
    }
}
