using System.ComponentModel.DataAnnotations;
using SoaEcommerce.Contracts;

namespace SoaEcommerce.CustomerService.Models;

public class Customer
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
