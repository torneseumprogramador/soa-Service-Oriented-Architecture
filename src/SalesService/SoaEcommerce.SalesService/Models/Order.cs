using System.ComponentModel.DataAnnotations;
using SoaEcommerce.Contracts;

namespace SoaEcommerce.SalesService.Models;

public class Order
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid CustomerId { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    [Required]
    public decimal Total { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid OrderId { get; set; }
    
    [Required]
    public Guid ProductId { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal UnitPrice { get; set; }
    
    [Required]
    public decimal Subtotal { get; set; }
    
    // Navigation property
    public virtual Order Order { get; set; } = null!;
}
