using SharedModels;
using Order = OrderApi.Models.Order;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (context.Orders.Any())
            {
                return;   
            }
            
            
            
            var orderLines = new List<OrderLine>{
                new OrderLine{Id = 0,Quantity = 10,ProductId = 1,OrderId = 0},
                new OrderLine{Id = 0,Quantity = 20,ProductId = 2,OrderId = 0},
                new OrderLine{Id = 0,Quantity = 4,ProductId = 3,OrderId = 5}
                
            };
            
            var orders = new List<Order>
            {
                new Order{Id =0,Date = DateTime.Today, Status = SharedModels.OrderDto.OrderStatus.pending,CustomerId = 0,OrderLines = orderLines},
            };

            context.Orders.AddRange(orders);
            context.OrderLines.AddRange(orderLines);
            context.SaveChanges();
        }
    }
}
