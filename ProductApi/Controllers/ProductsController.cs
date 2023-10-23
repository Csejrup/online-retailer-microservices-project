
using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;

namespace ProductApi.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IRepository<Product> _repository;
        private readonly IConverter<Product, ProductDto> _productConverter;

        public ProductsController(IRepository<Product> repos, IConverter<Product, ProductDto> converter)
        {
            _repository = repos ?? throw new ArgumentNullException(nameof(repos));
            _productConverter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        // GET products
        [HttpGet]
        public async Task<IEnumerable<ProductDto>> Get()
        {
            try
            {
                var products = await _repository.GetAllAsync();
                var productDtos = new List<ProductDto>();

                foreach (var product in products)
                {
                    productDtos.Add(_productConverter.Convert(product));
                }

                return productDtos;
            }
            catch (Exception ex)
            {
                return new List<ProductDto>(); 
            }
        }

        // GET products/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var product = await _repository.GetAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                return new ObjectResult(_productConverter.Convert(product));
            }
            catch (Exception ex)
            {
                return Problem(); 
            }
        }

        // POST products
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductDto productDto)
        {
            try
            {
                if (productDto == null)
                {
                    return BadRequest();
                }

                var product = _productConverter.Convert(productDto);
                var createdProduct = await _repository.AddAsync(product);
                return CreatedAtRoute("GetProduct", new { id = createdProduct.Id }, _productConverter.Convert(createdProduct));
            }
            catch (Exception ex)
            {
                return Problem(); 
            }
        }

        // PUT products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProductDto productDto)
        {
            try
            {
                if (productDto == null || productDto.Id != id)
                {
                    return BadRequest();
                }

                var product = await _repository.GetAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                product.Name = productDto.Name;
                product.Price = productDto.Price;
                product.ItemsInStock = productDto.ItemsInStock;
                product.ItemsReserved = productDto.ItemsReserved;

                await _repository.EditAsync(product);
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                return Problem(); 
            }
        }

        // DELETE products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _repository.GetAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                await _repository.RemoveAsync(id);
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                return Problem(); 
            }
        }
    }

