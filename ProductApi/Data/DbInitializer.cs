using System.Collections.Generic;
using System.Linq;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        public void Initialize(ProductApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            if (context.Products.Any())
            {
                return;   
            }

            List<Product> products = new List<Product>
            {
                new Product { Name = "Cog", Price = 5, ItemsInStock = 200, ItemsReserved = 5 },
                new Product { Name = "Screwdriver", Price = 70, ItemsInStock = 20, ItemsReserved = 0 },
                new Product { Name = "Drill", Price = 500, ItemsInStock = 2, ItemsReserved = 0 },
                new Product { Name = "Sledgehammer", Price = 750, ItemsInStock = 5, ItemsReserved = 1 },
                new Product { Name = "Saw", Price = 250, ItemsInStock = 2, ItemsReserved = 0 }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
