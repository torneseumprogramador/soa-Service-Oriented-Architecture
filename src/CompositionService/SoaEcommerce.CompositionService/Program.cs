using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
using Serilog;
using SoaEcommerce.Contracts;
using SoaEcommerce.Clients;
using SoaEcommerce.CompositionService.Security;
using SoaEcommerce.CompositionService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/composition-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar CoreWCF
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<ServiceMetadataBehavior>(_ => new ServiceMetadataBehavior { HttpGetEnabled = true });

// Registrar serviços
builder.Services.AddScoped<CompositionService>();
builder.Services.AddScoped<SoapSecurityInspector>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Configurar clients SOAP resilientes
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICustomerClient, CustomerClient>();
builder.Services.AddScoped<ICatalogClient, CatalogClient>();
builder.Services.AddScoped<ISalesClient, SalesClient>();

var app = builder.Build();

// Configurar CoreWCF endpoints
app.UseServiceModel(sm =>
{
    sm.AddService<CompositionService>();
    
    // Configurar binding SOAP
    var binding = new BasicHttpBinding
    {
        MaxReceivedMessageSize = 65536,
        ReaderQuotas = { MaxStringContentLength = 8192 }
    };

    // Adicionar endpoint SOAP
    sm.AddServiceEndpoint<CompositionService, ICompositionService>(binding, "/soap");
});

Log.Information("CompositionService iniciado na porta 7000");
app.Run();
