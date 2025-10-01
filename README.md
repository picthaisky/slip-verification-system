# ==================================
# Slip Verification System
# Makefile - Quick Commands
# ==================================

.PHONY: help setup start stop restart clean test build deploy logs

# Default target
.DEFAULT_GOAL := help

# Colors for output
YELLOW := \033[1;33m
GREEN := \033[1;32m
RED := \033[1;31m
NC := \033[0m # No Color

## help: Show this help message
help:
	@echo "${GREEN}Slip Verification System - Available Commands${NC}"
	@echo ""
	@grep -E '^## .*$$' $(MAKEFILE_LIST) | sed 's/## /  /'
	@echo ""

# ==================================
# SETUP & INSTALLATION
# ==================================

## setup: Initial project setup
setup:
	@echo "${YELLOW}Setting up project...${NC}"
	@cp .env.example .env
	@echo "${GREEN}✓ Created .env file${NC}"
	@$(MAKE) setup-backend
	@$(MAKE) setup-frontend
	@$(MAKE) setup-ocr
	@echo "${GREEN}✓ Setup complete!${NC}"

## setup-backend: Setup backend dependencies
setup-backend:
	@echo "${YELLOW}Setting up backend...${NC}"
	cd src/backend/SlipVerification.API && dotnet restore
	@echo "${GREEN}✓ Backend setup complete${NC}"

## setup-frontend: Setup frontend dependencies
setup-frontend:
	@echo "${YELLOW}Setting up frontend...${NC}"
	cd src/frontend/slip-verification-web && npm install
	@echo "${GREEN}✓ Frontend setup complete${NC}"

## setup-ocr: Setup OCR service dependencies
setup-ocr:
	@echo "${YELLOW}Setting up OCR service...${NC}"
	cd src/services/ocr-service && python -m venv venv && \
	. venv/bin/activate && pip install -r requirements.txt
	@echo "${GREEN}✓ OCR service setup complete${NC}"

# ==================================
# DOCKER COMMANDS
# ==================================

## docker-up: Start all services with Docker Compose
docker-up:
	@echo "${YELLOW}Starting all services...${NC}"
	docker-compose up -d
	@echo "${GREEN}✓ All services started${NC}"
	@$(MAKE) docker-logs

## docker-down: Stop all services
docker-down:
	@echo "${YELLOW}Stopping all services...${NC}"
	docker-compose down
	@echo "${GREEN}✓ All services stopped${NC}"

## docker-restart: Restart all services
docker-restart:
	@$(MAKE) docker-down
	@$(MAKE) docker-up

## docker-build: Build all Docker images
docker-build:
	@echo "${YELLOW}Building Docker images...${NC}"
	docker-compose build
	@echo "${GREEN}✓ Docker images built${NC}"

## docker-logs: Show logs from all services
docker-logs:
	docker-compose logs -f

## docker-logs-api: Show API logs
docker-logs-api:
	docker-compose logs -f api

## docker-logs-frontend: Show frontend logs
docker-logs-frontend:
	docker-compose logs -f frontend

## docker-logs-ocr: Show OCR service logs
docker-logs-ocr:
	docker-compose logs -f ocr-service

## docker-clean: Remove all containers, volumes, and images
docker-clean:
	@echo "${RED}Warning: This will remove all containers, volumes, and images!${NC}"
	@read -p "Are you sure? [y/N] " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		docker-compose down -v --rmi all; \
		echo "${GREEN}✓ Cleanup complete${NC}"; \
	fi

# ==================================
# DEVELOPMENT
# ==================================

## dev: Start development environment
dev:
	@echo "${YELLOW}Starting development environment...${NC}"
	@$(MAKE) docker-up
	@$(MAKE) migrate
	@$(MAKE) seed
	@echo "${GREEN}✓ Development environment ready!${NC}"
	@echo "API: http://localhost:5000"
	@echo "Frontend: http://localhost:4200"
	@echo "Swagger: http://localhost:5000/swagger"

## dev-backend: Run backend in development mode
dev-backend:
	cd src/backend/SlipVerification.API && dotnet watch run

## dev-frontend: Run frontend in development mode
dev-frontend:
	cd src/frontend/slip-verification-web && ng serve --open

## dev-ocr: Run OCR service in development mode
dev-ocr:
	cd src/services/ocr-service && \
	. venv/bin/activate && \
	uvicorn app.main:app --reload --host 0.0.0.0 --port 8000

# ==================================
# DATABASE
# ==================================

## migrate: Run database migrations
migrate:
	@echo "${YELLOW}Running migrations...${NC}"
	docker-compose exec api dotnet ef database update
	@echo "${GREEN}✓ Migrations applied${NC}"

## migrate-create: Create new migration (name=MigrationName)
migrate-create:
	@if [ -z "$(name)" ]; then \
		echo "${RED}Error: Please provide migration name: make migrate-create name=YourMigrationName${NC}"; \
		exit 1; \
	fi
	cd src/backend/SlipVerification.API && \
	dotnet ef migrations add $(name)
	@echo "${GREEN}✓ Migration $(name) created${NC}"

## migrate-rollback: Rollback last migration
migrate-rollback:
	@echo "${YELLOW}Rolling back migration...${NC}"
	docker-compose exec api dotnet ef database update 0
	@echo "${GREEN}✓ Migration rolled back${NC}"

## seed: Seed database with initial data
seed:
	@echo "${YELLOW}Seeding database...${NC}"
	docker-compose exec api dotnet run seed
	@echo "${GREEN}✓ Database seeded${NC}"

## db-backup: Backup database
db-backup:
	@echo "${YELLOW}Backing up database...${NC}"
	@mkdir -p backups
	docker-compose exec postgres pg_dump -U postgres slip_verification_db > \
		backups/backup_$$(date +%Y%m%d_%H%M%S).sql
	@echo "${GREEN}✓ Backup created${NC}"

## db-restore: Restore database from backup (file=backup.sql)
db-restore:
	@if [ -z "$(file)" ]; then \
		echo "${RED}Error: Please provide backup file: make db-restore file=backup.sql${NC}"; \
		exit 1; \
	fi
	@echo "${YELLOW}Restoring database...${NC}"
	docker-compose exec -T postgres psql -U postgres slip_verification_db < $(file)
	@echo "${GREEN}✓ Database restored${NC}"

# ==================================
# TESTING
# ==================================

## test: Run all tests
test:
	@$(MAKE) test-backend
	@$(MAKE) test-frontend

## test-backend: Run backend tests
test-backend:
	@echo "${YELLOW}Running backend tests...${NC}"
	cd src/backend && dotnet test --verbosity normal
	@echo "${GREEN}✓ Backend tests complete${NC}"

## test-frontend: Run frontend tests
test-frontend:
	@echo "${YELLOW}Running frontend tests...${NC}"
	cd src/frontend/slip-verification-web && ng test --watch=false --code-coverage
	@echo "${GREEN}✓ Frontend tests complete${NC}"

## test-integration: Run integration tests
test-integration:
	@echo "${YELLOW}Running integration tests...${NC}"
	cd tests/SlipVerification.IntegrationTests && dotnet test
	@echo "${GREEN}✓ Integration tests complete${NC}"

## test-e2e: Run E2E tests
test-e2e:
	@echo "${YELLOW}Running E2E tests...${NC}"
	cd src/frontend/slip-verification-web && ng e2e
	@echo "${GREEN}✓ E2E tests complete${NC}"

## test-coverage: Generate test coverage report
test-coverage:
	@echo "${YELLOW}Generating coverage report...${NC}"
	cd src/backend && dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=html
	@echo "${GREEN}✓ Coverage report generated${NC}"
	@echo "Open: src/backend/coverage/index.html"

# ==================================
# CODE QUALITY
# ==================================

## lint: Run linters
lint:
	@$(MAKE) lint-backend
	@$(MAKE) lint-frontend

## lint-backend: Lint backend code
lint-backend:
	@echo "${YELLOW}Linting backend...${NC}"
	cd src/backend && dotnet format --verify-no-changes
	@echo "${GREEN}✓ Backend linting complete${NC}"

## lint-frontend: Lint frontend code
lint-frontend:
	@echo "${YELLOW}Linting frontend...${NC}"
	cd src/frontend/slip-verification-web && ng lint
	@echo "${GREEN}✓ Frontend linting complete${NC}"

## format: Format code
format:
	@echo "${YELLOW}Formatting code...${NC}"
	cd src/backend && dotnet format
	cd src/frontend/slip-verification-web && npm run format
	@echo "${GREEN}✓ Code formatted${NC}"

# ==================================
# BUILD & DEPLOY
# ==================================

## build: Build all projects
build:
	@$(MAKE) build-backend
	@$(MAKE) build-frontend

## build-backend: Build backend
build-backend:
	@echo "${YELLOW}Building backend...${NC}"
	cd src/backend/SlipVerification.API && dotnet build --configuration Release
	@echo "${GREEN}✓ Backend built${NC}"

## build-frontend: Build frontend
build-frontend:
	@echo "${YELLOW}Building frontend...${NC}"
	cd src/frontend/slip-verification-web && ng build --configuration production
	@echo "${GREEN}✓ Frontend built${NC}"

## publish: Publish backend for deployment
publish:
	@echo "${YELLOW}Publishing backend...${NC}"
	cd src/backend/SlipVerification.API && \
	dotnet publish -c Release -o ../../publish/api
	@echo "${GREEN}✓ Backend published to publish/api${NC}"

## deploy-staging: Deploy to staging environment
deploy-staging:
	@echo "${YELLOW}Deploying to staging...${NC}"
	docker-compose -f docker-compose.prod.yml up -d
	@echo "${GREEN}✓ Deployed to staging${NC}"

## deploy-production: Deploy to production (requires confirmation)
deploy-production:
	@echo "${RED}Warning: This will deploy to PRODUCTION!${NC}"
	@read -p "Are you sure? [y/N] " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		./scripts/deploy-production.sh; \
		echo "${GREEN}✓ Deployed to production${NC}"; \
	fi

# ==================================
# MONITORING
# ==================================

## monitoring-up: Start monitoring stack
monitoring-up:
	@echo "${YELLOW}Starting monitoring stack...${NC}"
	docker-compose --profile monitoring up -d
	@echo "${GREEN}✓ Monitoring stack started${NC}"
	@echo "Prometheus: http://localhost:9090"
	@echo "Grafana: http://localhost:3000"
	@echo "Seq: http://localhost:5341"

## logs-api: Show API logs
logs-api:
	tail -f src/backend/SlipVerification.API/logs/*.log

## logs-frontend: Show frontend logs  
logs-frontend:
	cd src/frontend/slip-verification-web && ng build --watch

## health-check: Check health of all services
health-check:
	@echo "${YELLOW}Checking service health...${NC}"
	@curl -s http://localhost:5000/health | json_pp
	@curl -s http://localhost:8000/health | json_pp
	@echo "${GREEN}✓ Health check complete${NC}"

# ==================================
# UTILITIES
# ==================================

## ps: Show running containers
ps:
	docker-compose ps

## stats: Show container stats
stats:
	docker stats

## shell-api: Open shell in API container
shell-api:
	docker-compose exec api /bin/bash

## shell-db: Open PostgreSQL shell
shell-db:
	docker-compose exec postgres psql -U postgres slip_verification_db

## shell-redis: Open Redis CLI
shell-redis:
	docker-compose exec redis redis-cli

## clean-logs: Clean all log files
clean-logs:
	@echo "${YELLOW}Cleaning logs...${NC}"
	find . -name "*.log" -type f -delete
	@echo "${GREEN}✓ Logs cleaned${NC}"

## clean-cache: Clean build cache
clean-cache:
	@echo "${YELLOW}Cleaning cache...${NC}"
	cd src/backend && dotnet clean
	cd src/frontend/slip-verification-web && npm run clean
	@echo "${GREEN}✓ Cache cleaned${NC}"

## clean-all: Clean everything (cache, logs, builds)
clean-all:
	@$(MAKE) clean-logs
	@$(MAKE) clean-cache
	@$(MAKE) docker-clean

## reset: Reset project to initial state
reset:
	@echo "${RED}Warning: This will delete all data!${NC}"
	@read -p "Are you sure? [y/N] " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		$(MAKE) docker-down; \
		$(MAKE) clean-all; \
		docker volume prune -f; \
		echo "${GREEN}✓ Project reset${NC}"; \
	fi

# ==================================
# DOCUMENTATION
# ==================================

## docs-serve: Serve documentation locally
docs-serve:
	cd docs && python -m http.server 8080

## docs-generate: Generate API documentation
docs-generate:
	@echo "${YELLOW}Generating API docs...${NC}"
	cd src/backend/SlipVerification.API && dotnet swagger tofile --output swagger.json bin/Debug/net9.0/SlipVerification.API.dll v1
	@echo "${GREEN}✓ API docs generated${NC}"

# ==================================
# QUICK ACCESS URLs
# ==================================

## open: Open all services in browser
open:
	@echo "${GREEN}Opening services...${NC}"
	open http://localhost:4200        # Frontend
	open http://localhost:5000/swagger # API Swagger
	open http://localhost:15672        # RabbitMQ
	open http://localhost:9001         # MinIO
	open http://localhost:5050         # pgAdmin

## urls: Show all service URLs
urls:
	@echo "${GREEN}Service URLs:${NC}"
	@echo "Frontend:        http://localhost:4200"
	@echo "API:             http://localhost:5000"
	@echo "API Swagger:     http://localhost:5000/swagger"
	@echo "API ReDoc:       http://localhost:5000/redoc"
	@echo "OCR Service:     http://localhost:8000"
	@echo "RabbitMQ:        http://localhost:15672"
	@echo "MinIO:           http://localhost:9001"
	@echo "pgAdmin:         http://localhost:5050"
	@echo "Prometheus:      http://localhost:9090"
	@echo "Grafana:         http://localhost:3000"
	@echo "Seq:             http://localhost:5341"
