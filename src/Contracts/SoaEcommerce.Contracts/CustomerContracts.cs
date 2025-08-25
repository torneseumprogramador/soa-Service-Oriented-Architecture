using CoreWCF;
using System.Xml.Serialization;
using System.Runtime.Serialization;

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

    [OperationContract]
    GetCustomerByEmailResponse GetCustomerByEmail(GetCustomerByEmailRequest request);
}

[XmlRoot("CreateCustomerRequest", Namespace = "urn:soa-ecommerce:v1:customers")]
[DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts")]
public class CreateCustomerRequest
{
    [XmlElement("Email")]
    [DataMember(Order = 1)]
    public string Email { get; set; } = string.Empty;

    [XmlElement("Name")]
    [DataMember(Order = 2)]
    public string Name { get; set; } = string.Empty;
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

    [XmlElement("Customer")]
    public CustomerDto? Customer { get; set; }
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

[XmlRoot("GetCustomerByEmailRequest", Namespace = "urn:soa-ecommerce:v1:customers")]
public class GetCustomerByEmailRequest
{
    [XmlElement("Email")]
    public string Email { get; set; } = string.Empty;
}

[XmlRoot("GetCustomerByEmailResponse", Namespace = "urn:soa-ecommerce:v1:customers")]
public class GetCustomerByEmailResponse
{
    [XmlElement("Customer")]
    public CustomerDto? Customer { get; set; }

    [XmlElement("Success")]
    public bool Success { get; set; }

    [XmlElement("Message")]
    public string Message { get; set; } = string.Empty;
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
