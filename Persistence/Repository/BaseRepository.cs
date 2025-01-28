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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            entityModel.CreatedAt = DateTimeOffset.UtcNow;
            return (await _context.AddAsync(entityModel)).Entity;
        }

        public async Task<T> TryAddAsync(T entityModel)
        {
            entityModel.CreatedAt = DateTimeOffset.UtcNow;
            if (entityModel.Id != null && entityModel.Id != "")
            {
                return (await _context.FindAsync<T>(entityModel.Id)) ?? (await _context.AddAsync(entityModel)).Entity;
            }

            return (await _context.AddAsync(entityModel)).Entity;
        }

        public async Task<T> AddOrUpdateAsync(T entityModel)
        {
            if (entityModel.Id != null && entityModel.Id != "")
            {
                var existingEntity = await _context.FindAsync<T>(entityModel.Id);
                if (existingEntity != null)
                {
                    _context.Remove(existingEntity);
                }
                else
                {
                    entityModel.CreatedAt = DateTimeOffset.UtcNow;
                }
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

        public IQueryable<T> Where(Expression<Func<T, bool>> expression, bool eagerLoading = false)
        {
            var query = _context.Set<T>().Where(expression);

            var navigations = _context.Model.FindEntityType(typeof(T))
            .GetDerivedTypesInclusive()
            .SelectMany(type => type.GetNavigations())
            .Distinct();

            foreach (var property in navigations)
                query = query.Include(property.Name);

            return query;
        }

        public IQueryable<T> OrderBy(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().OrderBy(expression);
        }

        public IQueryable<T> OrderByCreationDate()
        {
            return _context.Set<T>().OrderBy(x => x.CreatedAt);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<ICollection<T2>> CustomToListAsync<T2>(IQueryable<T2> query) where T2: class
        {
            return await query.ToListAsync();
        }

        public async Task<bool> CustomAnyAsync(IQueryable<T> query)
        {
            return await query.AnyAsync();
        }
    }
}
