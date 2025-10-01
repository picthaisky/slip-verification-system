# Slip Verification System - DevOps Makefile
# Quick commands for deployment, monitoring, and maintenance

.PHONY: help dev prod monitoring k8s-deploy backup clean

# Default target
.DEFAULT_GOAL := help

# Colors for output
YELLOW := \033[1;33m
GREEN := \033[0;32m
RED := \033[0;31m
NC := \033[0m # No Color

##@ General

help: ## Display this help message
	@echo "$(GREEN)Slip Verification System - DevOps Commands$(NC)"
	@echo ""
	@awk 'BEGIN {FS = ":.*##"; printf "Usage:\n  make $(YELLOW)<target>$(NC)\n"} /^[a-zA-Z_0-9-]+:.*?##/ { printf "  $(YELLOW)%-20s$(NC) %s\n", $$1, $$2 } /^##@/ { printf "\n$(GREEN)%s$(NC)\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

##@ Development

dev-up: ## Start development environment
	@echo "$(GREEN)Starting development environment...$(NC)"
	docker-compose -f docker-compose.dev.yml up -d
	@echo "$(GREEN)Development environment is running!$(NC)"
	@echo "Frontend: http://localhost:4200"
	@echo "Backend: http://localhost:5000"
	@echo "OCR: http://localhost:8000"
	@echo "pgAdmin: http://localhost:5050"

dev-down: ## Stop development environment
	@echo "$(YELLOW)Stopping development environment...$(NC)"
	docker-compose -f docker-compose.dev.yml down

dev-logs: ## View development logs
	docker-compose -f docker-compose.dev.yml logs -f

dev-restart: ## Restart development environment
	@make dev-down
	@make dev-up

dev-clean: ## Clean development environment (removes volumes)
	@echo "$(RED)⚠️  This will remove all data! Press Ctrl+C to cancel...$(NC)"
	@sleep 5
	docker-compose -f docker-compose.dev.yml down -v
	@echo "$(GREEN)Development environment cleaned!$(NC)"

##@ Production

prod-up: ## Start production environment
	@echo "$(GREEN)Starting production environment...$(NC)"
	@if [ ! -f .env ]; then \
		echo "$(RED)Error: .env file not found!$(NC)"; \
		echo "Copy .env.production.example to .env and configure it."; \
		exit 1; \
	fi
	docker-compose -f docker-compose.prod.yml up -d
	@echo "$(GREEN)Production environment is running!$(NC)"

prod-down: ## Stop production environment
	@echo "$(YELLOW)Stopping production environment...$(NC)"
	docker-compose -f docker-compose.prod.yml down

prod-logs: ## View production logs
	docker-compose -f docker-compose.prod.yml logs -f

prod-restart: ## Restart production environment
	@make prod-down
	@make prod-up

##@ Monitoring

monitoring-up: ## Start monitoring stack (Prometheus, Grafana)
	@echo "$(GREEN)Starting monitoring stack...$(NC)"
	docker-compose -f docker-compose.monitoring.yml up -d
	@echo "$(GREEN)Monitoring is running!$(NC)"
	@echo "Prometheus: http://localhost:9090"
	@echo "Grafana: http://localhost:3000 (admin/admin)"
	@echo "Alertmanager: http://localhost:9093"

monitoring-down: ## Stop monitoring stack
	docker-compose -f docker-compose.monitoring.yml down

monitoring-logs: ## View monitoring logs
	docker-compose -f docker-compose.monitoring.yml logs -f

##@ Kubernetes

k8s-apply: ## Apply all Kubernetes manifests
	@echo "$(GREEN)Applying Kubernetes manifests...$(NC)"
	kubectl apply -f infrastructure/kubernetes/base/namespace.yaml
	kubectl apply -f infrastructure/kubernetes/base/secrets.yaml
	kubectl apply -f infrastructure/kubernetes/base/configmap.yaml
	kubectl apply -f infrastructure/kubernetes/base/pvc.yaml
	kubectl apply -f infrastructure/kubernetes/base/postgres-statefulset.yaml
	kubectl apply -f infrastructure/kubernetes/base/redis-deployment.yaml
	@echo "Waiting for databases to be ready..."
	kubectl wait --for=condition=ready pod -l app=postgres -n slip-verification --timeout=300s
	kubectl wait --for=condition=ready pod -l app=redis -n slip-verification --timeout=300s
	kubectl apply -f infrastructure/kubernetes/base/backend-deployment.yaml
	kubectl apply -f infrastructure/kubernetes/base/frontend-deployment.yaml
	kubectl apply -f infrastructure/kubernetes/base/ocr-deployment.yaml
	kubectl apply -f infrastructure/kubernetes/base/ingress.yaml
	kubectl apply -f infrastructure/kubernetes/base/hpa.yaml
	@echo "$(GREEN)Kubernetes deployment complete!$(NC)"

k8s-delete: ## Delete all Kubernetes resources
	@echo "$(RED)⚠️  This will delete all Kubernetes resources! Press Ctrl+C to cancel...$(NC)"
	@sleep 5
	kubectl delete -f infrastructure/kubernetes/base/ --ignore-not-found=true
	@echo "$(GREEN)Kubernetes resources deleted!$(NC)"

k8s-status: ## Check Kubernetes deployment status
	@echo "$(GREEN)Checking Kubernetes status...$(NC)"
	@echo "\n$(YELLOW)Pods:$(NC)"
	kubectl get pods -n slip-verification
	@echo "\n$(YELLOW)Services:$(NC)"
	kubectl get services -n slip-verification
	@echo "\n$(YELLOW)Ingress:$(NC)"
	kubectl get ingress -n slip-verification
	@echo "\n$(YELLOW)HPA:$(NC)"
	kubectl get hpa -n slip-verification

k8s-logs: ## View Kubernetes logs (usage: make k8s-logs POD=backend-api)
	@if [ -z "$(POD)" ]; then \
		echo "$(RED)Usage: make k8s-logs POD=backend-api$(NC)"; \
		exit 1; \
	fi
	kubectl logs -f deployment/$(POD) -n slip-verification

k8s-shell: ## Get shell in pod (usage: make k8s-shell POD=backend-api)
	@if [ -z "$(POD)" ]; then \
		echo "$(RED)Usage: make k8s-shell POD=backend-api$(NC)"; \
		exit 1; \
	fi
	kubectl exec -it deployment/$(POD) -n slip-verification -- /bin/sh

k8s-restart: ## Restart deployment (usage: make k8s-restart DEPLOY=backend-api)
	@if [ -z "$(DEPLOY)" ]; then \
		echo "$(RED)Usage: make k8s-restart DEPLOY=backend-api$(NC)"; \
		exit 1; \
	fi
	kubectl rollout restart deployment/$(DEPLOY) -n slip-verification

k8s-scale: ## Scale deployment (usage: make k8s-scale DEPLOY=backend-api REPLICAS=5)
	@if [ -z "$(DEPLOY)" ] || [ -z "$(REPLICAS)" ]; then \
		echo "$(RED)Usage: make k8s-scale DEPLOY=backend-api REPLICAS=5$(NC)"; \
		exit 1; \
	fi
	kubectl scale deployment/$(DEPLOY) --replicas=$(REPLICAS) -n slip-verification

##@ Database

db-backup: ## Backup database
	@echo "$(GREEN)Creating database backup...$(NC)"
	./scripts/backup/backup-database.sh
	@echo "$(GREEN)Backup completed!$(NC)"

db-restore: ## Restore database (usage: make db-restore FILE=/backups/file.backup.gz)
	@if [ -z "$(FILE)" ]; then \
		echo "$(RED)Usage: make db-restore FILE=/backups/SlipVerificationDb_20240101_120000.backup.gz$(NC)"; \
		exit 1; \
	fi
	@echo "$(RED)⚠️  This will restore the database from backup! Press Ctrl+C to cancel...$(NC)"
	@sleep 5
	./scripts/backup/restore-database.sh $(FILE)

db-connect: ## Connect to database
	docker-compose -f docker-compose.dev.yml exec postgres psql -U postgres -d SlipVerificationDb

##@ SSL/TLS

ssl-setup: ## Setup SSL certificates (usage: make ssl-setup DOMAIN=example.com EMAIL=admin@example.com)
	@if [ -z "$(DOMAIN)" ] || [ -z "$(EMAIL)" ]; then \
		echo "$(RED)Usage: make ssl-setup DOMAIN=example.com EMAIL=admin@example.com$(NC)"; \
		exit 1; \
	fi
	sudo ./scripts/ssl/setup-ssl.sh $(DOMAIN) $(EMAIL)

ssl-renew: ## Renew SSL certificates
	@echo "$(GREEN)Renewing SSL certificates...$(NC)"
	sudo certbot renew
	sudo systemctl reload nginx

##@ Build

build-all: ## Build all Docker images
	@echo "$(GREEN)Building all Docker images...$(NC)"
	docker build -t slip-backend:latest ./slip-verification-api
	docker build -t slip-frontend:latest ./slip-verification-web
	docker build -t slip-ocr:latest ./ocr-service
	@echo "$(GREEN)All images built successfully!$(NC)"

build-backend: ## Build backend image
	docker build -t slip-backend:latest ./slip-verification-api

build-frontend: ## Build frontend image
	docker build -t slip-frontend:latest ./slip-verification-web

build-ocr: ## Build OCR service image
	docker build -t slip-ocr:latest ./ocr-service

##@ Testing

test-all: ## Run all tests
	@echo "$(GREEN)Running all tests...$(NC)"
	@make test-backend
	@make test-frontend
	@make test-ocr

test-backend: ## Run backend tests
	@echo "$(GREEN)Running backend tests...$(NC)"
	cd slip-verification-api && dotnet test

test-frontend: ## Run frontend tests
	@echo "$(GREEN)Running frontend tests...$(NC)"
	cd slip-verification-web && npm test

test-ocr: ## Run OCR service tests
	@echo "$(GREEN)Running OCR service tests...$(NC)"
	cd ocr-service && pytest tests/ || echo "No tests found"

##@ Health Checks

health-check: ## Check health of all services
	@echo "$(GREEN)Checking service health...$(NC)"
	@echo "\n$(YELLOW)Frontend:$(NC)"
	@curl -s -o /dev/null -w "%{http_code}" http://localhost:4200 || echo "Not running"
	@echo "\n$(YELLOW)Backend:$(NC)"
	@curl -s http://localhost:5000/health | jq . || echo "Not running"
	@echo "\n$(YELLOW)OCR:$(NC)"
	@curl -s http://localhost:8000/health || echo "Not running"

##@ Cleanup

clean: ## Clean up Docker resources
	@echo "$(YELLOW)Cleaning up Docker resources...$(NC)"
	docker system prune -f
	@echo "$(GREEN)Cleanup complete!$(NC)"

clean-all: ## Clean up everything (including volumes)
	@echo "$(RED)⚠️  This will remove all Docker resources including volumes! Press Ctrl+C to cancel...$(NC)"
	@sleep 5
	docker system prune -a --volumes -f
	@echo "$(GREEN)Complete cleanup done!$(NC)"

##@ Utilities

logs: ## View all logs
	docker-compose -f docker-compose.dev.yml logs -f

ps: ## List all running containers
	docker-compose -f docker-compose.dev.yml ps

top: ## View container resource usage
	docker stats

version: ## Show version information
	@echo "$(GREEN)Slip Verification System$(NC)"
	@echo "Version: 1.0.0"
	@echo ""
	@echo "Components:"
	@echo "  - Frontend: Angular 20"
	@echo "  - Backend: .NET 9"
	@echo "  - OCR: Python 3.12"
	@echo "  - Database: PostgreSQL 16"
	@echo "  - Cache: Redis 7"

urls: ## Display all service URLs
	@echo "$(GREEN)Service URLs:$(NC)"
	@echo ""
	@echo "$(YELLOW)Development:$(NC)"
	@echo "  Frontend:  http://localhost:4200"
	@echo "  Backend:   http://localhost:5000"
	@echo "  API Docs:  http://localhost:5000/swagger"
	@echo "  OCR:       http://localhost:8000"
	@echo "  OCR Docs:  http://localhost:8000/docs"
	@echo "  pgAdmin:   http://localhost:5050"
	@echo ""
	@echo "$(YELLOW)Monitoring:$(NC)"
	@echo "  Prometheus:    http://localhost:9090"
	@echo "  Grafana:       http://localhost:3000"
	@echo "  Alertmanager:  http://localhost:9093"
