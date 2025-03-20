using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace CustomerService.Data
{
    public class CustomerContext : DbContext
    {
        public CustomerContext(DbContextOptions<CustomerContext> options): base(options)
        {
            
        }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" },
                new Customer { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com" }
                );
            base.OnModelCreating(modelBuilder);
        }
    }
}
