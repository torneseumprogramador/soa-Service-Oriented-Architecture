#!/bin/bash

echo "Gerando proxies SOAP..."

# Criar diretório de saída
mkdir -p src/Clients/SoaEcommerce.Clients/Generated

# Gerar proxy para CustomerService
echo "Gerando proxy para CustomerService..."
dotnet-svcutil http://localhost:7001/soap?wsdl \
  -n "*,SoaEcommerce.Clients.Customer" \
  -o src/Clients/SoaEcommerce.Clients/Generated/CustomerServiceProxy.cs \
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

# Gerar proxy para CatalogService
echo "Gerando proxy para CatalogService..."
dotnet-svcutil http://localhost:7002/soap?wsdl \
  -n "*,SoaEcommerce.Clients.Catalog" \
  -o src/Clients/SoaEcommerce.Clients/Generated/CatalogServiceProxy.cs \
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

# Gerar proxy para SalesService
echo "Gerando proxy para SalesService..."
dotnet-svcutil http://localhost:7003/soap?wsdl \
  -n "*,SoaEcommerce.Clients.Sales" \
  -o src/Clients/SoaEcommerce.Clients/Generated/SalesServiceProxy.cs \
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

# Gerar proxy para CompositionService
echo "Gerando proxy para CompositionService..."
dotnet-svcutil http://localhost:7000/soap?wsdl \
  -n "*,SoaEcommerce.Clients.Composition" \
  -o src/Clients/SoaEcommerce.Clients/Generated/CompositionServiceProxy.cs \
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

echo "Proxies SOAP gerados com sucesso!"
