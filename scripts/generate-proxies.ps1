# Script PowerShell para gerar proxies SOAP

Write-Host "Gerando proxies SOAP..." -ForegroundColor Green

# Criar diretório de saída
New-Item -ItemType Directory -Force -Path "src/Clients/SoaEcommerce.Clients/Generated"

# Gerar proxy para CustomerService
Write-Host "Gerando proxy para CustomerService..." -ForegroundColor Yellow
dotnet-svcutil http://localhost:7001/soap?wsdl `
  -n "*,SoaEcommerce.Clients.Customer" `
  -o src/Clients/SoaEcommerce.Clients/Generated/CustomerServiceProxy.cs `
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

# Gerar proxy para CatalogService
Write-Host "Gerando proxy para CatalogService..." -ForegroundColor Yellow
dotnet-svcutil http://localhost:7002/soap?wsdl `
  -n "*,SoaEcommerce.Clients.Catalog" `
  -o src/Clients/SoaEcommerce.Clients/Generated/CatalogServiceProxy.cs `
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

# Gerar proxy para SalesService
Write-Host "Gerando proxy para SalesService..." -ForegroundColor Yellow
dotnet-svcutil http://localhost:7003/soap?wsdl `
  -n "*,SoaEcommerce.Clients.Sales" `
  -o src/Clients/SoaEcommerce.Clients/Generated/SalesServiceProxy.cs `
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

# Gerar proxy para CompositionService
Write-Host "Gerando proxy para CompositionService..." -ForegroundColor Yellow
dotnet-svcutil http://localhost:7000/soap?wsdl `
  -n "*,SoaEcommerce.Clients.Composition" `
  -o src/Clients/SoaEcommerce.Clients/Generated/CompositionServiceProxy.cs `
  -r src/Contracts/SoaEcommerce.Contracts/bin/Debug/net8.0/SoaEcommerce.Contracts.dll

Write-Host "Proxies SOAP gerados com sucesso!" -ForegroundColor Green
