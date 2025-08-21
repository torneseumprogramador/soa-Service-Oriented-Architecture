using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoaEcommerce.Contracts;

namespace SoaEcommerce.Clients;

public class CustomerClient : BaseSoapClient, ICustomerClient
{
    public CustomerClient(HttpClient httpClient, ILogger<CustomerClient> logger, IConfiguration configuration)
        : base(httpClient, logger, configuration, "CustomerService")
    {
    }

    public CreateCustomerResponse CreateCustomer(CreateCustomerRequest request)
    {
        return SendSoapRequestAsync<CreateCustomerResponse>("CreateCustomer", request).GetAwaiter().GetResult();
    }

    public GetCustomerResponse GetCustomer(GetCustomerRequest request)
    {
        return SendSoapRequestAsync<GetCustomerResponse>("GetCustomer", request).GetAwaiter().GetResult();
    }

    public GetCustomerStatusResponse GetCustomerStatus(GetCustomerStatusRequest request)
    {
        return SendSoapRequestAsync<GetCustomerStatusResponse>("GetCustomerStatus", request).GetAwaiter().GetResult();
    }

    public GetCustomerByEmailResponse GetCustomerByEmail(GetCustomerByEmailRequest request)
    {
        return SendSoapRequestAsync<GetCustomerByEmailResponse>("GetCustomerByEmail", request).GetAwaiter().GetResult();
    }

    protected override string GetServiceNamespace() => "customers";
    protected override string GetServiceName() => "Customer";
}
