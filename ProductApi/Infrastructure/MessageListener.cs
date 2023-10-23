using EasyNetQ;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
using SharedModels.Messages;

namespace ProductApi.Infrastructure
{
    public class MessageListener
    {
        private readonly IServiceProvider _provider;
        private readonly string _connection;
        private readonly IBus _bus;

        public MessageListener(IServiceProvider serviceProvider, string connection, IBus bus)
        {
            _provider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task StartAsync()
        {
            await _bus.PubSub.SubscribeAsync<OrderCreatedMessage>("product.orderCreated", HandleOrderCreatedAsync);
            await _bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>("product.orderStatusChanged",
                (msg, ct) => HandleOrderCancelledAsync(msg), cfg => cfg.WithTopic("cancelled"));

            await _bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>("product.orderStatusChanged",
                (msg, ct) => HandleOrderShippedAsync(msg), cfg => cfg.WithTopic("shipped"));
                
            lock (this)
            {
                Monitor.Wait(this);
            }
        }

        private async Task HandleOrderShippedAsync(OrderStatusChangedMessage obj)
        {
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var repo = services.GetService<IRepository<Product>>();

            foreach (var orderLine in obj.OrderLine)
            {
                var product = await repo.GetAsync(orderLine.ProductId);
                product.ItemsReserved -= orderLine.Quantity;
                product.ItemsInStock -= orderLine.Quantity;
                await repo.EditAsync(product);
            }
        }

        private async Task HandleOrderCancelledAsync(OrderStatusChangedMessage obj)
        {
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var repo = services.GetService<IRepository<Product>>();

            foreach (var orderLine in obj.OrderLine)
            {
                var product = await repo.GetAsync(orderLine.ProductId);
                product.ItemsReserved -= orderLine.Quantity;
                await repo.EditAsync(product);
            }
        }

        private async Task HandleOrderCreatedAsync(OrderCreatedMessage message)
        {
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var productRepo = services.GetService<IRepository<Product>>();

            if (await ProductItemsAvailableAsync(message.OrderLines, productRepo))
            {
                foreach (var order in message.OrderLines)
                {
                    var product = await productRepo.GetAsync(order.ProductId);
                    product.ItemsReserved += order.Quantity;
                    await productRepo.EditAsync(product);
                }

                var replyMessage = new OrderAcceptedMessage { OrderId = message.OrderId };
                _bus.PubSub.Publish(replyMessage);
            }
            else
            {
                var replyMessage = new OrderRejectedMessage { OrderId = message.OrderId };
                _bus.PubSub.Publish(replyMessage);
            }
        }

        private async Task<bool> ProductItemsAvailableAsync(IList<OrderLine> orderLines, IRepository<Product> productRepo)
        {
            foreach (var order in orderLines)
            {
                var product = await productRepo.GetAsync(order.ProductId);
                if (order.Quantity > product.ItemsInStock - product.ItemsReserved)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
