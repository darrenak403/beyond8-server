using System.Linq.Expressions;

namespace Beyond8.Common.Data.Interfaces;

public interface IGenericRepository<T> where T : IEntity
{
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> expression);
    Task<T?> GetByIdAsync(Guid id, Expression<Func<T, bool>> expression);
    Task<T?> GetByIdAsync(Guid id);

    Task<T?> FindOneAsync(Expression<Func<T, bool>> expression);
    Task<long> CountAsync(Expression<Func<T, bool>> expression);

    Task<T> AddAsync(T entity);
    Task UpdateAsync(Guid id, T entity);
    Task DeleteAsync(Guid id);

    IQueryable<T> AsQueryable();

    Task<(List<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? includes = null
    );
}
