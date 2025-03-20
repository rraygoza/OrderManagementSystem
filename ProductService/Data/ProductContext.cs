using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace ProductService.Data
{
    public class ProductContext : DbContext
    {
        public ProductContext(DbContextOptions<ProductContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Product 1",
                    Price = 100
                },
                new Product
                {
                    Id = 2,
                    Name = "Product 2",
                    Price = 200
                },
                new Product
                {
                    Id = 3,
                    Name = "Product 3",
                    Price = 300
                }
            );
            base.OnModelCreating(modelBuilder);
        }
    }
}
