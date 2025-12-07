# DevOps Quick Start Guide

## 🚀 Getting Started in 5 Minutes

### Prerequisites

Install the following on your machine:
- Docker & Docker Compose
- kubectl (for Kubernetes deployments)
- make (for using Makefile commands)

### Option 1: Development with Docker Compose (Easiest)

```bash
# 1. Start all services
make dev-up

# 2. Access the applications
# Frontend: http://localhost:4200
# Backend API: http://localhost:5000
# OCR Service: http://localhost:8000
# Swagger: http://localhost:5000/swagger
# pgAdmin: http://localhost:5050

# 3. View logs
make dev-logs

# 4. Stop services
make dev-down
```

### Option 2: Production with Docker Compose

```bash
# 1. Copy environment template
cp .env.production.example .env

# 2. Edit .env and set your production values
nano .env

# 3. Start production stack
make prod-up

# 4. Check status
docker-compose -f docker-compose.prod.yml ps
```

### Option 3: Kubernetes Deployment

```bash
# 1. Update secrets first!
nano infrastructure/kubernetes/base/secrets.yaml

# 2. Deploy to Kubernetes
make k8s-apply

# 3. Check deployment status
make k8s-status

# 4. Access services
kubectl port-forward service/frontend-service 8080:80 -n slip-verification
```

## 📊 Start Monitoring

```bash
# Start Prometheus and Grafana
make monitoring-up

# Access dashboards
# Prometheus: http://localhost:9090
# Grafana: http://localhost:3000 (admin/admin)
# Alertmanager: http://localhost:9093
```

## 🔐 Setup SSL/TLS

```bash
# On your server
sudo make ssl-setup DOMAIN=slipverification.com EMAIL=admin@slipverification.com
```

## 💾 Database Backup

```bash
# Manual backup
make db-backup

# Automated backup (cron)
0 2 * * * cd /path/to/project && make db-backup >> /var/log/backup.log 2>&1
```

## 🔍 Useful Commands

```bash
# View all available commands
make help

# Check service health
make health-check

# View all service URLs
make urls

# Build Docker images
make build-all

# Run tests
make test-all

# Clean up Docker resources
make clean
```

## 🐛 Troubleshooting

### Services not starting?

```bash
# Check logs
make dev-logs

# Restart services
make dev-restart

# Clean and restart
make dev-clean
make dev-up
```

### Database connection issues?

```bash
# Connect to database
make db-connect

# Check database logs
docker-compose -f docker-compose.dev.yml logs postgres
```

### Kubernetes pods not ready?

```bash
# Check pod status
kubectl get pods -n slip-verification

# View pod logs
make k8s-logs POD=backend-api

# Describe pod for events
kubectl describe pod <pod-name> -n slip-verification
```

## 📚 Next Steps

1. **Read full documentation**: [infrastructure/README.md](infrastructure/README.md)
2. **Review deployment runbook**: [DEPLOYMENT_RUNBOOK.md](DEPLOYMENT_RUNBOOK.md)
3. **Check architecture diagram**: [ARCHITECTURE.md](ARCHITECTURE.md)
4. **Configure CI/CD**: Review `.github/workflows/ci-cd.yml`
5. **Setup monitoring alerts**: Configure `infrastructure/monitoring/alertmanager/alertmanager.yml`

## 🆘 Get Help

- Check documentation in `infrastructure/` directory
- Review logs: `make logs` or `make k8s-logs POD=<pod-name>`
- Contact DevOps team

## 🔗 Important Links

- [Full Infrastructure Documentation](infrastructure/README.md)
- [Deployment Runbook](DEPLOYMENT_RUNBOOK.md)
- [Architecture Diagram](ARCHITECTURE.md)
- [GitHub Actions Workflows](.github/workflows/)
- [Kubernetes Manifests](infrastructure/kubernetes/)

---

**Made with ❤️ by the DevOps Team**
