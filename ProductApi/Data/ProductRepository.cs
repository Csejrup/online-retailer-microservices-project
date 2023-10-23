using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data;

    public class ProductRepository : IRepository<Product>
    {
        private readonly ProductApiContext _db;

        public ProductRepository(ProductApiContext context)
        {
            _db = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Product> AddAsync(Product entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var newProduct = (await _db.Products.AddAsync(entity)).Entity;
            await _db.SaveChangesAsync();
            return newProduct;
        }

        public async Task EditAsync(Product entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _db.Entry(entity).State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task<Product> GetAsync(int id)
        {
            return await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.Products.ToListAsync();
        }

        public async Task RemoveAsync(int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
        }
    }

