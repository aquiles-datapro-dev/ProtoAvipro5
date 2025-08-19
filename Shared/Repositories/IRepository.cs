using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Shared.Models;

namespace Shared.Repositories
{
   
        public interface IRepository<T> where T : class
        {
            Task<T> GetByIdAsync(long id);
            Task<IEnumerable<T>> GetAllAsync();
            Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
            Task AddAsync(T entity);
            Task UpdateAsync(T entity);
            Task DeleteAsync(long id);
            Task<bool> ExistsAsync(long id);
        }
    
}
