using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TrendSentinel.Domain.Common;

namespace TrendSentinel.Domain.Interfaces
{
    public interface IAsyncRepository<T> where T : BaseEntity
    {
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task<T> GetByIdAsync(Guid id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}