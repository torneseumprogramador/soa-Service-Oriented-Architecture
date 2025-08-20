# Makefile para SOA E-commerce
# Arquitetura Orientada a Serviços com SOAP/WSDL

.PHONY: help build clean restore test docker-up docker-down docker-build docker-logs docker-clean
.PHONY: run-all run-customer run-catalog run-sales run-composition
.PHONY: migrate-all migrate-customer migrate-catalog migrate-sales
.PHONY: generate-proxies test-soap health-check logs

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

clean: ## Limpa arquivos de build
	@echo "$(YELLOW)🧹 Limpando arquivos de build...$(NC)"
	dotnet clean $(SOLUTION_FILE)
	find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
	find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

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
	@echo "$(YELLOW)🚀 Iniciando todos os serviços...$(NC)"
	@echo "$(BLUE)📝 Use 'make logs' para ver os logs$(NC)"
	@echo "$(BLUE)📝 Use 'make stop-all' para parar todos os serviços$(NC)"
	$(MAKE) run-customer &
	$(MAKE) run-catalog &
	$(MAKE) run-sales &
	$(MAKE) run-composition &
	@echo "$(GREEN)✅ Todos os serviços iniciados!$(NC)"

run-customer: ## Executa CustomerService
	@echo "$(YELLOW)👤 Iniciando CustomerService na porta 7001...$(NC)"
	dotnet run --project src/CustomerService/SoaEcommerce.CustomerService

run-catalog: ## Executa CatalogService
	@echo "$(YELLOW)📦 Iniciando CatalogService na porta 7002...$(NC)"
	dotnet run --project src/CatalogService/SoaEcommerce.CatalogService

run-sales: ## Executa SalesService
	@echo "$(YELLOW)💰 Iniciando SalesService na porta 7003...$(NC)"
	dotnet run --project src/SalesService/SoaEcommerce.SalesService

run-composition: ## Executa CompositionService
	@echo "$(YELLOW)🎯 Iniciando CompositionService na porta 7000...$(NC)"
	dotnet run --project src/CompositionService/SoaEcommerce.CompositionService

stop-all: ## Para todos os processos .NET
	@echo "$(YELLOW)🛑 Parando todos os serviços...$(NC)"
	pkill -f "dotnet run" || true
	@echo "$(GREEN)✅ Serviços parados!$(NC)"

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

test-soap: ## Executa testes SOAP manuais
	@echo "$(YELLOW)🧪 Executando testes SOAP...$(NC)"
	chmod +x scripts/test-soap-services.sh
	./scripts/test-soap-services.sh

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

logs: ## Mostra logs dos serviços (se executando localmente)
	@echo "$(YELLOW)📋 Mostrando logs dos serviços...$(NC)"
	@echo "$(BLUE)Use Ctrl+C para sair$(NC)"
	@if [ -d "logs" ]; then \
		tail -f logs/*.txt; \
	else \
		echo "$(RED)Nenhum arquivo de log encontrado$(NC)"; \
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
	ps aux | grep "dotnet run" | grep -v grep || echo "$(RED)Nenhum processo .NET encontrado$(NC)"
	@echo "$(BLUE)Containers Docker:$(NC)"
	docker ps --filter "name=soa" || echo "$(RED)Nenhum container SOA encontrado$(NC)"

ports: ## Mostra portas utilizadas
	@echo "$(YELLOW)🔌 Portas utilizadas:$(NC)"
	@echo "$(BLUE)CompositionService:$(NC) http://localhost:7000"
	@echo "$(BLUE)CustomerService:$(NC) http://localhost:7001"
	@echo "$(BLUE)CatalogService:$(NC) http://localhost:7002"
	@echo "$(BLUE)SalesService:$(NC) http://localhost:7003"
	@echo "$(BLUE)PostgreSQL Customers:$(NC) localhost:5432"
	@echo "$(BLUE)PostgreSQL Catalog:$(NC) localhost:5433"
	@echo "$(BLUE)PostgreSQL Sales:$(NC) localhost:5434"
