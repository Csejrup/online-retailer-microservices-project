
using SharedModels;

namespace OrderApi.Infrastructure;

    public interface IMessagePublisher
    {
        Task<bool> PublishOrderCreateMessageAsync(int customerId, int orderId, IList<OrderLine> orderLine);
        Task OrderStatusChangedMessageAsync(int id, IList<OrderLine> orderLine, string topic);
        Task CreditStandingChangedMessageAsync(int customerId);
    }
