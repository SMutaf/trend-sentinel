using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TrendSentinel.Domain.Common;
using TrendSentinel.Domain.Interfaces;
using TrendSentinel.Infrastructure.Persistence;

namespace TrendSentinel.Infrastructure.Repositories
{
    public class EfRepository<T> : IAsyncRepository<T> where T : BaseEntity
    {
        protected readonly TrendSentinelDbContext _dbContext;

        public EfRepository(TrendSentinelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
}