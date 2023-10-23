using CustomersApi.Data;
using CustomersApi.Models;
using Microsoft.AspNetCore.Mvc;
using SharedModels;

namespace CustomersApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> _repository;
        private readonly IConverter<Customer, CustomerDto> _customerConverter;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(IRepository<Customer> repository, 
                                    IConverter<Customer, CustomerDto> customerConverter,
                                    ILogger<CustomersController> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _customerConverter = customerConverter ?? throw new ArgumentNullException(nameof(customerConverter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
        {
            try
            {
                var customerDtoList = new List<CustomerDto>();
                await foreach (var customer in await _repository.GetAllAsync())
                {
                    var customerDto = _customerConverter.ConvertModelToDto(customer);
                    customerDtoList.Add(customerDto);
                }
                return Ok(customerDtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all customers.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                var customer = await _repository.GetAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }
                var customerDto = _customerConverter.ConvertModelToDto(customer);
                return new ObjectResult(customerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting the customer with ID {id}.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CustomerDto customerDto)
        {
            if (customerDto == null)
            {
                return BadRequest("Customer data is required.");
            }

            try
            {
                var customer = _customerConverter.ConvertDtoToModel(customerDto);
                var newCustomer = await _repository.AddAsync(customer);
                return new ObjectResult(newCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering a new customer.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CustomerDto customerDto)
        {
            if (customerDto == null || customerDto.Id != id)
            {
                return BadRequest("Invalid customer data or ID.");
            }

            try
            {
                var modifiedCustomer = await _repository.GetAsync(id);

                if (modifiedCustomer == null)
                {
                    return NotFound();
                }

                modifiedCustomer.Email = customerDto.Email;
                modifiedCustomer.Phone = customerDto.Phone;
                modifiedCustomer.BillingAddress = customerDto.BillingAddress;
                modifiedCustomer.ShippingAddress = customerDto.ShippingAddress;
                await _repository.EditAsync(modifiedCustomer);
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the customer with ID {id}.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _repository.GetAsync(id);

                if (customer == null)
                {
                    return NotFound();
                }

                await _repository.RemoveAsync(id);
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the customer with ID {id}.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
