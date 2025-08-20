using CoreWCF;
using Microsoft.EntityFrameworkCore;
using SoaEcommerce.Contracts;
using SoaEcommerce.SalesService.Data;
using SoaEcommerce.SalesService.Models;

namespace SoaEcommerce.SalesService.Services;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
public class SalesService : ISalesService
{
    private readonly SalesDbContext _context;
    private readonly ILogger<SalesService> _logger;

    public SalesService(SalesDbContext context, ILogger<SalesService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public CreateOrderResponse CreateOrder(CreateOrderRequest request)
    {
        _logger.LogInformation("Criando pedido para cliente: {CustomerId}", request.CustomerId);

        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var total = 0m;
            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Quantity * item.UnitPrice
                };

                order.Items.Add(orderItem);
                total += orderItem.Subtotal;
            }

            order.Total = total;

            _context.Orders.Add(order);
            _context.SaveChanges();

            _logger.LogInformation("Pedido criado com sucesso: {OrderId}", order.Id);

            return new CreateOrderResponse
            {
                OrderId = order.Id,
                Success = true,
                Message = "Pedido criado com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido para cliente: {CustomerId}", request.CustomerId);
            throw Faults.InvalidRequest($"Erro ao criar pedido: {ex.Message}");
        }
    }

    public ConfirmOrderResponse ConfirmOrder(ConfirmOrderRequest request)
    {
        _logger.LogInformation("Confirmando pedido: {OrderId}", request.OrderId);

        var order = _context.Orders.FirstOrDefault(o => o.Id == request.OrderId);

        if (order == null)
        {
            _logger.LogWarning("Pedido não encontrado: {OrderId}", request.OrderId);
            throw Faults.OrderNotFound(request.OrderId);
        }

        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogWarning("Pedido não está pendente: {OrderId} - Status: {Status}", request.OrderId, order.Status);
            throw Faults.InvalidRequest("Pedido não está pendente");
        }

        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();

        _logger.LogInformation("Pedido confirmado: {OrderId}", order.Id);

        return new ConfirmOrderResponse
        {
            Success = true,
            Message = "Pedido confirmado com sucesso"
        };
    }

    public CancelOrderResponse CancelOrder(CancelOrderRequest request)
    {
        _logger.LogInformation("Cancelando pedido: {OrderId} - Motivo: {Reason}", request.OrderId, request.Reason);

        var order = _context.Orders.FirstOrDefault(o => o.Id == request.OrderId);

        if (order == null)
        {
            _logger.LogWarning("Pedido não encontrado: {OrderId}", request.OrderId);
            throw Faults.OrderNotFound(request.OrderId);
        }

        if (order.Status == OrderStatus.Canceled)
        {
            _logger.LogWarning("Pedido já está cancelado: {OrderId}", request.OrderId);
            throw Faults.InvalidRequest("Pedido já está cancelado");
        }

        order.Status = OrderStatus.Canceled;
        order.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();

        _logger.LogInformation("Pedido cancelado: {OrderId}", order.Id);

        return new CancelOrderResponse
        {
            Success = true,
            Message = "Pedido cancelado com sucesso"
        };
    }

    public GetOrderResponse GetOrder(GetOrderRequest request)
    {
        _logger.LogInformation("Buscando pedido: {OrderId}", request.OrderId);

        var order = _context.Orders
            .Include(o => o.Items)
            .FirstOrDefault(o => o.Id == request.OrderId);

        if (order == null)
        {
            _logger.LogWarning("Pedido não encontrado: {OrderId}", request.OrderId);
            throw Faults.OrderNotFound(request.OrderId);
        }

        return new GetOrderResponse
        {
            Order = new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = (Contracts.OrderStatus)order.Status,
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Subtotal
                }).ToList()
            },
            Success = true
        };
    }
}
