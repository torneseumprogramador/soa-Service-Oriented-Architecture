namespace SoaEcommerce.CompositionService.Services;

public interface IPaymentService
{
    bool ProcessPayment(Guid orderId, decimal amount);
}

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly Random _random = new Random();

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public bool ProcessPayment(Guid orderId, decimal amount)
    {
        _logger.LogInformation("Processando pagamento: OrderId={OrderId}, Amount={Amount}", orderId, amount);

        // Simular processamento de pagamento
        Thread.Sleep(100); // Simular latência

        // 90% de chance de sucesso
        var success = _random.Next(1, 11) <= 9;

        if (success)
        {
            _logger.LogInformation("Pagamento aprovado: OrderId={OrderId}", orderId);
        }
        else
        {
            _logger.LogWarning("Pagamento rejeitado: OrderId={OrderId}", orderId);
        }

        return success;
    }
}
