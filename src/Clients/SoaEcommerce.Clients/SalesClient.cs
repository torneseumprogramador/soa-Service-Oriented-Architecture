using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoaEcommerce.Contracts;

namespace SoaEcommerce.Clients;

public class SalesClient : BaseSoapClient, ISalesClient
{
    public SalesClient(HttpClient httpClient, ILogger<SalesClient> logger, IConfiguration configuration)
        : base(httpClient, logger, configuration, "SalesService")
    {
    }

    public CreateOrderResponse CreateOrder(CreateOrderRequest request)
    {
        return SendSoapRequestAsync<CreateOrderResponse>("CreateOrder", request).GetAwaiter().GetResult();
    }

    public ConfirmOrderResponse ConfirmOrder(ConfirmOrderRequest request)
    {
        return SendSoapRequestAsync<ConfirmOrderResponse>("ConfirmOrder", request).GetAwaiter().GetResult();
    }

    public CancelOrderResponse CancelOrder(CancelOrderRequest request)
    {
        return SendSoapRequestAsync<CancelOrderResponse>("CancelOrder", request).GetAwaiter().GetResult();
    }

    public GetOrderResponse GetOrder(GetOrderRequest request)
    {
        return SendSoapRequestAsync<GetOrderResponse>("GetOrder", request).GetAwaiter().GetResult();
    }

    protected override string GetServiceNamespace() => "sales";
}
