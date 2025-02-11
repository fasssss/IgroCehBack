﻿using Domain.Entities;
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
        public Task<T> TryAddAsync(T entityModel);
        public Task<T> AddOrUpdateAsync(T entityModel);

        public T Update(T entityModel);

        public T Delete(T entityModel);

        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);

        public IQueryable<T> Where(Expression<Func<T, bool>> expression, bool eagerLoading = false);
        public IQueryable<T> OrderBy(Expression<Func<T, bool>> expression);
        public IQueryable<T> OrderByCreationDate();

        public Task<int> SaveAsync();

        public Task<ICollection<T2>> CustomToListAsync<T2>(IQueryable<T2> query) where T2: class;
        public Task<bool> CustomAnyAsync(IQueryable<T> query);
    }
}
