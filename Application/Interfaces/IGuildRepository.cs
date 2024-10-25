using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IGuildRepository: IBaseRepository<Guild>
    {
        public Task<ICollection<Guild>> GetGuildsByUserIdAsync(long id);
        public Task<ICollection<Guild>> GetFilteredByUserIdAndNameAsync(long id, string searchString);
    }
}
