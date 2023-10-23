
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using SharedModels;
using Order = OrderApi.Models.Order;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository<Order> _orderRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOrderConverter _converter;

        public OrdersController(IOrderRepository<Order> repos,
            IMessagePublisher publisher,
            IOrderConverter orderConverter)
        {
            _orderRepository = repos;
            _messagePublisher = publisher;
            _converter = orderConverter;
        }

        // GET: orders
        [HttpGet]
        public async Task<IEnumerable<OrderDto>> GetAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            var dtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderLines = o.OrderLines,
                CustomerId = o.CustomerId,
                Status = o.Status,
                Date = o.Date
            });
            return dtos;
        }

        [HttpGet("getAllByCustomer/{customerId}")]
        public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(int customerId)
        {
            var orders = await _orderRepository.GetAllByCustomerAsync(customerId);
            var dtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderLines = o.OrderLines,
                CustomerId = o.CustomerId,
                Status = o.Status,
                Date = o.Date
            });
            return dtos;
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var item = await _orderRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return new ObjectResult(item);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] OrderDto order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            try
            {
                order.Status = OrderDto.OrderStatus.pending;
        
                var newOrder = await _orderRepository.AddAsync(_converter.Convert(order));
                bool orderAccepted = await _messagePublisher.PublishOrderCreateMessageAsync(
                    newOrder.CustomerId, newOrder.Id, newOrder.OrderLines);
        
                if (!orderAccepted)
                {
                    return BadRequest("The order was rejected because the customer does not exist.");
                }
        
                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }
            catch
            {
                return StatusCode(500, "An error occurred.");
            }
        }

        

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelAsync(int id)
        {
            var order = await _orderRepository.GetAsync(id);

            if (order == null)
            {
                return NotFound();
            }
            if (order.Status ==  OrderDto.OrderStatus.shipped)
            {
                return StatusCode(403);
            }
            await _orderRepository.EditAsync(id, new Order
            {
                Id = id,
                Status = OrderDto.OrderStatus.cancelled
            });
            await _messagePublisher.OrderStatusChangedMessageAsync(id, order.OrderLines, "cancelled");
            await _messagePublisher.CreditStandingChangedMessageAsync(order.CustomerId);
            return Ok();
        }
        
        [HttpPut("ship/{id}")]
        public async Task<IActionResult> ShipAsync(int id)
        {
            var order = await _orderRepository.GetAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            await _orderRepository.EditAsync(id, new Order
            {
                Id = id,
                Status = OrderDto.OrderStatus.shipped
            });
            await _messagePublisher.OrderStatusChangedMessageAsync(id, order.OrderLines, "shipped");
            return Ok();
        }
        
        [HttpPut("pay/{id}")]
        public async Task<IActionResult> PayAsync(int id)
        {
            var order = await _orderRepository.GetAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            await _orderRepository.EditAsync(id, new Order
            {
                Id = id,
                Status = OrderDto.OrderStatus.paid
            });
            await _messagePublisher.CreditStandingChangedMessageAsync(order.CustomerId);
            return Ok();
        }
    }
}
