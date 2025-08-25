using CoreWCF;
using SoaEcommerce.Clients;
using SoaEcommerce.Contracts;

namespace SoaEcommerce.CompositionService.Services;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
public class CompositionService : ICompositionService
{
    private readonly ICustomerClient _customerClient;
    private readonly ICatalogClient _catalogClient;
    private readonly ISalesClient _salesClient;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CompositionService> _logger;

    public CompositionService(
        ICustomerClient customerClient,
        ICatalogClient catalogClient,
        ISalesClient salesClient,
        IPaymentService paymentService,
        ILogger<CompositionService> logger)
    {
        _customerClient = customerClient;
        _catalogClient = catalogClient;
        _salesClient = salesClient;
        _paymentService = paymentService;
        _logger = logger;
    }

    public PlaceOrderResponse PlaceOrder(PlaceOrderRequest request)
    {
        _logger.LogInformation("Iniciando PlaceOrder para cliente: {CustomerEmail}", request.CustomerEmail);

        try
        {
            // 1. Buscar cliente por email
            var customerResponse = _customerClient.GetCustomerByEmail(new GetCustomerByEmailRequest
            {
                Email = request.CustomerEmail
            });

            if (!customerResponse.Success || customerResponse.Customer == null)
            {
                _logger.LogWarning("Cliente não encontrado: {CustomerEmail}", request.CustomerEmail);
                throw Faults.InvalidCustomer();
            }

            var customer = customerResponse.Customer;

            // 2. Validar status do cliente
            if (customer.Status != Contracts.CustomerStatus.Active)
            {
                _logger.LogWarning("Cliente inativo: {CustomerEmail}", request.CustomerEmail);
                throw Faults.InvalidCustomer();
            }

            // 2. Validar produtos e obter preços
            _logger.LogInformation("PlaceOrder: itens recebidos no request = {Count}", request.Items?.Count ?? 0);
            var reserveRequest = new ReserveInventoryRequest
            {
                Lines = request.Items.Select(item => new ReserveLine
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };
            _logger.LogInformation("PlaceOrder: linhas para reserva = {Count}", reserveRequest.Lines.Count);

            var reserveResponse = _catalogClient.ReserveInventory(reserveRequest);

            if (!reserveResponse.Success)
            {
                _logger.LogWarning("Falha na reserva de estoque: {Issues}", string.Join("; ", reserveResponse.Issues));
                throw Faults.InsufficientStock(reserveResponse.Issues);
            }

            // 3. Criar pedido
            var createOrderRequest = new CreateOrderRequest
            {
                CustomerId = customer.Id,
                Items = reserveResponse.PricedLines.Select(line => new OrderItemInput
                {
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice
                }).ToList()
            };
            _logger.LogInformation("PlaceOrder: itens para CreateOrder = {Count}", createOrderRequest.Items.Count);

            var createOrderResponse = _salesClient.CreateOrder(createOrderRequest);

            try
            {
                // 4. Processar pagamento (mock)
                var paymentSuccess = _paymentService.ProcessPayment(createOrderResponse.OrderId, 
                    reserveResponse.PricedLines.Sum(line => line.Quantity * line.UnitPrice));

                if (!paymentSuccess)
                {
                    throw new Exception("PAYMENT_FAILED");
                }

                // 5. Confirmar pedido
                _salesClient.ConfirmOrder(new ConfirmOrderRequest
                {
                    OrderId = createOrderResponse.OrderId
                });

                // 6. Obter pedido confirmado
                var order = _salesClient.GetOrder(new GetOrderRequest
                {
                    OrderId = createOrderResponse.OrderId
                });

                _logger.LogInformation("PlaceOrder concluído com sucesso: {OrderId}", createOrderResponse.OrderId);

                return new PlaceOrderResponse
                {
                    Order = order.Order,
                    Success = true,
                    Message = "Pedido processado com sucesso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante processamento do pedido: {OrderId}", createOrderResponse.OrderId);

                // Compensação: liberar estoque e cancelar pedido
                _catalogClient.ReleaseInventory(new ReleaseInventoryRequest
                {
                    Lines = reserveResponse.PricedLines.Select(line => new ReserveLine
                    {
                        ProductId = line.ProductId,
                        Quantity = line.Quantity
                    }).ToList()
                });

                _salesClient.CancelOrder(new CancelOrderRequest
                {
                    OrderId = createOrderResponse.OrderId,
                    Reason = ex.Message
                });

                throw Faults.PaymentFailed();
            }
        }
        catch (FaultException)
        {
            // Re-throw SOAP faults
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado no PlaceOrder para cliente: {CustomerEmail}", request.CustomerEmail);
            throw Faults.InvalidRequest($"Erro inesperado: {ex.Message}");
        }
    }
}
