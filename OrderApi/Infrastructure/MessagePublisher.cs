
using EasyNetQ;
using SharedModels;
using SharedModels.Messages;

namespace OrderApi.Infrastructure;

    public class MessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IBus _bus;

        public MessagePublisher(string connectionString)
        {
            
            _bus = RabbitHutch.CreateBus(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
        }

        public async Task<bool> PublishOrderCreateMessageAsync(int customerId, int orderId, IList<OrderLine> orderLine)
        {
            var message = new OrderCreatedMessage
            {
                CustomerId = customerId,
                OrderId = orderId,
                OrderLines = orderLine ?? throw new ArgumentNullException(nameof(orderLine))
            };
    
            var response = await _bus.Rpc.RequestAsync<OrderCreatedMessage, OrderCreationResponse>(message);
    
            return response.OrderAccepted; 
        }


        public async Task CreditStandingChangedMessageAsync(int customerId)
        {
            var message = new CreditStandingChangedMessage
            {
                CustomerId = customerId
            };
            await _bus.PubSub.PublishAsync(message);
        }

        public async Task OrderStatusChangedMessageAsync(int id, IList<OrderLine> orderLine, string topic)
        {
            var message = new OrderStatusChangedMessage
            {
                OrderId = id,
                OrderLine = orderLine ?? throw new ArgumentNullException(nameof(orderLine))
            };
            await _bus.PubSub.PublishAsync(message, x =>
            {
                x.WithTopic(topic ?? throw new ArgumentNullException(nameof(topic)));
            });
        }
      

        public void Dispose()
        {
            _bus?.Dispose();
        }
    }
