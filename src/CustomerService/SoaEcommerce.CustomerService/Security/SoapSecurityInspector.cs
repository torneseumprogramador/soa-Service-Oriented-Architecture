using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Dispatcher;
using System.Xml;

namespace SoaEcommerce.CustomerService.Security;

public class SoapSecurityInspector : IDispatchMessageInspector
{
    private readonly ILogger<SoapSecurityInspector> _logger;

    public SoapSecurityInspector(ILogger<SoapSecurityInspector> logger)
    {
        _logger = logger;
    }

    public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
    {
        _logger.LogInformation("Validando segurança SOAP para operação: {Action}", request.Headers.Action);

        // Extrair CorrelationId do header SOAP
        var correlationId = ExtractCorrelationId(request);
        if (!string.IsNullOrEmpty(correlationId))
        {
            _logger.LogInformation("CorrelationId: {CorrelationId}", correlationId);
        }

        // Validar ApiKey do header SOAP
        var apiKey = ExtractApiKey(request);
        if (string.IsNullOrEmpty(apiKey) || apiKey != "dev")
        {
            _logger.LogWarning("ApiKey inválida ou ausente: {ApiKey}", apiKey);
            throw new FaultException("ApiKey inválida ou ausente");
        }

        return correlationId ?? Guid.NewGuid().ToString();
    }

    public void BeforeSendReply(ref Message reply, object correlationState)
    {
        var correlationId = correlationState as string;
        if (!string.IsNullOrEmpty(correlationId))
        {
            _logger.LogInformation("Respondendo com CorrelationId: {CorrelationId}", correlationId);
        }
    }

    private string? ExtractCorrelationId(Message request)
    {
        try
        {
            var headerIndex = request.Headers.FindHeader("CorrelationId", "urn:soa-ecommerce:v1:headers");
            if (headerIndex >= 0)
            {
                var reader = request.Headers.GetReaderAtHeader(headerIndex);
                return reader.ReadElementContentAsString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao extrair CorrelationId");
        }
        return null;
    }

    private string? ExtractApiKey(Message request)
    {
        try
        {
            var headerIndex = request.Headers.FindHeader("ApiKey", "urn:soa-ecommerce:v1:auth");
            if (headerIndex >= 0)
            {
                var reader = request.Headers.GetReaderAtHeader(headerIndex);
                return reader.ReadElementContentAsString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao extrair ApiKey");
        }
        return null;
    }
}
