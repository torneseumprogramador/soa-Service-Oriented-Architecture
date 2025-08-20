using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoaEcommerce.Contracts;

namespace SoaEcommerce.Clients;

public class CatalogClient : BaseSoapClient, ICatalogClient
{
    public CatalogClient(HttpClient httpClient, ILogger<CatalogClient> logger, IConfiguration configuration)
        : base(httpClient, logger, configuration, "CatalogService")
    {
    }

    public CreateProductResponse CreateProduct(CreateProductRequest request)
    {
        return SendSoapRequestAsync<CreateProductResponse>("CreateProduct", request).GetAwaiter().GetResult();
    }

    public GetProductResponse GetProduct(GetProductRequest request)
    {
        return SendSoapRequestAsync<GetProductResponse>("GetProduct", request).GetAwaiter().GetResult();
    }

    public ReserveInventoryResponse ReserveInventory(ReserveInventoryRequest request)
    {
        return SendSoapRequestAsync<ReserveInventoryResponse>("ReserveInventory", request).GetAwaiter().GetResult();
    }

    public ReleaseInventoryResponse ReleaseInventory(ReleaseInventoryRequest request)
    {
        return SendSoapRequestAsync<ReleaseInventoryResponse>("ReleaseInventory", request).GetAwaiter().GetResult();
    }

    protected override string GetServiceNamespace() => "catalog";
}
