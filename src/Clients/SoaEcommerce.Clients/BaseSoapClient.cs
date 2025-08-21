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

    private string CreateSoapEnvelope(string operationName, object request, string correlationId)
    {
        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(request.GetType());
        var stringWriter = new StringWriter();
        xmlSerializer.Serialize(stringWriter, request);
        var requestXml = stringWriter.ToString();

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

    private T ParseSoapResponse<T>(string responseContent, string operationName)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(responseContent);

        var responseElement = xmlDoc.SelectSingleNode($"//*[local-name()='{operationName}Response']");
        if (responseElement == null)
        {
            throw new InvalidOperationException($"Response element '{operationName}Response' not found");
        }

        var responseType = typeof(T);
        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(responseType);
        var stringReader = new StringReader(responseElement.OuterXml);
        return (T)xmlSerializer.Deserialize(stringReader);
    }

    protected abstract string GetServiceNamespace();
    protected abstract string GetServiceName();
}
