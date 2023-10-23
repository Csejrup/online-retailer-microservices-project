
using Microsoft.EntityFrameworkCore;
using Order = OrderApi.Models.Order;

namespace OrderApi.Data;

    public class OrderOrderRepository : IOrderRepository<Order>
    {
        private readonly OrderApiContext _db;

        public OrderOrderRepository(OrderApiContext context)
        {
            _db = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Order> AddAsync(Order entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.Date == null)
                entity.Date = DateTime.Now;

            var newOrder = (await _db.Orders.AddAsync(entity)).Entity;
            await _db.SaveChangesAsync();
            return newOrder;
        }

        public async Task EditAsync(int id, Order entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var order = await _db.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = entity.Status;
                _db.Entry(order).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Order>> GetAllByCustomerAsync(int customerId)
        {
            return await _db.Orders
                            .Where(order => order.CustomerId == customerId)
                            .Include(o => o.OrderLines)
                            .ToListAsync();
        }

        public async Task<Order> GetAsync(int id)
        {
            return await _db.Orders
                            .Include(o => o.OrderLines)
                            .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _db.Orders
                            .Include(o => o.OrderLines)
                            .ToListAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(p => p.Id == id);
            if (order != null)
            {
                _db.Orders.Remove(order);
                await _db.SaveChangesAsync();
            }
        }
    }

