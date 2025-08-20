using Microsoft.EntityFrameworkCore;
using SoaEcommerce.CatalogService.Models;

namespace SoaEcommerce.CatalogService.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Stock).IsRequired();
        });

        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Notebook Dell Inspiron",
                Price = 2999.99m,
                Stock = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Product
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Mouse Gamer RGB",
                Price = 89.90m,
                Stock = 50,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Product
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Teclado Mecânico",
                Price = 299.90m,
                Stock = 25,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        );
    }
}
