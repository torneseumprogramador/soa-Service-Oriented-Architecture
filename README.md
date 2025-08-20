# SOA E-commerce - Arquitetura Orientada a Serviços Clássica

Este projeto implementa uma solução **SOA clássica** com **SOAP/WSDL/XSD** para um sistema de e-commerce, seguindo os princípios de arquitetura orientada a serviços tradicionais.

## 🏗️ Arquitetura

### Serviços Coarse-Grained

1. **CustomerService** (Porta 7001) - Gerenciamento de clientes
2. **CatalogService** (Porta 7002) - Catálogo de produtos e estoque
3. **SalesService** (Porta 7003) - Vendas e pedidos
4. **CompositionService** (Porta 7000) - Orquestração de processos

### Tecnologias

- **.NET 9** com **CoreWCF** para serviços SOAP
- **PostgreSQL** (3 bancos separados)
- **Entity Framework Core** para persistência
- **Polly** para resiliência (retry, circuit breaker)
- **Serilog** para observabilidade
- **Docker Compose** para infraestrutura

## 🚀 Como Executar

### Pré-requisitos

- .NET 9 SDK
- Docker e Docker Compose
- PostgreSQL (opcional para desenvolvimento local)
- Make (opcional, mas recomendado)

### 🎯 Execução Rápida com Makefile

```bash
# Clonar o repositório
git clone <repository-url>
cd soa-Service-Oriented-Architecture

# Configurar ambiente e executar todos os serviços
make dev-setup
make run-all

# Ou usar Docker
make docker-up
```

### Comandos Essenciais do Makefile

```bash
make help          # Ver todos os comandos disponíveis
make status        # Verificar status dos serviços
make health-check  # Verificar saúde dos endpoints
make test-soap     # Executar testes SOAP
make stop-all      # Parar todos os serviços
```

> 📖 **Documentação completa do Makefile**: Veja [MAKEFILE_README.md](MAKEFILE_README.md) para todos os comandos e fluxos de trabalho.

### Execução Manual com Docker Compose

```bash
# Executar todos os serviços
docker-compose -f docker/docker-compose.yml up --build
```

### Execução Manual Local

```bash
# Restaurar dependências
dotnet restore

# Executar migrações dos bancos
dotnet ef database update --project src/CustomerService/SoaEcommerce.CustomerService
dotnet ef database update --project src/CatalogService/SoaEcommerce.CatalogService
dotnet ef database update --project src/SalesService/SoaEcommerce.SalesService

# Executar serviços (em terminais separados)
dotnet run --project src/CustomerService/SoaEcommerce.CustomerService
dotnet run --project src/CatalogService/SoaEcommerce.CatalogService
dotnet run --project src/SalesService/SoaEcommerce.SalesService
dotnet run --project src/CompositionService/SoaEcommerce.CompositionService
```

## 📋 Endpoints SOAP

### CustomerService
- **URL**: `http://localhost:7001/soap`
- **WSDL**: `http://localhost:7001/soap?wsdl`
- **Operações**:
  - `CreateCustomer`
  - `GetCustomer`
  - `GetCustomerStatus`

### CatalogService
- **URL**: `http://localhost:7002/soap`
- **WSDL**: `http://localhost:7002/soap?wsdl`
- **Operações**:
  - `CreateProduct`
  - `GetProduct`
  - `ReserveInventory`
  - `ReleaseInventory`

### SalesService
- **URL**: `http://localhost:7003/soap`
- **WSDL**: `http://localhost:7003/soap?wsdl`
- **Operações**:
  - `CreateOrder`
  - `ConfirmOrder`
  - `CancelOrder`
  - `GetOrder`

### CompositionService
- **URL**: `http://localhost:7000/soap`
- **WSDL**: `http://localhost:7000/soap?wsdl`
- **Operações**:
  - `PlaceOrder` (orquestração completa)

## 🔐 Segurança

### WS-Security (Básico)

Todos os serviços implementam validação de **ApiKey** via header SOAP:

```xml
<soap:Header>
  <AuthHeader xmlns="urn:soa-ecommerce:v1:auth">
    <ApiKey>dev</ApiKey>
  </AuthHeader>
  <CorrelationId xmlns="urn:soa-ecommerce:v1:headers">12345</CorrelationId>
</soap:Header>
```

## 🔄 Orquestração - PlaceOrder

O `CompositionService.PlaceOrder` implementa o fluxo completo:

1. **Validação de Cliente** → `GetCustomerStatus`
2. **Reserva de Estoque** → `ReserveInventory` (lote)
3. **Criação de Pedido** → `CreateOrder`
4. **Processamento de Pagamento** → Mock (90% sucesso)
5. **Confirmação** → `ConfirmOrder`
6. **Compensação** → Em caso de falha: `ReleaseInventory` + `CancelOrder`

## 🛠️ Geração de Clientes SOAP

### Scripts Automatizados

```bash
# Linux/macOS
chmod +x scripts/generate-proxies.sh
./scripts/generate-proxies.sh

# Windows PowerShell
.\scripts\generate-proxies.ps1
```

### Comando Manual

```bash
# Gerar proxy para CustomerService
dotnet-svcutil http://localhost:7001/soap?wsdl \
  -n "*,SoaEcommerce.Clients.Customer" \
  -o CustomerServiceProxy.cs
```

## 🧪 Testes

```bash
# Executar testes
dotnet test tests/CompositionService.Tests/SoaEcommerce.CompositionService.Tests/

# Cenários testados:
# - PlaceOrder com sucesso
# - PlaceOrder com estoque insuficiente
# - PlaceOrder com cliente inválido
```

## 📊 Observabilidade

### Logs

- **Serilog** configurado em todos os serviços
- Logs em arquivo: `logs/{service-name}-{date}.txt`
- **CorrelationId** propagado via header SOAP

### Health Checks

```bash
# Verificar saúde dos serviços
curl http://localhost:7000/health
curl http://localhost:7001/health
curl http://localhost:7002/health
curl http://localhost:7003/health
```

## 🗄️ Bancos de Dados

### PostgreSQL (3 instâncias)

- **pg-customers** (Porta 5432) - `customers_db`
- **pg-catalog** (Porta 5433) - `catalog_db`
- **pg-sales** (Porta 5434) - `sales_db`

### Dados de Seed

- **2 Clientes** pré-cadastrados
- **3 Produtos** pré-cadastrados
- Migrações automáticas ao iniciar

## 📁 Estrutura do Projeto

```
soa-Service-Oriented-Architecture/
├── src/
│   ├── Contracts/                    # DataContracts comuns
│   ├── CustomerService/              # Serviço de clientes
│   ├── CatalogService/               # Serviço de catálogo
│   ├── SalesService/                 # Serviço de vendas
│   ├── CompositionService/           # Orquestração
│   └── Clients/                      # Proxies SOAP resilientes
├── docker/
│   └── docker-compose.yml           # Infraestrutura
├── tests/
│   └── CompositionService.Tests/    # Testes end-to-end
├── scripts/
│   ├── generate-proxies.sh          # Scripts de geração
│   └── generate-proxies.ps1
├── services-registry.json           # Registro de serviços
├── Makefile                         # Comandos automatizados
├── MAKEFILE_README.md               # Documentação do Makefile
└── README.md
```

## 🔧 Configuração

### Variáveis de Ambiente

```json
{
  "ServiceRegistry": {
    "CustomerService": "http://localhost:7001/soap",
    "CatalogService": "http://localhost:7002/soap",
    "SalesService": "http://localhost:7003/soap"
  }
}
```

### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=customers_db;Username=postgres;Password=postgres"
  }
}
```

## 🚨 Resiliência

### Polly Policies

- **Retry**: 3 tentativas com backoff exponencial
- **Circuit Breaker**: Abre após 5 falhas, fecha após 30s
- **Timeout**: 30 segundos por operação

## 📝 Exemplo de Uso

### Envelope SOAP para PlaceOrder

```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Header>
    <AuthHeader xmlns="urn:soa-ecommerce:v1:auth">
      <ApiKey>dev</ApiKey>
    </AuthHeader>
    <CorrelationId xmlns="urn:soa-ecommerce:v1:headers">12345</CorrelationId>
  </soap:Header>
  <soap:Body>
    <PlaceOrder xmlns="urn:soa-ecommerce:v1:process">
      <CustomerId>11111111-1111-1111-1111-111111111111</CustomerId>
      <Items>
        <Item>
          <ProductId>33333333-3333-3333-3333-333333333333</ProductId>
          <Quantity>1</Quantity>
        </Item>
      </Items>
    </PlaceOrder>
  </soap:Body>
</soap:Envelope>
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature
3. Commit suas mudanças
4. Push para a branch
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.
