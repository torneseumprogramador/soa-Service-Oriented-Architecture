# Makefile para SOA E-commerce
# Arquitetura Orientada a Serviços com SOAP/WSDL

.PHONY: help build clean restore test docker-up docker-down docker-build docker-logs docker-clean
.PHONY: build-customer build-catalog build-sales build-composition build-contracts build-clients build-tests
.PHONY: clean-customer clean-catalog clean-sales clean-composition clean-contracts clean-clients clean-tests
.PHONY: run-all run-customer run-catalog run-sales run-composition
.PHONY: run-customer-bg run-catalog-bg run-sales-bg run-composition-bg
.PHONY: stop-all stop-customer stop-catalog stop-sales stop-composition
.PHONY: migrate-all migrate-customer migrate-catalog migrate-sales
.PHONY: generate-proxies test-customer test-catalog test-sales test-composition health-check logs
.PHONY: logs-customer logs-catalog logs-sales logs-composition

# Variáveis
DOCKER_COMPOSE_FILE = docker/docker-compose.yml
SOLUTION_FILE = SoaEcommerceSoap.sln

# Cores para output
GREEN = \033[0;32m
YELLOW = \033[1;33m
RED = \033[0;31m
BLUE = \033[0;34m
NC = \033[0m # No Color

# Comando padrão
.DEFAULT_GOAL := help

help: ## Mostra esta ajuda
	@echo "$(BLUE)SOA E-commerce - Comandos Disponíveis$(NC)"
	@echo "=================================="
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "$(GREEN)%-20s$(NC) %s\n", $$1, $$2}'

# =============================================================================
# DESENVOLVIMENTO LOCAL
# =============================================================================

restore: ## Restaura dependências do .NET
	@echo "$(YELLOW)🔄 Restaurando dependências...$(NC)"
	dotnet restore $(SOLUTION_FILE)

build: restore ## Compila a solução
	@echo "$(YELLOW)🔨 Compilando solução...$(NC)"
	dotnet build $(SOLUTION_FILE) --configuration Release

build-customer: ## Compila apenas o CustomerService
	@echo "$(YELLOW)🔨 Compilando CustomerService...$(NC)"
	dotnet build src/CustomerService/SoaEcommerce.CustomerService --configuration Release

build-catalog: ## Compila apenas o CatalogService
	@echo "$(YELLOW)🔨 Compilando CatalogService...$(NC)"
	dotnet build src/CatalogService/SoaEcommerce.CatalogService --configuration Release

build-sales: ## Compila apenas o SalesService
	@echo "$(YELLOW)🔨 Compilando SalesService...$(NC)"
	dotnet build src/SalesService/SoaEcommerce.SalesService --configuration Release

build-composition: ## Compila apenas o CompositionService
	@echo "$(YELLOW)🔨 Compilando CompositionService...$(NC)"
	dotnet build src/CompositionService/SoaEcommerce.CompositionService --configuration Release

build-contracts: ## Compila apenas os Contracts
	@echo "$(YELLOW)🔨 Compilando Contracts...$(NC)"
	dotnet build src/Contracts/SoaEcommerce.Contracts --configuration Release

build-clients: ## Compila apenas os Clients
	@echo "$(YELLOW)🔨 Compilando Clients...$(NC)"
	dotnet build src/Clients/SoaEcommerce.Clients --configuration Release

build-tests: ## Compila apenas os Tests
	@echo "$(YELLOW)🔨 Compilando Tests...$(NC)"
	dotnet build tests/CompositionService.Tests/SoaEcommerce.CompositionService.Tests --configuration Release

clean: ## Limpa arquivos de build
	@echo "$(YELLOW)🧹 Limpando arquivos de build...$(NC)"
	dotnet clean $(SOLUTION_FILE)
	find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
	find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

clean-customer: ## Limpa apenas o CustomerService
	@echo "$(YELLOW)🧹 Limpando CustomerService...$(NC)"
	dotnet clean src/CustomerService/SoaEcommerce.CustomerService
	rm -rf src/CustomerService/SoaEcommerce.CustomerService/bin 2>/dev/null || true
	rm -rf src/CustomerService/SoaEcommerce.CustomerService/obj 2>/dev/null || true

clean-catalog: ## Limpa apenas o CatalogService
	@echo "$(YELLOW)🧹 Limpando CatalogService...$(NC)"
	dotnet clean src/CatalogService/SoaEcommerce.CatalogService
	rm -rf src/CatalogService/SoaEcommerce.CatalogService/bin 2>/dev/null || true
	rm -rf src/CatalogService/SoaEcommerce.CatalogService/obj 2>/dev/null || true

clean-sales: ## Limpa apenas o SalesService
	@echo "$(YELLOW)🧹 Limpando SalesService...$(NC)"
	dotnet clean src/SalesService/SoaEcommerce.SalesService
	rm -rf src/SalesService/SoaEcommerce.SalesService/bin 2>/dev/null || true
	rm -rf src/SalesService/SoaEcommerce.SalesService/obj 2>/dev/null || true

clean-composition: ## Limpa apenas o CompositionService
	@echo "$(YELLOW)🧹 Limpando CompositionService...$(NC)"
	dotnet clean src/CompositionService/SoaEcommerce.CompositionService
	rm -rf src/CompositionService/SoaEcommerce.CompositionService/bin 2>/dev/null || true
	rm -rf src/CompositionService/SoaEcommerce.CompositionService/obj 2>/dev/null || true

clean-contracts: ## Limpa apenas os Contracts
	@echo "$(YELLOW)🧹 Limpando Contracts...$(NC)"
	dotnet clean src/Contracts/SoaEcommerce.Contracts
	rm -rf src/Contracts/SoaEcommerce.Contracts/bin 2>/dev/null || true
	rm -rf src/Contracts/SoaEcommerce.Contracts/obj 2>/dev/null || true

clean-clients: ## Limpa apenas os Clients
	@echo "$(YELLOW)🧹 Limpando Clients...$(NC)"
	dotnet clean src/Clients/SoaEcommerce.Clients
	rm -rf src/Clients/SoaEcommerce.Clients/bin 2>/dev/null || true
	rm -rf src/Clients/SoaEcommerce.Clients/obj 2>/dev/null || true

clean-tests: ## Limpa apenas os Tests
	@echo "$(YELLOW)🧹 Limpando Tests...$(NC)"
	dotnet clean tests/CompositionService.Tests/SoaEcommerce.CompositionService.Tests
	rm -rf tests/CompositionService.Tests/SoaEcommerce.CompositionService.Tests/bin 2>/dev/null || true
	rm -rf tests/CompositionService.Tests/SoaEcommerce.CompositionService.Tests/obj 2>/dev/null || true

# =============================================================================
# MIGRAÇÕES DE BANCO
# =============================================================================

migrate-all: migrate-customer migrate-catalog migrate-sales ## Executa migrações em todos os bancos

migrate-customer: ## Executa migrações do CustomerService
	@echo "$(YELLOW)🗄️ Executando migrações do CustomerService...$(NC)"
	dotnet ef database update --project src/CustomerService/SoaEcommerce.CustomerService

migrate-catalog: ## Executa migrações do CatalogService
	@echo "$(YELLOW)🗄️ Executando migrações do CatalogService...$(NC)"
	dotnet ef database update --project src/CatalogService/SoaEcommerce.CatalogService

migrate-sales: ## Executa migrações do SalesService
	@echo "$(YELLOW)🗄️ Executando migrações do SalesService...$(NC)"
	dotnet ef database update --project src/SalesService/SoaEcommerce.SalesService

# =============================================================================
# EXECUÇÃO LOCAL
# =============================================================================

run-all: ## Executa todos os serviços localmente (em background)
	@echo "$(YELLOW)🚀 Iniciando todos os serviços em background...$(NC)"
	@echo "$(BLUE)📝 Use 'make logs' para ver os logs$(NC)"
	@echo "$(BLUE)📝 Use 'make stop-all' para parar todos os serviços$(NC)"
	@echo "$(BLUE)📝 Use 'make status' para verificar status$(NC)"
	$(MAKE) run-customer-bg &
	$(MAKE) run-catalog-bg &
	$(MAKE) run-sales-bg &
	$(MAKE) run-composition-bg &
	@echo "$(GREEN)✅ Todos os serviços iniciados em background!$(NC)"
	@echo "$(BLUE)⏳ Aguardando inicialização...$(NC)"
	@sleep 5
	$(MAKE) status

run-customer: ## Executa CustomerService (foreground)
	@echo "$(YELLOW)👤 Iniciando CustomerService na porta 7001...$(NC)"
	dotnet run --project src/CustomerService/SoaEcommerce.CustomerService

run-catalog: ## Executa CatalogService (foreground)
	@echo "$(YELLOW)📦 Iniciando CatalogService na porta 7002...$(NC)"
	dotnet run --project src/CatalogService/SoaEcommerce.CatalogService

run-sales: ## Executa SalesService (foreground)
	@echo "$(YELLOW)💰 Iniciando SalesService na porta 7003...$(NC)"
	dotnet run --project src/SalesService/SoaEcommerce.SalesService

run-composition: ## Executa CompositionService (foreground)
	@echo "$(YELLOW)🎯 Iniciando CompositionService na porta 7000...$(NC)"
	dotnet run --project src/CompositionService/SoaEcommerce.CompositionService

run-customer-bg: ## Executa CustomerService em background
	@echo "$(YELLOW)👤 Iniciando CustomerService em background (porta 7001)...$(NC)"
	@mkdir -p logs
	@nohup dotnet run --project src/CustomerService/SoaEcommerce.CustomerService > logs/customer-service.log 2>&1 &

run-catalog-bg: ## Executa CatalogService em background
	@echo "$(YELLOW)📦 Iniciando CatalogService em background (porta 7002)...$(NC)"
	@mkdir -p logs
	@nohup dotnet run --project src/CatalogService/SoaEcommerce.CatalogService > logs/catalog-service.log 2>&1 &

run-sales-bg: ## Executa SalesService em background
	@echo "$(YELLOW)💰 Iniciando SalesService em background (porta 7003)...$(NC)"
	@mkdir -p logs
	@nohup dotnet run --project src/SalesService/SoaEcommerce.SalesService > logs/sales-service.log 2>&1 &

run-composition-bg: ## Executa CompositionService em background
	@echo "$(YELLOW)🎯 Iniciando CompositionService em background (porta 7000)...$(NC)"
	@mkdir -p logs
	@nohup dotnet run --project src/CompositionService/SoaEcommerce.CompositionService > logs/composition-service.log 2>&1 &

stop-all: ## Para todos os processos .NET
	@echo "$(YELLOW)🛑 Parando todos os serviços...$(NC)"
	pkill -f "dotnet run" || true
	@echo "$(GREEN)✅ Serviços parados!$(NC)"

stop-customer: ## Para apenas o CustomerService
	@echo "$(YELLOW)🛑 Parando CustomerService...$(NC)"
	pkill -f "dotnet run.*CustomerService" || true
	@echo "$(GREEN)✅ CustomerService parado!$(NC)"

stop-catalog: ## Para apenas o CatalogService
	@echo "$(YELLOW)🛑 Parando CatalogService...$(NC)"
	pkill -f "dotnet run.*CatalogService" || true
	@echo "$(GREEN)✅ CatalogService parado!$(NC)"

stop-sales: ## Para apenas o SalesService
	@echo "$(YELLOW)🛑 Parando SalesService...$(NC)"
	pkill -f "dotnet run.*SalesService" || true
	@echo "$(GREEN)✅ SalesService parado!$(NC)"

stop-composition: ## Para apenas o CompositionService
	@echo "$(YELLOW)🛑 Parando CompositionService...$(NC)"
	pkill -f "dotnet run.*CompositionService" || true
	@echo "$(GREEN)✅ CompositionService parado!$(NC)"

# =============================================================================
# DOCKER
# =============================================================================

docker-up: ## Inicia apenas os bancos de dados com Docker Compose
	@echo "$(YELLOW)🐳 Iniciando bancos de dados com Docker Compose...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_FILE) up -d

docker-down: ## Para todos os bancos de dados Docker
	@echo "$(YELLOW)🛑 Parando bancos de dados Docker...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_FILE) down

docker-build: ## Constrói as imagens Docker (apenas bancos)
	@echo "$(YELLOW)🔨 Construindo imagens Docker...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_FILE) build

docker-logs: ## Mostra logs dos bancos de dados Docker
	@echo "$(YELLOW)📋 Mostrando logs dos bancos de dados...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_FILE) logs -f

docker-clean: ## Remove containers, volumes e imagens Docker
	@echo "$(YELLOW)🧹 Limpando Docker...$(NC)"
	docker-compose -f $(DOCKER_COMPOSE_FILE) down -v --rmi all
	docker system prune -f

# =============================================================================
# TESTES
# =============================================================================

test: ## Executa todos os testes
	@echo "$(YELLOW)🧪 Executando testes...$(NC)"
	dotnet test tests/CompositionService.Tests/SoaEcommerce.CompositionService.Tests/



test-customer: ## Testa apenas o CustomerService
	@echo "$(YELLOW)🧪 Testando CustomerService...$(NC)"
	@echo "$(BLUE)Testando endpoint SOAP...$(NC)"
	@if curl -s http://localhost:7001/soap > /dev/null; then \
		echo "$(GREEN)✅ Endpoint SOAP acessível$(NC)"; \
	else \
		echo "$(RED)❌ Endpoint SOAP não acessível$(NC)"; \
		exit 1; \
	fi
	@echo "$(BLUE)Testando operação CreateCustomer...$(NC)"
	@curl -X POST http://localhost:7001/soap \
		-H "Content-Type: text/xml; charset=utf-8" \
		-H "SOAPAction: urn:soa-ecommerce:v1:customers/ICustomerService/CreateCustomer" \
		-d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Header/><soapenv:Body><CreateCustomer xmlns="urn:soa-ecommerce:v1:customers"><request xmlns:q1="http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts"><q1:Email>test@example.com</q1:Email><q1:Name>Test User</q1:Name></request></CreateCustomer></soapenv:Body></soapenv:Envelope>' \
		-s | grep -q "CreateCustomerResponse" && echo "$(GREEN)✅ CreateCustomer funcionando$(NC)" || echo "$(RED)❌ CreateCustomer falhou$(NC)"

test-catalog: ## Testa apenas o CatalogService
	@echo "$(YELLOW)🧪 Testando CatalogService...$(NC)"
	@echo "$(BLUE)Testando endpoint SOAP...$(NC)"
	@if curl -s http://localhost:7002/soap > /dev/null; then \
		echo "$(GREEN)✅ Endpoint SOAP acessível$(NC)"; \
	else \
		echo "$(RED)❌ Endpoint SOAP não acessível$(NC)"; \
		exit 1; \
	fi
	@echo "$(BLUE)Testando operação CreateProduct...$(NC)"
	@curl -X POST http://localhost:7002/soap \
		-H "Content-Type: text/xml; charset=utf-8" \
		-H "SOAPAction: urn:soa-ecommerce:v1:catalog/ICatalogService/CreateProduct" \
		-d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Header/><soapenv:Body><CreateProduct xmlns="urn:soa-ecommerce:v1:catalog"><request xmlns:q1="http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts"><q1:Name>Test Product</q1:Name><q1:Price>99.99</q1:Price><q1:Stock>10</q1:Stock></request></CreateProduct></soapenv:Body></soapenv:Envelope>' \
		-s | grep -q "CreateProductResponse" && echo "$(GREEN)✅ CreateProduct funcionando$(NC)" || echo "$(RED)❌ CreateProduct falhou$(NC)"

test-sales: ## Testa apenas o SalesService
	@echo "$(YELLOW)🧪 Testando SalesService...$(NC)"
	@echo "$(BLUE)Testando endpoint SOAP...$(NC)"
	@if curl -s http://localhost:7003/soap > /dev/null; then \
		echo "$(GREEN)✅ Endpoint SOAP acessível$(NC)"; \
	else \
		echo "$(RED)❌ Endpoint SOAP não acessível$(NC)"; \
		exit 1; \
	fi
	@echo "$(BLUE)Criando cliente para teste...$(NC)"
	@CUSTOMER_EMAIL="sales-test-$$(date +%s)@example.com"; \
	CUSTOMER_RESPONSE=$$(curl -s -X POST http://localhost:7001/soap \
		-H "Content-Type: text/xml; charset=utf-8" \
		-H "SOAPAction: urn:soa-ecommerce:v1:customers/ICustomerService/CreateCustomer" \
		-d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Header/><soapenv:Body><CreateCustomer xmlns="urn:soa-ecommerce:v1:customers"><request xmlns:q1="http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts"><q1:Email>'"$$CUSTOMER_EMAIL"'</q1:Email><q1:Name>Sales Test User</q1:Name></request></CreateCustomer></soapenv:Body></soapenv:Envelope>'); \
	CUSTOMER_ID=$$(echo "$$CUSTOMER_RESPONSE" | sed -n 's:.*<[^>]*CustomerId>\([^<]*\)</[^>]*CustomerId>.*:\1:p'); \
	if [ -z "$$CUSTOMER_ID" ]; then \
		echo "$(RED)❌ Falha ao criar cliente para teste$(NC)"; \
		echo "Resposta completa:"; echo "$$CUSTOMER_RESPONSE"; \
		exit 1; \
	fi; \
	echo "$(GREEN)✅ Cliente criado: $$CUSTOMER_ID$(NC)"; \
	echo "$(BLUE)Testando operação CreateOrder...$(NC)"; \
	curl -X POST http://localhost:7003/soap \
		-H "Content-Type: text/xml; charset=utf-8" \
		-H "SOAPAction: urn:soa-ecommerce:v1:sales/ISalesService/CreateOrder" \
		-d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Header/><soapenv:Body><CreateOrder xmlns="urn:soa-ecommerce:v1:sales"><request xmlns:q1="http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts"><q1:CustomerId>'"$$CUSTOMER_ID"'</q1:CustomerId><q1:Items><q1:Item><q1:ProductId>33333333-3333-3333-3333-333333333333</q1:ProductId><q1:Quantity>1</q1:Quantity><q1:UnitPrice>99.99</q1:UnitPrice></q1:Item></q1:Items></request></CreateOrder></soapenv:Body></soapenv:Envelope>' \
		-s | grep -q "CreateOrderResponse" && echo "$(GREEN)✅ CreateOrder funcionando$(NC)" || (echo "$(RED)❌ CreateOrder falhou$(NC)"; exit 1)

test-composition: ## Testa apenas o CompositionService
	@echo "$(YELLOW)🧪 Testando CompositionService...$(NC)"
	@echo "$(BLUE)Testando endpoint SOAP...$(NC)"
	@if curl -s http://localhost:7000/soap > /dev/null; then \
		echo "$(GREEN)✅ Endpoint SOAP acessível$(NC)"; \
	else \
		echo "$(RED)❌ Endpoint SOAP não acessível$(NC)"; \
		exit 1; \
	fi
	@echo "$(BLUE)Criando cliente para teste...$(NC)"
	@CUSTOMER_EMAIL="teste@teste.com"; \
	CUSTOMER_RESPONSE=$$(curl -s -X POST http://localhost:7001/soap \
		-H "Content-Type: text/xml; charset=utf-8" \
		-H "SOAPAction: urn:soa-ecommerce:v1:customers/ICustomerService/CreateCustomer" \
		-d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Header/><soapenv:Body><CreateCustomer xmlns="urn:soa-ecommerce:v1:customers"><request xmlns:q1="http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts"><q1:Email>'"$$CUSTOMER_EMAIL"'</q1:Email><q1:Name>Composition Test User</q1:Name></request></CreateCustomer></soapenv:Body></soapenv:Envelope>'); \
	CUSTOMER_ID=$$(echo "$$CUSTOMER_RESPONSE" | sed -n 's:.*<[^>]*CustomerId>\([^<]*\)</[^>]*CustomerId>.*:\1:p'); \
	if [ -z "$$CUSTOMER_ID" ]; then \
		echo "$(RED)❌ Falha ao criar cliente para teste$(NC)"; \
		echo "Resposta completa:"; echo "$$CUSTOMER_RESPONSE"; \
		exit 1; \
	fi; \
	echo "$(GREEN)✅ Cliente criado: ID=$$CUSTOMER_ID, Email=$$CUSTOMER_EMAIL$(NC)"; \
	echo "$(BLUE)Criando produto para teste...$(NC)"; \
	PRODUCT_RESPONSE=$$(curl -s -X POST http://localhost:7002/soap \
		-H "Content-Type: text/xml; charset=utf-8" \
		-H "SOAPAction: urn:soa-ecommerce:v1:catalog/ICatalogService/CreateProduct" \
		-d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Header/><soapenv:Body><CreateProduct xmlns="urn:soa-ecommerce:v1:catalog"><request xmlns:q1="http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts"><q1:Name>Composition Test Product</q1:Name><q1:Price>99.99</q1:Price><q1:Stock>10</q1:Stock></request></CreateProduct></soapenv:Body></soapenv:Envelope>'); \
	PRODUCT_ID=$$(echo "$$PRODUCT_RESPONSE" | sed -n 's:.*<[^>]*ProductId>\([^<]*\)</[^>]*ProductId>.*:\1:p'); \
	if [ -z "$$PRODUCT_ID" ]; then \
		echo "$(RED)❌ Falha ao criar produto para teste$(NC)"; \
		echo "Resposta completa:"; echo "$$PRODUCT_RESPONSE"; \
		exit 1; \
	fi; \
	echo "$(GREEN)✅ Produto criado: ID=$$PRODUCT_ID$(NC)"; \
	echo "$(BLUE)Testando operação PlaceOrder...$(NC)"; \
	curl -X POST http://localhost:7000/soap \
		-H "Content-Type: text/xml; charset=utf-8" \
		-H "SOAPAction: urn:soa-ecommerce:v1:process/ICompositionService/PlaceOrder" \
		-d '<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"><soapenv:Header/><soapenv:Body><PlaceOrder xmlns="urn:soa-ecommerce:v1:process"><request xmlns:q1="http://schemas.datacontract.org/2004/07/SoaEcommerce.Contracts"><q1:CustomerEmail>'"$$CUSTOMER_EMAIL"'</q1:CustomerEmail><q1:Items><q1:Item><q1:ProductId>'"$$PRODUCT_ID"'</q1:ProductId><q1:Quantity>1</q1:Quantity></q1:Item></q1:Items></request></PlaceOrder></soapenv:Body></soapenv:Envelope>' \
		-s | grep -q "PlaceOrderResponse" && echo "$(GREEN)✅ PlaceOrder funcionando$(NC)" || (echo "$(RED)❌ PlaceOrder falhou$(NC)"; exit 1)

# =============================================================================
# UTILITÁRIOS
# =============================================================================

generate-proxies: ## Gera proxies SOAP
	@echo "$(YELLOW)🔧 Gerando proxies SOAP...$(NC)"
	@if [ "$(OS)" = "Windows_NT" ]; then \
		powershell -ExecutionPolicy Bypass -File scripts/generate-proxies.ps1; \
	else \
		chmod +x scripts/generate-proxies.sh; \
		./scripts/generate-proxies.sh; \
	fi

health-check: ## Verifica saúde dos serviços
	@echo "$(YELLOW)🏥 Verificando saúde dos serviços...$(NC)"
	@echo "$(BLUE)CompositionService:$(NC)"
	curl -s http://localhost:7000/health || echo "$(RED)❌ Indisponível$(NC)"
	@echo "$(BLUE)CustomerService:$(NC)"
	curl -s http://localhost:7001/health || echo "$(RED)❌ Indisponível$(NC)"
	@echo "$(BLUE)CatalogService:$(NC)"
	curl -s http://localhost:7002/health || echo "$(RED)❌ Indisponível$(NC)"
	@echo "$(BLUE)SalesService:$(NC)"
	curl -s http://localhost:7003/health || echo "$(RED)❌ Indisponível$(NC)"

logs: ## Mostra logs dos serviços em background
	@echo "$(YELLOW)📋 Mostrando logs dos serviços...$(NC)"
	@echo "$(BLUE)Use Ctrl+C para sair$(NC)"
	@mkdir -p logs
	@if [ -f "logs/customer-service.log" ] || [ -f "logs/catalog-service.log" ] || [ -f "logs/sales-service.log" ] || [ -f "logs/composition-service.log" ]; then \
		tail -f logs/*.log; \
	else \
		echo "$(RED)Nenhum arquivo de log encontrado$(NC)"; \
		echo "$(BLUE)Execute 'make run-all' primeiro para iniciar os serviços$(NC)"; \
	fi

logs-customer: ## Mostra logs do CustomerService
	@echo "$(YELLOW)📋 Mostrando logs do CustomerService...$(NC)"
	@echo "$(BLUE)Use Ctrl+C para sair$(NC)"
	@if [ -f "logs/customer-service.log" ]; then \
		tail -f logs/customer-service.log; \
	else \
		echo "$(RED)Log do CustomerService não encontrado$(NC)"; \
	fi

logs-catalog: ## Mostra logs do CatalogService
	@echo "$(YELLOW)📋 Mostrando logs do CatalogService...$(NC)"
	@echo "$(BLUE)Use Ctrl+C para sair$(NC)"
	@if [ -f "logs/catalog-service.log" ]; then \
		tail -f logs/catalog-service.log; \
	else \
		echo "$(RED)Log do CatalogService não encontrado$(NC)"; \
	fi

logs-sales: ## Mostra logs do SalesService
	@echo "$(YELLOW)📋 Mostrando logs do SalesService...$(NC)"
	@echo "$(BLUE)Use Ctrl+C para sair$(NC)"
	@if [ -f "logs/sales-service.log" ]; then \
		tail -f logs/sales-service.log; \
	else \
		echo "$(RED)Log do SalesService não encontrado$(NC)"; \
	fi

logs-composition: ## Mostra logs do CompositionService
	@echo "$(YELLOW)📋 Mostrando logs do CompositionService...$(NC)"
	@echo "$(BLUE)Use Ctrl+C para sair$(NC)"
	@if [ -f "logs/composition-service.log" ]; then \
		tail -f logs/composition-service.log; \
	else \
		echo "$(RED)Log do CompositionService não encontrado$(NC)"; \
	fi

# =============================================================================
# DESENVOLVIMENTO
# =============================================================================

dev-setup: restore migrate-all ## Configuração inicial para desenvolvimento
	@echo "$(GREEN)✅ Ambiente de desenvolvimento configurado!$(NC)"
	@echo "$(BLUE)📝 Próximos passos:$(NC)"
	@echo "$(BLUE)   - make run-all (para executar todos os serviços)$(NC)"
	@echo "$(BLUE)   - make docker-up (para usar Docker)$(NC)"
	@echo "$(BLUE)   - make test-soap (para testar os serviços)$(NC)"

dev-reset: clean docker-clean ## Reset completo do ambiente
	@echo "$(YELLOW)🔄 Resetando ambiente de desenvolvimento...$(NC)"
	$(MAKE) clean
	$(MAKE) docker-clean
	$(MAKE) dev-setup

# =============================================================================
# PRODUÇÃO
# =============================================================================

prod-build: ## Build para produção
	@echo "$(YELLOW)🏭 Build para produção...$(NC)"
	dotnet publish $(SOLUTION_FILE) --configuration Release --output ./publish

prod-deploy: prod-build docker-up ## Deploy para produção
	@echo "$(GREEN)✅ Deploy concluído!$(NC)"

# =============================================================================
# MONITORAMENTO
# =============================================================================

status: ## Mostra status dos serviços
	@echo "$(YELLOW)📊 Status dos serviços:$(NC)"
	@echo "$(BLUE)Processos .NET:$(NC)"
	@if ps aux | grep "dotnet run" | grep -v grep > /dev/null; then \
		ps aux | grep "dotnet run" | grep -v grep; \
	else \
		echo "$(RED)Nenhum processo .NET encontrado$(NC)"; \
	fi
	@echo "$(BLUE)Portas em uso:$(NC)"
	@if lsof -i :7000 > /dev/null 2>&1; then echo "$(GREEN)✅ Porta 7000 (CompositionService) - Ativa$(NC)"; else echo "$(RED)❌ Porta 7000 (CompositionService) - Inativa$(NC)"; fi
	@if lsof -i :7001 > /dev/null 2>&1; then echo "$(GREEN)✅ Porta 7001 (CustomerService) - Ativa$(NC)"; else echo "$(RED)❌ Porta 7001 (CustomerService) - Inativa$(NC)"; fi
	@if lsof -i :7002 > /dev/null 2>&1; then echo "$(GREEN)✅ Porta 7002 (CatalogService) - Ativa$(NC)"; else echo "$(RED)❌ Porta 7002 (CatalogService) - Inativa$(NC)"; fi
	@if lsof -i :7003 > /dev/null 2>&1; then echo "$(GREEN)✅ Porta 7003 (SalesService) - Ativa$(NC)"; else echo "$(RED)❌ Porta 7003 (SalesService) - Inativa$(NC)"; fi
	@echo "$(BLUE)Containers Docker:$(NC)"
	@if docker ps --filter "name=soa" --filter "name=pg-" > /dev/null 2>&1; then \
		docker ps --filter "name=soa" --filter "name=pg-"; \
	else \
		echo "$(RED)Nenhum container SOA encontrado$(NC)"; \
	fi

ports: ## Mostra portas utilizadas
	@echo "$(YELLOW)🔌 Portas utilizadas:$(NC)"
	@echo "$(BLUE)CompositionService:$(NC) http://localhost:7000"
	@echo "$(BLUE)CustomerService:$(NC) http://localhost:7001"
	@echo "$(BLUE)CatalogService:$(NC) http://localhost:7002"
	@echo "$(BLUE)SalesService:$(NC) http://localhost:7003"
	@echo "$(BLUE)PostgreSQL Customers:$(NC) localhost:5432"
	@echo "$(BLUE)PostgreSQL Catalog:$(NC) localhost:5433"
	@echo "$(BLUE)PostgreSQL Sales:$(NC) localhost:5434"
