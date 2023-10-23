

namespace CustomersApi.Data;

    public interface IRepository<T>
    {
        Task<IAsyncEnumerable<T?>> GetAllAsync();
        Task<T?> GetAsync(int id);
        Task<T?> AddAsync(T entity);
        Task EditAsync(T entity);
        Task RemoveAsync(int id);
    }

