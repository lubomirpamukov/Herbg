﻿using System.Linq.Expressions;

namespace Herbg.Infrastructure.Interfaces;

public interface IRepository<T> where T : class
{
    public Task<IEnumerable<T>> GetAllAsync();

    public IQueryable<T> GetAllAttached();

    public Task<T?> FindByIdAsync(object id);

    public Task<bool> AddAsync(T entity);

    public Task<bool> AddRangeAsync(IEnumerable<T> entities);

    public Task<bool> UpdateAsync(T entity);

    public Task<bool> SoftDeleteAsync(T entity);

    public Task<bool> SoftDeleteRangeAsync(IEnumerable<T> entities);

    public Task<bool> DeleteAsync(T entity);

    public Task<bool> DeleteRangeAsync(IEnumerable<T> entities);

    public Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    public Task<int> CountAsync();

    public Task<bool>SaveChangesAsync();
}
