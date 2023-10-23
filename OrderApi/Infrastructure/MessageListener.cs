using EasyNetQ;
using OrderApi.Data;
using OrderApi.Models;
using SharedModels;
using SharedModels.Messages;

namespace OrderApi.Infrastructure;

    public class MessageListener
    {
        private readonly IServiceProvider _provider;
        private readonly string _connectionString;
        private IBus _bus;

        public MessageListener(IServiceProvider provider, string connectionString)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task StartAsync()
        {
            _bus = RabbitHutch.CreateBus(_connectionString);
            _bus.PubSub.SubscribeAsync<OrderAcceptedMessage>("order.Accepted", HandleOrderAcceptedAsync);
            _bus.PubSub.SubscribeAsync<OrderRejectedMessage>("order.Rejected", HandleOrderRejectedAsync);
            
            await Task.Delay(Timeout.Infinite);
        }

        private async Task HandleOrderRejectedAsync(OrderRejectedMessage obj)
        {
            Console.WriteLine($"ORDER REJECTED {obj.OrderId}");
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetRequiredService<IOrderRepository<Order>>();

            await orderRepo.RemoveAsync(obj.OrderId);
            Console.WriteLine($"ORDER REMOVED");
        }

        private async Task HandleOrderAcceptedAsync(OrderAcceptedMessage obj)
        {
            Console.WriteLine($"ORDER ACCEPTED {obj.OrderId}");
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var orderRepo = services.GetRequiredService<IOrderRepository<Order>>();

            var order = await orderRepo.GetAsync(obj.OrderId);
            if (order != null)
            {
                order.Status = OrderDto.OrderStatus.completed;
                await orderRepo.EditAsync(order.Id, order);
                Console.WriteLine($"ORDER {obj.OrderId} CHANGED STATUS TO COMPLETED");
            }
        }
    }

