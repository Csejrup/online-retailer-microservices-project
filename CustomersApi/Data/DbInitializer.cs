using System.Collections.Generic;
using System.Linq;
using CustomersApi.Models;

namespace CustomersApi.Data;

public class DbInitializer : IDbInitializer
{
    public void Initialize(CustomerApiContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        
        if (context.Customers.Any())
        {
            return;   
        }

        var customers = new List<Customer>()
        {
            new Customer
            {
                Name = "Alice",
                BillingAddress = "123 Wonderland Ave",
                Email = "alice@email.com",
                CreditStanding = true,
                Phone = 384746372,
                ShippingAddress = "123 Wonderland Ave"
            },
            new Customer
            {
                Name = "Bob",
                BillingAddress = "456 Evergreen St",
                Email = "bob@email.com",
                CreditStanding = false,
                Phone = 84739273,
                ShippingAddress = "456 Evergreen St"
            },
            new Customer
            {
                Name = "Charlie",
                BillingAddress = "789 Chocolate Factory Rd",
                Email = "charlie@email.com",
                CreditStanding = true,
                Phone = 83948473,
                ShippingAddress = "789 Chocolate Factory Rd"
            },
            new Customer
            {
                Name = "David",
                BillingAddress = "101 Main St",
                Email = "david@email.com",
                CreditStanding = false,
                Phone = 03847573,
                ShippingAddress = "101 Main St"
            },
            new Customer
            {
                Name = "Eve",
                BillingAddress = "202 Lake View",
                Email = "eve@email.com",
                CreditStanding = true,
                Phone = 38475968,
                ShippingAddress = "202 Lake View"
            }
        };
        context.Customers.AddRange(customers);
        context.SaveChanges();
    }
}