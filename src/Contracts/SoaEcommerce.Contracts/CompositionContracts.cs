using CoreWCF;
using System.Xml.Serialization;

namespace SoaEcommerce.Contracts;

[ServiceContract(Namespace = "urn:soa-ecommerce:v1:process")]
public interface ICompositionService
{
    [OperationContract]
    PlaceOrderResponse PlaceOrder(PlaceOrderRequest request);
}

[XmlRoot("PlaceOrderRequest", Namespace = "urn:soa-ecommerce:v1:process")]
public class PlaceOrderRequest
{
    [XmlElement("CustomerEmail")]
    public string CustomerEmail { get; set; } = string.Empty;

    [XmlArray("Items"), XmlArrayItem("Item")]
    public List<PlaceOrderItem> Items { get; set; } = new();
}

[XmlRoot("PlaceOrderItem", Namespace = "urn:soa-ecommerce:v1:process")]
public class PlaceOrderItem
{
    [XmlElement("ProductId")]
    public Guid ProductId { get; set; }

    [XmlElement("Quantity")]
    public int Quantity { get; set; }
}

[XmlRoot("PlaceOrderResponse", Namespace = "urn:soa-ecommerce:v1:process")]
public class PlaceOrderResponse
{
    [XmlElement("Order")]
    public OrderDto? Order { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
}
