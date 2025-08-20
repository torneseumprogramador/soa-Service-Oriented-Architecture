using CoreWCF;

namespace SoaEcommerce.Contracts;

public static class Faults
{
    public static FaultException<ServiceFault> InvalidCustomer()
    {
        var fault = new ServiceFault
        {
            Code = "INVALID_CUSTOMER",
            Message = "Cliente não encontrado ou inativo",
            Details = "O cliente especificado não existe ou está bloqueado"
        };
        return new FaultException<ServiceFault>(fault, new FaultReason("Cliente inválido"));
    }

    public static FaultException<ServiceFault> InsufficientStock(List<string> issues)
    {
        var fault = new ServiceFault
        {
            Code = "INSUFFICIENT_STOCK",
            Message = "Estoque insuficiente para um ou mais produtos",
            Details = string.Join("; ", issues)
        };
        return new FaultException<ServiceFault>(fault, new FaultReason("Estoque insuficiente"));
    }

    public static FaultException<ServiceFault> PaymentFailed()
    {
        var fault = new ServiceFault
        {
            Code = "PAYMENT_FAILED",
            Message = "Falha no processamento do pagamento",
            Details = "O pagamento foi rejeitado pelo processador"
        };
        return new FaultException<ServiceFault>(fault, new FaultReason("Falha no pagamento"));
    }

    public static FaultException<ServiceFault> ProductNotFound(Guid productId)
    {
        var fault = new ServiceFault
        {
            Code = "PRODUCT_NOT_FOUND",
            Message = "Produto não encontrado",
            Details = $"Produto com ID {productId} não foi encontrado"
        };
        return new FaultException<ServiceFault>(fault, new FaultReason("Produto não encontrado"));
    }

    public static FaultException<ServiceFault> OrderNotFound(Guid orderId)
    {
        var fault = new ServiceFault
        {
            Code = "ORDER_NOT_FOUND",
            Message = "Pedido não encontrado",
            Details = $"Pedido com ID {orderId} não foi encontrado"
        };
        return new FaultException<ServiceFault>(fault, new FaultReason("Pedido não encontrado"));
    }

    public static FaultException<ServiceFault> InvalidRequest(string details)
    {
        var fault = new ServiceFault
        {
            Code = "INVALID_REQUEST",
            Message = "Requisição inválida",
            Details = details
        };
        return new FaultException<ServiceFault>(fault, new FaultReason("Requisição inválida"));
    }
}

[System.Xml.Serialization.XmlRoot("ServiceFault", Namespace = "urn:soa-ecommerce:v1:faults")]
public class ServiceFault
{
    [System.Xml.Serialization.XmlElement("Code")]
    public string Code { get; set; } = string.Empty;

    [System.Xml.Serialization.XmlElement("Message")]
    public string Message { get; set; } = string.Empty;

    [System.Xml.Serialization.XmlElement("Details")]
    public string Details { get; set; } = string.Empty;

    [System.Xml.Serialization.XmlElement("Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
