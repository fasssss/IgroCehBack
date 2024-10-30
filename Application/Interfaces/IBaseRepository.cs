using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBaseRepository<T> where T: IBaseEntity
    {
        public Task<T> GetByIdAsync(string id);

        public Task<T> AddAsync(T entityModel);

        public T Update(T entityModel);

        public T Delete(T entityModel);

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);

        public IQueryable<T> Where(Expression<Func<T, bool>> expression);

        public Task<int> SaveAsync();

        public Task<ICollection<T>> CustomToListAsync(IQueryable<T> query);
        public Task<bool> CustomAnyAsync(IQueryable<T> query);
    }
}
