using Microsoft.EntityFrameworkCore;
using SoaEcommerce.Contracts;
using SoaEcommerce.CustomerService.Models;

namespace SoaEcommerce.CustomerService.Data;

public class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Seed data
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "João Silva",
                Email = "joao.silva@email.com",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Customer
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Maria Santos",
                Email = "maria.santos@email.com",
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        );
    }
}
