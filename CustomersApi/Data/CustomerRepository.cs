using CustomersApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CustomersApi.Data
{
    public class CustomerRepository : IRepository<Customer>
    {
        private readonly CustomerApiContext _db;

        public CustomerRepository(CustomerApiContext context)
        {
            _db = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IAsyncEnumerable<Customer?>> GetAllAsync()
        {
            return  _db.Customers.AsAsyncEnumerable();
        }

        public async Task<Customer?> GetAsync(int id)
        {
            return await _db.Customers.FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Customer?> AddAsync(Customer? entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var newCustomer = await _db.Customers.AddAsync(entity);
            await _db.SaveChangesAsync();
            return newCustomer.Entity;
        }
        
        public async Task EditAsync(Customer entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(o => o.Id == id);
            if (customer == null)
            {
                throw new ArgumentNullException($"Customer with id {id} not found.");
            }

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
        }
    }
}