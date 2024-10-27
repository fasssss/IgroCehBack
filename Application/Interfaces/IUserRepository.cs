using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository: IBaseRepository<User>
    {
        public Task<IEnumerable<Guild>> UpdateUserGuildsAsync(string userId, IEnumerable<Guild> guilds);
    }
}
