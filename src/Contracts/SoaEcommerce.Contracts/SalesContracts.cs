using CoreWCF;
using System.Xml.Serialization;

namespace SoaEcommerce.Contracts;

[ServiceContract(Namespace = "urn:soa-ecommerce:v1:sales")]
public interface ISalesService
{
    [OperationContract]
    CreateOrderResponse CreateOrder(CreateOrderRequest request);

    [OperationContract]
    ConfirmOrderResponse ConfirmOrder(ConfirmOrderRequest request);

    [OperationContract]
    CancelOrderResponse CancelOrder(CancelOrderRequest request);

    [OperationContract]
    GetOrderResponse GetOrder(GetOrderRequest request);
}

[XmlRoot("CreateOrderRequest", Namespace = "urn:soa-ecommerce:v1:sales")]
public class CreateOrderRequest
{
    [XmlElement("CustomerId")]
    public Guid CustomerId { get; set; }

    [XmlArray("Items"), XmlArrayItem("Item")]
    public List<OrderItemInput> Items { get; set; } = new();
}

[XmlRoot("OrderItemInput", Namespace = "urn:soa-ecommerce:v1:sales")]
public class OrderItemInput
{
    [XmlElement("ProductId")]
    public Guid ProductId { get; set; }

    [XmlElement("Quantity")]
    public int Quantity { get; set; }

    [XmlElement("UnitPrice")]
    public decimal UnitPrice { get; set; }
}

[XmlRoot("CreateOrderResponse", Namespace = "urn:soa-ecommerce:v1:sales")]
public class CreateOrderResponse
{
    [XmlElement("OrderId")]
    public Guid OrderId { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
}

[XmlRoot("ConfirmOrderRequest", Namespace = "urn:soa-ecommerce:v1:sales")]
public class ConfirmOrderRequest
{
    [XmlElement("OrderId")]
    public Guid OrderId { get; set; }
}

[XmlRoot("ConfirmOrderResponse", Namespace = "urn:soa-ecommerce:v1:sales")]
public class ConfirmOrderResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
}

[XmlRoot("CancelOrderRequest", Namespace = "urn:soa-ecommerce:v1:sales")]
public class CancelOrderRequest
{
    [XmlElement("OrderId")]
    public Guid OrderId { get; set; }

    [XmlElement("Reason")]
    public string Reason { get; set; } = string.Empty;
}

[XmlRoot("CancelOrderResponse", Namespace = "urn:soa-ecommerce:v1:sales")]
public class CancelOrderResponse
{
    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
}

[XmlRoot("GetOrderRequest", Namespace = "urn:soa-ecommerce:v1:sales")]
public class GetOrderRequest
{
    [XmlElement("OrderId")]
    public Guid OrderId { get; set; }
}

[XmlRoot("GetOrderResponse", Namespace = "urn:soa-ecommerce:v1:sales")]
public class GetOrderResponse
{
    [XmlElement("Order")]
    public OrderDto? Order { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }
}

[XmlRoot("Order", Namespace = "urn:soa-ecommerce:v1:sales")]
public class OrderDto
{
    [XmlElement("Id")]
    public Guid Id { get; set; }

    [XmlElement("CustomerId")]
    public Guid CustomerId { get; set; }

    [XmlElement("Status")]
    public OrderStatus Status { get; set; }

    [XmlElement("Total")]
    public decimal Total { get; set; }

    [XmlElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [XmlArray("Items"), XmlArrayItem("Item")]
    public List<OrderItemDto> Items { get; set; } = new();
}

[XmlRoot("OrderItem", Namespace = "urn:soa-ecommerce:v1:sales")]
public class OrderItemDto
{
    [XmlElement("ProductId")]
    public Guid ProductId { get; set; }

    [XmlElement("Quantity")]
    public int Quantity { get; set; }

    [XmlElement("UnitPrice")]
    public decimal UnitPrice { get; set; }

    [XmlElement("Subtotal")]
    public decimal Subtotal { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Canceled
}
