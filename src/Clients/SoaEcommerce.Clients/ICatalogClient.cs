using SoaEcommerce.Contracts;

namespace SoaEcommerce.Clients;

public interface ICatalogClient
{
    CreateProductResponse CreateProduct(CreateProductRequest request);
    GetProductResponse GetProduct(GetProductRequest request);
    ReserveInventoryResponse ReserveInventory(ReserveInventoryRequest request);
    ReleaseInventoryResponse ReleaseInventory(ReleaseInventoryRequest request);
}
