using System.Linq.Expressions;

namespace TrackingCar.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        //IQueryable<T> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        Task<T?> GetAsync(Expression<Func<T, bool>>? filter = null, bool tracked = true, string? includeProperties = null);
        Task CreateAsync(T entiry);
        Task RemoveAsync(T entiry);
        Task SoftRemoveAsync(T entity);
        Task<List<T>> GetRemovedAsync(Expression<Func<T, bool>>? filter = null);
        Task UpdateAsync(T entity);
        Task SaveAsync();
        Task<int> GetCountAsync(Expression<Func<T, bool>>? filter = null);
        Task<List<T>> GetPaginatedAsync(int? pageNumber = null, int? pageSize = null, Expression<Func<T, bool>>? filter = null);


    }
}
