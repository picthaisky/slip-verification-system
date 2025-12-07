# Makefile for Slip Verification System
# Quick commands for development and deployment

.PHONY: help dev up down build test clean logs

# Default target
help:
	@echo "Slip Verification System - Available Commands"
	@echo "============================================="
	@echo ""
	@echo "Development:"
	@echo "  make dev         - Start development environment"
	@echo "  make up          - Start full stack with monitoring"
	@echo "  make down        - Stop all services"
	@echo "  make logs        - View service logs"
	@echo ""
	@echo "Testing:"
	@echo "  make test        - Run all tests"
	@echo "  make test-api    - Run API tests"
	@echo "  make test-web    - Run Web tests"
	@echo "  make test-ocr    - Run OCR tests"
	@echo "  make test-mobile - Run Mobile tests"
	@echo ""
	@echo "Building:"
	@echo "  make build       - Build all Docker images"
	@echo "  make clean       - Remove all containers and volumes"
	@echo ""

# ============================================
# Development
# ============================================
dev:
	cd infrastructure && docker-compose -f docker-compose.dev.yml up -d
	@echo "Development environment started!"
	@echo "  API: http://localhost:5000"
	@echo "  OCR: http://localhost:8000"

up:
	cd infrastructure && docker-compose up -d
	@echo "Full stack started!"
	@echo "  Nginx:      http://localhost"
	@echo "  API:        http://localhost:5000"
	@echo "  Web:        http://localhost:4200"
	@echo "  OCR:        http://localhost:8000"
	@echo "  Prometheus: http://localhost:9090"
	@echo "  Grafana:    http://localhost:3000"

down:
	cd infrastructure && docker-compose down
	cd infrastructure && docker-compose -f docker-compose.dev.yml down

logs:
	cd infrastructure && docker-compose logs -f

# ============================================
# Testing
# ============================================
test: test-api test-web test-ocr test-mobile

test-api:
	cd slip-verification-api && dotnet test

test-web:
	cd slip-verification-web && npm run test

test-ocr:
	cd ocr-service && pytest -v

test-mobile:
	cd slip-verification-mobile && npm test

# ============================================
# Building
# ============================================
build:
	docker-compose -f infrastructure/docker-compose.yml build

build-api:
	docker build -t slip-api:latest ./slip-verification-api

build-web:
	docker build -t slip-web:latest ./slip-verification-web

build-ocr:
	docker build -t slip-ocr:latest ./ocr-service

# ============================================
# Cleanup
# ============================================
clean:
	cd infrastructure && docker-compose down -v --remove-orphans
	cd infrastructure && docker-compose -f docker-compose.dev.yml down -v --remove-orphans
	docker system prune -f

# ============================================
# Kubernetes
# ============================================
k8s-apply:
	kubectl apply -f infrastructure/kubernetes/base/

k8s-delete:
	kubectl delete -f infrastructure/kubernetes/base/

k8s-status:
	kubectl get pods -n slip-verification
