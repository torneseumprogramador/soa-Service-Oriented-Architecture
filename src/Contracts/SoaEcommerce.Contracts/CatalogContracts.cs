using CoreWCF;
using System.Xml.Serialization;

namespace SoaEcommerce.Contracts;

[ServiceContract(Namespace = "urn:soa-ecommerce:v1:catalog")]
public interface ICatalogService
{
    [OperationContract]
    CreateProductResponse CreateProduct(CreateProductRequest request);

    [OperationContract]
    GetProductResponse GetProduct(GetProductRequest request);

    [OperationContract]
    ReserveInventoryResponse ReserveInventory(ReserveInventoryRequest request);

    [OperationContract]
    ReleaseInventoryResponse ReleaseInventory(ReleaseInventoryRequest request);
}

[XmlRoot("CreateProductRequest", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class CreateProductRequest
{
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Price")]
    public decimal Price { get; set; }

    [XmlElement("Stock")]
    public int Stock { get; set; }
}

[XmlRoot("CreateProductResponse", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class CreateProductResponse
{
    [XmlElement("ProductId")]
    public Guid ProductId { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
}

[XmlRoot("GetProductRequest", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class GetProductRequest
{
    [XmlElement("ProductId")]
    public Guid ProductId { get; set; }
}

[XmlRoot("GetProductResponse", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class GetProductResponse
{
    [XmlElement("Product")]
    public ProductDto? Product { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }
}

[XmlRoot("ReserveInventoryRequest", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class ReserveInventoryRequest
{
    [XmlArray("Lines"), XmlArrayItem("Line")]
    public List<ReserveLine> Lines { get; set; } = new();
}

[XmlRoot("ReserveLine", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class ReserveLine
{
    [XmlElement("ProductId")]
    public Guid ProductId { get; set; }

    [XmlElement("Quantity")]
    public int Quantity { get; set; }
}

[XmlRoot("ReserveInventoryResponse", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class ReserveInventoryResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlArray("PricedLines"), XmlArrayItem("Line")]
    public List<PricedLine> PricedLines { get; set; } = new();

    [XmlArray("Issues"), XmlArrayItem("Issue")]
    public List<string> Issues { get; set; } = new();
}

[XmlRoot("PricedLine", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class PricedLine
{
    [XmlElement("ProductId")]
    public Guid ProductId { get; set; }

    [XmlElement("Quantity")]
    public int Quantity { get; set; }

    [XmlElement("UnitPrice")]
    public decimal UnitPrice { get; set; }
}

[XmlRoot("ReleaseInventoryRequest", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class ReleaseInventoryRequest
{
    [XmlArray("Lines"), XmlArrayItem("Line")]
    public List<ReserveLine> Lines { get; set; } = new();
}

[XmlRoot("ReleaseInventoryResponse", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class ReleaseInventoryResponse
{
    [XmlElement("ReleasedCount")]
    public int ReleasedCount { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }
}

[XmlRoot("Product", Namespace = "urn:soa-ecommerce:v1:catalog")]
public class ProductDto
{
    [XmlElement("Id")]
    public Guid Id { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Price")]
    public decimal Price { get; set; }

    [XmlElement("Stock")]
    public int Stock { get; set; }

    [XmlElement("IsActive")]
    public bool IsActive { get; set; }
}
