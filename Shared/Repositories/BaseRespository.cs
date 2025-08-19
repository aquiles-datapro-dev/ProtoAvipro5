using Shared.Connector;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Repositories;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Shared.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly CustomDBContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(CustomDBContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T> GetByIdAsync(long id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<bool> ExistsAsync(long id)
        {
            return await _dbSet.FindAsync(id) != null;
        }

        
    }
}