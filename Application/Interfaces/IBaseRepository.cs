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
        public Task<T> GetByIdAsync(long id);

        public Task<T> AddAsync(T entityModel);

        public T Update(T entityModel);

        public T Delete(T entityModel);

        public Task<T> FirstOrDefaultAsync(Expression<Func<IQueryable<T>>> expression);

        public IQueryable<T> Where(Expression<Func<IQueryable<T>>> expression);

        public Task<int> SaveAsync();

    }
}
