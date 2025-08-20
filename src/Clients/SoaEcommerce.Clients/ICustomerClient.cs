using SoaEcommerce.Contracts;

namespace SoaEcommerce.Clients;

public interface ICustomerClient
{
    CreateCustomerResponse CreateCustomer(CreateCustomerRequest request);
    GetCustomerResponse GetCustomer(GetCustomerRequest request);
    GetCustomerStatusResponse GetCustomerStatus(GetCustomerStatusRequest request);
}
