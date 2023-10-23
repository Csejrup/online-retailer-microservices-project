
namespace OrderApi.Data;

    public interface IOrderRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllByCustomerAsync(int customerId);
        Task<T> GetAsync(int id);
        Task<T> AddAsync(T entity);
        Task EditAsync(int id, T entity);
        Task RemoveAsync(int id);
    }
