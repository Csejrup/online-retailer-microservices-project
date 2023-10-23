using CustomersApi.Data;
using CustomersApi.Models;
using EasyNetQ;
using SharedModels.Messages;

namespace CustomersApi.Infrastructure
{
    public class MessageListener
    {
        private readonly IServiceProvider _provider;
        private readonly IBus _bus;

        public MessageListener(IServiceProvider provider, IBus bus)
        {
            _provider = provider;
            _bus = bus;
        }

        public void Start()
        {
            _bus.PubSub.SubscribeAsync<OrderCreatedMessage>("customer.orderCreated", HandleOrderCreated);
            _bus.PubSub.SubscribeAsync<CreditStandingChangedMessage>("creditChanged", HandleChangeCreditStanding);
            
            lock (this)
            {
                Monitor.Wait(this);
            }
        }

        private async Task HandleChangeCreditStanding(CreditStandingChangedMessage obj)
        {
            Console.WriteLine("Change customer standing to true");
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var customerRepository = services.GetService<IRepository<Customer>>();
            
            var customer = await customerRepository.GetAsync(obj.CustomerId);

            if (customer != null && !customer.CreditStanding)
            {
                customer.CreditStanding = true;
                await customerRepository.EditAsync(customer);
            }
        }

        private async Task HandleOrderCreated(OrderCreatedMessage message)
        {
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var customerRepository = services.GetService<IRepository<Customer>>();

            if (await GoodStanding(message.CustomerId, customerRepository))
            {
                var customer = await customerRepository.GetAsync(message.CustomerId);
                
                if (customer != null)
                {
                    customer.CreditStanding = false;
                    await customerRepository.EditAsync(customer);
                }

                var replyMessage = new OrderAcceptedMessage()
                {
                    OrderId = message.OrderId
                };

                await _bus.PubSub.PublishAsync(replyMessage);
            }
            else
            {
                var replyMessage = new OrderRejectedMessage
                {
                    OrderId = message.OrderId
                };

                await _bus.PubSub.PublishAsync(replyMessage);
            }
        }

        private async Task<bool> GoodStanding(int customerId, IRepository<Customer> customerRepo)
        {
            var customer = await customerRepo.GetAsync(customerId);
            return customer?.CreditStanding ?? false;
        }

        private async Task<CustomerValidationResponse> HandleCustomerValidationRequest(CustomerValidationRequest request, CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();
            var services = scope.ServiceProvider;
            var customerRepository = services.GetService<IRepository<Customer>>();
            
            var customerExists = await customerRepository.GetAsync(request.CustomerId) != null;
            
            return new CustomerValidationResponse
            {
                CustomerId = request.CustomerId,
                Exists = customerExists
            };
        }
    }
}
