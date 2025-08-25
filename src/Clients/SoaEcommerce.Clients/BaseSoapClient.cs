using CoreWCF;
using CoreWCF.Channels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SoaEcommerce.Clients;

public abstract class BaseSoapClient
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly string _serviceUrl;
    protected readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    protected readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

    protected BaseSoapClient(HttpClient httpClient, ILogger logger, IConfiguration configuration, string serviceName)
    {
        _httpClient = httpClient;
        _logger = logger;
        _serviceUrl = configuration[$"ServiceRegistry:{serviceName}"] ?? throw new ArgumentException($"Service URL not found for {serviceName}");

        // Configurar políticas de resiliência
        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogWarning("Tentativa {RetryAttempt} falhou para {ServiceName}: {Error}", 
                        retryAttempt, serviceName, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });

        _circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Circuit breaker aberto para {ServiceName} por {Duration}s", serviceName, duration.TotalSeconds);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker fechado para {ServiceName}", serviceName);
                });
    }

    protected async Task<T> SendSoapRequestAsync<T>(string operationName, object request, string? correlationId = null)
    {
        var soapEnvelope = CreateSoapEnvelope(operationName, request, correlationId);
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", $"\"urn:soa-ecommerce:v1:{GetServiceNamespace()}/I{GetServiceName()}Service/{operationName}\"");

        var response = await _circuitBreakerPolicy
            .WrapAsync(_retryPolicy)
            .ExecuteAsync(async () => await _httpClient.PostAsync(_serviceUrl, content));

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"HTTP {response.StatusCode}: {response.ReasonPhrase}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        return ParseSoapResponse<T>(responseContent, operationName);
    }

    private string CreateSoapEnvelope(string operationName, object request, string? correlationId)
    {
        var requestXml = SerializeRequestParameter(request);

        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Header>
    <AuthHeader xmlns=""urn:soa-ecommerce:v1:auth"">
      <ApiKey>dev</ApiKey>
    </AuthHeader>";

        if (!string.IsNullOrEmpty(correlationId))
        {
            soapEnvelope += $@"
    <CorrelationId xmlns=""urn:soa-ecommerce:v1:headers"">{correlationId}</CorrelationId>";
        }

        soapEnvelope += $@"
  </soap:Header>
  <soap:Body>
    <{operationName} xmlns=""urn:soa-ecommerce:v1:{GetServiceNamespace()}"">
      {requestXml}
    </{operationName}>
  </soap:Body>
</soap:Envelope>";

        return soapEnvelope;
    }

    private string SerializeRequestParameter(object request)
    {
        var requestType = request.GetType();
        var serviceNamespace = $"urn:soa-ecommerce:v1:{GetServiceNamespace()}";
        var dataContractNamespace = $"http://schemas.datacontract.org/2004/07/{requestType.Namespace}";

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Encoding = new UTF8Encoding(false)
        };

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, settings);

        xmlWriter.WriteStartElement("request", serviceNamespace);
        xmlWriter.WriteAttributeString("xmlns", "q1", null, dataContractNamespace);

        // Serialização especializada para tipos conhecidos com coleções
        if (requestType.Name == "ReserveInventoryRequest")
        {
            var linesProp = requestType.GetProperty("Lines");
            var lines = linesProp?.GetValue(request) as System.Collections.IEnumerable;
            xmlWriter.WriteStartElement("q1", "Lines", dataContractNamespace);
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    var lineType = line!.GetType();
                    xmlWriter.WriteStartElement("q1", "Line", dataContractNamespace);
                    var productId = lineType.GetProperty("ProductId")?.GetValue(line)?.ToString();
                    var quantity = lineType.GetProperty("Quantity")?.GetValue(line)?.ToString();
                    if (!string.IsNullOrEmpty(productId))
                    {
                        xmlWriter.WriteElementString("q1", "ProductId", dataContractNamespace, productId);
                    }
                    if (!string.IsNullOrEmpty(quantity))
                    {
                        xmlWriter.WriteElementString("q1", "Quantity", dataContractNamespace, quantity);
                    }
                    xmlWriter.WriteEndElement(); // q1:Line
                }
            }
            xmlWriter.WriteEndElement(); // q1:Lines
        }
        else if (requestType.Name == "CreateOrderRequest")
        {
            // Campos simples
            var customerId = requestType.GetProperty("CustomerId")?.GetValue(request)?.ToString();
            if (!string.IsNullOrEmpty(customerId))
            {
                xmlWriter.WriteElementString("q1", "CustomerId", dataContractNamespace, customerId);
            }
            // Itens
            var itemsProp = requestType.GetProperty("Items");
            var items = itemsProp?.GetValue(request) as System.Collections.IEnumerable;
            xmlWriter.WriteStartElement("q1", "Items", dataContractNamespace);
            if (items != null)
            {
                foreach (var item in items)
                {
                    var itemType = item!.GetType();
                    xmlWriter.WriteStartElement("q1", "Item", dataContractNamespace);
                    var productId = itemType.GetProperty("ProductId")?.GetValue(item)?.ToString();
                    var quantity = itemType.GetProperty("Quantity")?.GetValue(item)?.ToString();
                    var unitPrice = itemType.GetProperty("UnitPrice")?.GetValue(item)?.ToString();
                    if (!string.IsNullOrEmpty(productId))
                    {
                        xmlWriter.WriteElementString("q1", "ProductId", dataContractNamespace, productId);
                    }
                    if (!string.IsNullOrEmpty(quantity))
                    {
                        xmlWriter.WriteElementString("q1", "Quantity", dataContractNamespace, quantity);
                    }
                    if (!string.IsNullOrEmpty(unitPrice))
                    {
                        xmlWriter.WriteElementString("q1", "UnitPrice", dataContractNamespace, unitPrice);
                    }
                    xmlWriter.WriteEndElement(); // q1:Item
                }
            }
            xmlWriter.WriteEndElement(); // q1:Items
        }
        else
        {
            // Fallback: propriedades simples
            foreach (var property in requestType.GetProperties())
            {
                var value = property.GetValue(request);
                if (value != null)
                {
                    xmlWriter.WriteElementString("q1", property.Name, dataContractNamespace, value.ToString());
                }
            }
        }

        xmlWriter.WriteEndElement(); // </request>
        xmlWriter.Flush();
        return stringWriter.ToString();
    }

    private T ParseSoapResponse<T>(string responseContent, string operationName)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(responseContent);

        var responseElement = xmlDoc.SelectSingleNode($"//*[local-name()='{operationName}Response']");
        if (responseElement == null)
        {
            throw new InvalidOperationException($"Response element '{operationName}Response' not found");
        }

        // Tenta desserializar o elemento ...Result quando presente
        var resultElement = responseElement.SelectSingleNode($"*[local-name()='{operationName}Result']");
        var nodeToDeserialize = resultElement ?? responseElement;
        _logger.LogInformation("SOAP Parse Start: op={Operation} hasResult={HasResult} nodeLocalName={Local}", operationName, resultElement != null, nodeToDeserialize.LocalName);

        // Tentar desserialização direta
        var responseType = typeof(T);
        try
        {
            var overrideRoot = new XmlRootAttribute(nodeToDeserialize.LocalName)
            {
                Namespace = nodeToDeserialize.NamespaceURI
            };
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(responseType, overrideRoot);
            using var stringReader = new StringReader(nodeToDeserialize.OuterXml);
            var direct = xmlSerializer.Deserialize(stringReader);
            if (direct is T directT)
            {
                // Validar coerência mínima; se divergente, usar fallback
                bool ExpectedSuccess()
                {
                    var successNode = nodeToDeserialize.SelectSingleNode(".//*[local-name()='Success']");
                    return successNode != null && bool.TryParse(successNode.InnerText, out var b) && b;
                }

                var propSuccess = responseType.GetProperty("Success");
                var propCustomer = responseType.GetProperty("Customer");
                bool hasCustomerNode = nodeToDeserialize.SelectSingleNode(".//*[local-name()='Customer']") != null;
                bool expectedSuccess = ExpectedSuccess();
                bool deserializedSuccess = propSuccess != null && propSuccess.PropertyType == typeof(bool) && (bool)(propSuccess.GetValue(directT) ?? false);
                bool deserializedCustomerNull = propCustomer != null && propCustomer.GetValue(directT) == null;

                if ((expectedSuccess && !deserializedSuccess) || (hasCustomerNode && deserializedCustomerNull))
                {
                    _logger.LogWarning("SOAP Parse Fallback: op={Operation} expectedSuccess={Expected} directSuccess={Direct} hasCustomerNode={HasCustomer} customerNull={CustNull}", operationName, expectedSuccess, deserializedSuccess, hasCustomerNode, deserializedCustomerNull);
                    return ParseByLocalName<T>(nodeToDeserialize);
                }
                _logger.LogInformation("SOAP Parse Direct OK: op={Operation} success={Success}", operationName, deserializedSuccess);
                return directT;
            }
        }
        catch
        {
            // fallback abaixo
        }

        // Fallback: parser tolerante a namespace usando local-name()
        _logger.LogWarning("SOAP Parse Fallback (exception): op={Operation}", operationName);
        return ParseByLocalName<T>(nodeToDeserialize);
    }

    private T ParseByLocalName<T>(XmlNode root)
    {
        var result = Activator.CreateInstance<T>();
        var type = typeof(T);

        // Helper local
        string? GetString(string name) => root.SelectSingleNode($".//*[local-name()='{name}']")?.InnerText;
        bool GetBool(string name) => bool.TryParse(GetString(name), out var b) && b;
        Guid GetGuid(string name) => Guid.TryParse(GetString(name), out var g) ? g : Guid.Empty;
        DateTime GetDateTime(string name) => DateTime.TryParse(GetString(name), out var d) ? d : default;

        // Preencher propriedades comuns de Response: Success, Message
        var propSuccess = type.GetProperty("Success");
        if (propSuccess != null && propSuccess.PropertyType == typeof(bool))
        {
            propSuccess.SetValue(result, GetBool("Success"));
        }
        var propMessage = type.GetProperty("Message");
        if (propMessage != null)
        {
            propMessage.SetValue(result, GetString("Message"));
        }

        // Customer (quando existir)
        var propCustomer = type.GetProperty("Customer");
        if (propCustomer != null)
        {
            var customerNode = root.SelectSingleNode($".//*[local-name()='Customer']");
            if (customerNode != null)
            {
                var customerObj = Activator.CreateInstance(propCustomer.PropertyType);
                propCustomer.PropertyType.GetProperty("Id")?.SetValue(customerObj, GetGuid("Id"));
                propCustomer.PropertyType.GetProperty("Name")?.SetValue(customerObj, GetString("Name") ?? string.Empty);
                propCustomer.PropertyType.GetProperty("Email")?.SetValue(customerObj, GetString("Email") ?? string.Empty);
                var statusProp = propCustomer.PropertyType.GetProperty("Status");
                var statusStr = GetString("Status");
                if (statusProp != null && statusStr != null && Enum.TryParse(statusProp.PropertyType, statusStr, out var statusEnum))
                {
                    statusProp.SetValue(customerObj, statusEnum);
                }
                var createdAtProp = propCustomer.PropertyType.GetProperty("CreatedAt");
                if (createdAtProp != null)
                {
                    createdAtProp.SetValue(customerObj, GetDateTime("CreatedAt"));
                }
                propCustomer.SetValue(result, customerObj);
            }
        }

        // Order (quando existir)
        var propOrder = type.GetProperty("Order");
        if (propOrder != null)
        {
            var orderNode = root.SelectSingleNode($".//*[local-name()='Order']");
            if (orderNode != null)
            {
                var orderObj = Activator.CreateInstance(propOrder.PropertyType);
                propOrder.PropertyType.GetProperty("Id")?.SetValue(orderObj, GetGuid("Id"));
                propOrder.PropertyType.GetProperty("CustomerId")?.SetValue(orderObj, GetGuid("CustomerId"));
                propOrder.PropertyType.GetProperty("Total")?.SetValue(orderObj, decimal.TryParse(GetString("Total"), out var dec) ? dec : 0m);
                var statusProp = propOrder.PropertyType.GetProperty("Status");
                var statusStr = GetString("Status");
                if (statusProp != null && statusStr != null && Enum.TryParse(statusProp.PropertyType, statusStr, out var statusEnum))
                {
                    statusProp.SetValue(orderObj, statusEnum);
                }

                // Items
                var itemsProp = propOrder.PropertyType.GetProperty("Items");
                var itemsNodeList = root.SelectNodes($".//*[local-name()='OrderItemDto']");
                if (itemsProp != null && itemsNodeList != null)
                {
                    var list = (System.Collections.IList)itemsProp.GetValue(orderObj)!;
                    foreach (XmlNode item in itemsNodeList)
                    {
                        var itemObj = Activator.CreateInstance(itemsProp.PropertyType.GenericTypeArguments[0]);
                        itemObj!.GetType().GetProperty("ProductId")?.SetValue(itemObj, Guid.TryParse(item.SelectSingleNode($".//*[local-name()='ProductId']")?.InnerText, out var pg) ? pg : Guid.Empty);
                        itemObj.GetType().GetProperty("Quantity")?.SetValue(itemObj, int.TryParse(item.SelectSingleNode($".//*[local-name()='Quantity']")?.InnerText, out var q) ? q : 0);
                        itemObj.GetType().GetProperty("UnitPrice")?.SetValue(itemObj, decimal.TryParse(item.SelectSingleNode($".//*[local-name()='UnitPrice']")?.InnerText, out var up) ? up : 0m);
                        itemObj.GetType().GetProperty("Subtotal")?.SetValue(itemObj, decimal.TryParse(item.SelectSingleNode($".//*[local-name()='Subtotal']")?.InnerText, out var st) ? st : 0m);
                        list.Add(itemObj);
                    }
                }

                propOrder.SetValue(result, orderObj);
            }
        }

        // Ids simples (ex.: CustomerId, OrderId)
        var propCustomerId = type.GetProperty("CustomerId");
        propCustomerId?.SetValue(result, GetGuid("CustomerId"));
        var propOrderId = type.GetProperty("OrderId");
        propOrderId?.SetValue(result, GetGuid("OrderId"));

        return result;
    }

    protected abstract string GetServiceNamespace();
    protected abstract string GetServiceName();
}
