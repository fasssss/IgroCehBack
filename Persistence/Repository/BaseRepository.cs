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

        public async Task<T> GetByIdAsync(long id)
        {
            var entity = await _context.FindAsync<T>(id);
            return entity;
        }

        public async Task<T> AddAsync(T entityModel)
        {
            if (entityModel.Id != 0)
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

        public async Task<T> FirstOrDefaultAsync(Expression<Func<IQueryable<T>>> expression)
        {
            return await _context.FromExpression(expression).FirstOrDefaultAsync();
        }

        public IQueryable<T> Where(Expression<Func<IQueryable<T>>> expression)
        {
            return _context.FromExpression(expression);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
