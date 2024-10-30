using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repository
{
    public abstract class BaseRepository<T>: IBaseRepository<T> where T : class, IBaseEntity
    {
        private IgroCehContext _context;
        public BaseRepository(IgroCehContext context)
        {
            _context = context;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var entity = await _context.FindAsync<T>(id);
            return entity;
        }

        public async Task<T> AddAsync(T entityModel)
        {
            if (entityModel.Id != null && entityModel.Id != "")
            {
                return (await _context.FindAsync<T>(entityModel.Id)) ?? (await _context.AddAsync(entityModel)).Entity;
            }

            return (await _context.AddAsync(entityModel)).Entity;
        }

        public T Update(T entityModel)
        {
            return _context.Update(entityModel).Entity;
        }

        public T Delete(T entityModel)
        {
            return _context.Remove(entityModel).Entity;
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(expression);
        }

        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Where(expression);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<ICollection<T>> CustomToListAsync(IQueryable<T> query)
        {
            return await query.ToListAsync();
        }

        public async Task<bool> CustomAnyAsync(IQueryable<T> query)
        {
            return await query.AnyAsync();
        }
    }
}
