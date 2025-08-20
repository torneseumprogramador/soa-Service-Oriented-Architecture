using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SoaEcommerce.Contracts;
using SoaEcommerce.CustomerService.Data;
using SoaEcommerce.CustomerService.Security;
using SoaEcommerce.CustomerService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/customer-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar Entity Framework
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar CoreWCF
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<ServiceMetadataBehavior>(_ => new ServiceMetadataBehavior { HttpGetEnabled = true });

// Registrar serviços
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SoapSecurityInspector>();

var app = builder.Build();

// Configurar CoreWCF endpoints
app.UseServiceModel(sm =>
{
    sm.AddService<CustomerService>();
    
    // Configurar binding SOAP
    var binding = new BasicHttpBinding
    {
        MaxReceivedMessageSize = 65536,
        ReaderQuotas = { MaxStringContentLength = 8192 }
    };

    // Adicionar endpoint SOAP
    sm.AddServiceEndpoint<CustomerService, ICustomerService>(binding, "/soap");
});

// Migrar banco de dados
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    context.Database.Migrate();
}

Log.Information("CustomerService iniciado na porta 7001");
app.Run();
