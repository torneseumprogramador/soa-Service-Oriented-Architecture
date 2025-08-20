using CoreWCF;
using Microsoft.EntityFrameworkCore;
using SoaEcommerce.Contracts;
using SoaEcommerce.CustomerService.Data;
using SoaEcommerce.CustomerService.Models;

namespace SoaEcommerce.CustomerService.Services;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
public class CustomerService : ICustomerService
{
    private readonly CustomerDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(CustomerDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public CreateCustomerResponse CreateCustomer(CreateCustomerRequest request)
    {
        _logger.LogInformation("Criando cliente: {Email}", request.Email);

        try
        {
            // Se já existir cliente com o mesmo email, retornar o existente como sucesso
            var existing = _context.Customers.AsNoTracking().FirstOrDefault(c => c.Email == request.Email);
            if (existing != null)
            {
                _logger.LogInformation("Cliente já existe, retornando existente: {Email}", request.Email);
                return new CreateCustomerResponse
                {
                    CustomerId = existing.Id,
                    Success = true,
                    Message = "Cliente já existente",
                    Customer = new CustomerDto
                    {
                        Id = existing.Id,
                        Name = existing.Name,
                        Email = existing.Email,
                        Status = (Contracts.CustomerStatus)existing.Status,
                        CreatedAt = existing.CreatedAt
                    }
                };
            }

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Status = CustomerStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            _logger.LogInformation("Cliente criado com sucesso: {CustomerId}", customer.Id);

            return new CreateCustomerResponse
            {
                CustomerId = customer.Id,
                Success = true,
                Message = "Cliente criado com sucesso",
                Customer = new CustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    Status = (Contracts.CustomerStatus)customer.Status,
                    CreatedAt = customer.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente: {Email}", request.Email);
            throw Faults.InvalidRequest($"Erro ao criar cliente: {ex.Message}");
        }
    }

    public GetCustomerResponse GetCustomer(GetCustomerRequest request)
    {
        _logger.LogInformation("Buscando cliente: {CustomerId}", request.CustomerId);

        var customer = _context.Customers.FirstOrDefault(c => c.Id == request.CustomerId);

        if (customer == null)
        {
            _logger.LogWarning("Cliente não encontrado: {CustomerId}", request.CustomerId);
            throw Faults.InvalidCustomer();
        }

        return new GetCustomerResponse
        {
            Customer = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Status = (Contracts.CustomerStatus)customer.Status,
                CreatedAt = customer.CreatedAt
            },
            Success = true
        };
    }

    public GetCustomerStatusResponse GetCustomerStatus(GetCustomerStatusRequest request)
    {
        _logger.LogInformation("Verificando status do cliente: {CustomerId}", request.CustomerId);

        var customer = _context.Customers.FirstOrDefault(c => c.Id == request.CustomerId);

        if (customer == null)
        {
            _logger.LogWarning("Cliente não encontrado: {CustomerId}", request.CustomerId);
            throw Faults.InvalidCustomer();
        }

        // Mock score baseado na data de criação
        var score = customer.CreatedAt < DateTime.UtcNow.AddDays(-30) ? 100 : 50;

        return new GetCustomerStatusResponse
        {
            IsActive = customer.Status == CustomerStatus.Active,
            Score = score,
            Success = true
        };
    }
}
