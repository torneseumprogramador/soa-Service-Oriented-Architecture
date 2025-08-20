using CoreWCF;
using Microsoft.EntityFrameworkCore;
using SoaEcommerce.CatalogService.Data;
using SoaEcommerce.CatalogService.Models;
using SoaEcommerce.Contracts;

namespace SoaEcommerce.CatalogService.Services;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
public class CatalogService : ICatalogService
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<CatalogService> _logger;

    public CatalogService(CatalogDbContext context, ILogger<CatalogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public CreateProductResponse CreateProduct(CreateProductRequest request)
    {
        _logger.LogInformation("Criando produto: {Name}", request.Name);

        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = request.Price,
                Stock = request.Stock,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            _logger.LogInformation("Produto criado com sucesso: {ProductId}", product.Id);

            return new CreateProductResponse
            {
                ProductId = product.Id,
                Success = true,
                Message = "Produto criado com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto: {Name}", request.Name);
            throw Faults.InvalidRequest($"Erro ao criar produto: {ex.Message}");
        }
    }

    public GetProductResponse GetProduct(GetProductRequest request)
    {
        _logger.LogInformation("Buscando produto: {ProductId}", request.ProductId);

        var product = _context.Products.FirstOrDefault(p => p.Id == request.ProductId);

        if (product == null)
        {
            _logger.LogWarning("Produto não encontrado: {ProductId}", request.ProductId);
            throw Faults.ProductNotFound(request.ProductId);
        }

        return new GetProductResponse
        {
            Product = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive
            },
            Success = true
        };
    }

    public ReserveInventoryResponse ReserveInventory(ReserveInventoryRequest request)
    {
        _logger.LogInformation("Reservando estoque para {Count} itens", request.Lines.Count);

        var response = new ReserveInventoryResponse();
        var issues = new List<string>();

        foreach (var line in request.Lines)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == line.ProductId);

            if (product == null)
            {
                issues.Add($"Produto {line.ProductId} não encontrado");
                continue;
            }

            if (!product.IsActive)
            {
                issues.Add($"Produto {product.Name} está inativo");
                continue;
            }

            if (product.Stock < line.Quantity)
            {
                issues.Add($"Estoque insuficiente para {product.Name}: solicitado {line.Quantity}, disponível {product.Stock}");
                continue;
            }

            // Reservar estoque
            product.Stock -= line.Quantity;
            _context.SaveChanges();

            response.PricedLines.Add(new PricedLine
            {
                ProductId = product.Id,
                Quantity = line.Quantity,
                UnitPrice = product.Price
            });

            _logger.LogInformation("Estoque reservado: {ProductName} - {Quantity} unidades", product.Name, line.Quantity);
        }

        response.Success = issues.Count == 0;
        response.Issues = issues;

        if (!response.Success)
        {
            _logger.LogWarning("Falha na reserva de estoque: {Issues}", string.Join("; ", issues));
        }

        return response;
    }

    public ReleaseInventoryResponse ReleaseInventory(ReleaseInventoryRequest request)
    {
        _logger.LogInformation("Liberando estoque para {Count} itens", request.Lines.Count);

        var releasedCount = 0;

        foreach (var line in request.Lines)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == line.ProductId);

            if (product != null)
            {
                product.Stock += line.Quantity;
                releasedCount += line.Quantity;
                _logger.LogInformation("Estoque liberado: {ProductName} - {Quantity} unidades", product.Name, line.Quantity);
            }
        }

        _context.SaveChanges();

        return new ReleaseInventoryResponse
        {
            ReleasedCount = releasedCount,
            Success = true
        };
    }
}
