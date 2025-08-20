using SoaEcommerce.Contracts;

namespace SoaEcommerce.Clients;

public interface ISalesClient
{
    CreateOrderResponse CreateOrder(CreateOrderRequest request);
    ConfirmOrderResponse ConfirmOrder(ConfirmOrderRequest request);
    CancelOrderResponse CancelOrder(CancelOrderRequest request);
    GetOrderResponse GetOrder(GetOrderRequest request);
}
