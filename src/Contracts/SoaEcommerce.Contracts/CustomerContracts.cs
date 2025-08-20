using CoreWCF;
using System.Xml.Serialization;

namespace SoaEcommerce.Contracts;

[ServiceContract(Namespace = "urn:soa-ecommerce:v1:customers")]
public interface ICustomerService
{
    [OperationContract]
    CreateCustomerResponse CreateCustomer(CreateCustomerRequest request);

    [OperationContract]
    GetCustomerResponse GetCustomer(GetCustomerRequest request);

    [OperationContract]
    GetCustomerStatusResponse GetCustomerStatus(GetCustomerStatusRequest request);
}

[XmlRoot("CreateCustomerRequest", Namespace = "urn:soa-ecommerce:v1:customers")]
public class CreateCustomerRequest
{
    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Email")]
    public string Email { get; set; } = string.Empty;
}

[XmlRoot("CreateCustomerResponse", Namespace = "urn:soa-ecommerce:v1:customers")]
public class CreateCustomerResponse
{
    [XmlElement("CustomerId")]
    public Guid CustomerId { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
}

[XmlRoot("GetCustomerRequest", Namespace = "urn:soa-ecommerce:v1:customers")]
public class GetCustomerRequest
{
    [XmlElement("CustomerId")]
    public Guid CustomerId { get; set; }
}

[XmlRoot("GetCustomerResponse", Namespace = "urn:soa-ecommerce:v1:customers")]
public class GetCustomerResponse
{
    [XmlElement("Customer")]
    public CustomerDto? Customer { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }
}

[XmlRoot("GetCustomerStatusRequest", Namespace = "urn:soa-ecommerce:v1:customers")]
public class GetCustomerStatusRequest
{
    [XmlElement("CustomerId")]
    public Guid CustomerId { get; set; }
}

[XmlRoot("GetCustomerStatusResponse", Namespace = "urn:soa-ecommerce:v1:customers")]
public class GetCustomerStatusResponse
{
    [XmlElement("IsActive")]
    public bool IsActive { get; set; }

    [XmlElement("Score")]
    public int Score { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }
}

[XmlRoot("Customer", Namespace = "urn:soa-ecommerce:v1:customers")]
public class CustomerDto
{
    [XmlElement("Id")]
    public Guid Id { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Email")]
    public string Email { get; set; } = string.Empty;

    [XmlElement("Status")]
    public CustomerStatus Status { get; set; }

    [XmlElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}

public enum CustomerStatus
{
    Active,
    Blocked
}
