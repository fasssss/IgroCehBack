using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IGuildRepository: IBaseRepository<Guild>
    {
        public Task<ICollection<Guild>> GetGuildsByUserIdAsync(string id);
        public Task<ICollection<Guild>> GetFilteredByUserIdAndNameAsync(string id, string searchString);
    }
}
