# DevOps Infrastructure Documentation

## 🏗️ Infrastructure Overview

This repository includes a complete DevOps setup for the Slip Verification System with:

- **Docker & Docker Compose** - Containerized development and production environments
- **Kubernetes (K8s)** - Production-grade container orchestration
- **GitHub Actions** - Automated CI/CD pipeline
- **Nginx** - Reverse proxy and load balancing
- **Prometheus & Grafana** - Monitoring and alerting
- **Automated Backups** - Database backup and restore
- **SSL/TLS** - Automated certificate management

## 📁 Directory Structure

```
infrastructure/
├── kubernetes/           # Kubernetes manifests
│   ├── base/            # Base configurations
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   ├── secrets.yaml
│   │   ├── backend-deployment.yaml
│   │   ├── frontend-deployment.yaml
│   │   ├── ocr-deployment.yaml
│   │   ├── postgres-statefulset.yaml
│   │   ├── redis-deployment.yaml
│   │   ├── ingress.yaml
│   │   ├── hpa.yaml
│   │   └── pvc.yaml
│   └── overlays/        # Environment-specific overlays
│       ├── dev/
│       ├── staging/
│       └── prod/
├── nginx/               # Nginx configurations
│   ├── nginx.conf       # Development config
│   ├── nginx.prod.conf  # Production config
│   └── conf.d/          # Site configurations
│       └── default.conf
├── monitoring/          # Monitoring configurations
│   ├── prometheus/
│   │   ├── prometheus.yml
│   │   └── alerts.yml
│   └── grafana/
│       └── dashboards/
└── ssl/                 # SSL certificates

scripts/
├── backup/              # Backup scripts
│   ├── backup-database.sh
│   └── restore-database.sh
└── ssl/                 # SSL scripts
    └── setup-ssl.sh

.github/
└── workflows/           # CI/CD workflows
    └── ci-cd.yml

docker-compose.dev.yml   # Development environment
docker-compose.prod.yml  # Production environment
```

## 🚀 Quick Start

### Development Environment

```bash
# Start all services
docker-compose -f docker-compose.dev.yml up -d

# Check status
docker-compose -f docker-compose.dev.yml ps

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Stop all services
docker-compose -f docker-compose.dev.yml down
```

### Production Environment

```bash
# Set environment variables
export REGISTRY=ghcr.io/your-org
export TAG=v1.0.0
export JWT_SECRET=your-secret-key
export DB_PASSWORD=your-db-password
export REDIS_PASSWORD=your-redis-password

# Start production stack
docker-compose -f docker-compose.prod.yml up -d
```

## ☸️ Kubernetes Deployment

### Prerequisites

- Kubernetes cluster (v1.24+)
- kubectl configured
- Helm (optional)

### Deploy to Kubernetes

```bash
# Create namespace
kubectl apply -f infrastructure/kubernetes/base/namespace.yaml

# Apply secrets (update with real values first!)
kubectl apply -f infrastructure/kubernetes/base/secrets.yaml

# Apply configurations
kubectl apply -f infrastructure/kubernetes/base/configmap.yaml

# Deploy databases
kubectl apply -f infrastructure/kubernetes/base/postgres-statefulset.yaml
kubectl apply -f infrastructure/kubernetes/base/redis-deployment.yaml

# Wait for databases to be ready
kubectl wait --for=condition=ready pod -l app=postgres -n slip-verification --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis -n slip-verification --timeout=300s

# Deploy applications
kubectl apply -f infrastructure/kubernetes/base/backend-deployment.yaml
kubectl apply -f infrastructure/kubernetes/base/frontend-deployment.yaml
kubectl apply -f infrastructure/kubernetes/base/ocr-deployment.yaml

# Deploy ingress and HPA
kubectl apply -f infrastructure/kubernetes/base/ingress.yaml
kubectl apply -f infrastructure/kubernetes/base/hpa.yaml

# Check deployment status
kubectl get pods -n slip-verification
kubectl get services -n slip-verification
kubectl get ingress -n slip-verification
```

### Update Deployment

```bash
# Update image tag
kubectl set image deployment/backend-api \
  backend=ghcr.io/your-org/backend:v1.0.1 \
  -n slip-verification

# Check rollout status
kubectl rollout status deployment/backend-api -n slip-verification

# Rollback if needed
kubectl rollout undo deployment/backend-api -n slip-verification
```

## 🔄 CI/CD Pipeline

The GitHub Actions workflow (`.github/workflows/ci-cd.yml`) automatically:

1. **Security Scan** - Trivy vulnerability scanning
2. **Test** - Run unit and integration tests
3. **Build** - Build Docker images
4. **Push** - Push to container registry
5. **Deploy** - Deploy to Kubernetes
6. **Health Check** - Verify deployment
7. **Rollback** - Auto-rollback on failure

### Required Secrets

Configure these secrets in GitHub Settings → Secrets:

- `KUBE_CONFIG_DEV` - Base64 encoded kubeconfig for dev
- `KUBE_CONFIG_PROD` - Base64 encoded kubeconfig for prod
- `CODECOV_TOKEN` - Codecov token (optional)
- `SLACK_WEBHOOK` - Slack webhook for notifications

### Trigger Deployment

```bash
# Deploy to development
git push origin develop

# Deploy to production
git push origin main
```

## 📊 Monitoring

### Prometheus

Access Prometheus at: `http://prometheus.your-domain.com`

Default configuration monitors:
- Application metrics
- Database performance
- System resources
- Kubernetes cluster

### Grafana

Access Grafana at: `http://grafana.your-domain.com`

Pre-configured dashboards for:
- Application overview
- Database metrics
- Infrastructure health
- Alert status

### Alerts

Prometheus alerts are configured in `infrastructure/monitoring/prometheus/alerts.yml`:

- High CPU/Memory usage
- Service downtime
- High error rates
- Database issues
- Pod restarts
- HPA at max capacity

## 💾 Backup & Restore

### Database Backup

```bash
# Manual backup
./scripts/backup/backup-database.sh

# With S3 upload
S3_BUCKET=my-backup-bucket \
DB_PASSWORD=your-password \
./scripts/backup/backup-database.sh

# With encryption
ENCRYPTION_KEY=your-encryption-key \
DB_PASSWORD=your-password \
./scripts/backup/backup-database.sh
```

### Automated Backups

Set up cron job:

```bash
# Daily backup at 2 AM
0 2 * * * /path/to/scripts/backup/backup-database.sh >> /var/log/backup.log 2>&1
```

### Restore Database

```bash
# Restore from backup
./scripts/backup/restore-database.sh /backups/SlipVerificationDb_20240101_120000.backup.gz

# With decryption
ENCRYPTION_KEY=your-encryption-key \
./scripts/backup/restore-database.sh /backups/SlipVerificationDb_20240101_120000.backup.gz.enc
```

## 🔒 SSL/TLS Configuration

### Setup SSL with Let's Encrypt

```bash
# Run SSL setup script
sudo ./scripts/ssl/setup-ssl.sh slipverification.com admin@slipverification.com

# The script will:
# 1. Install Certbot
# 2. Obtain certificates
# 3. Configure Nginx
# 4. Setup auto-renewal
```

### Manual Certificate Renewal

```bash
# Renew certificates
sudo certbot renew

# Reload Nginx
sudo systemctl reload nginx
```

## 🔐 Security Best Practices

### Secrets Management

1. **Never commit secrets** to version control
2. Use **Kubernetes Secrets** for sensitive data
3. Consider using **HashiCorp Vault** or **sealed-secrets**
4. Rotate secrets regularly

### Update secrets.yaml:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: backend-secret
  namespace: slip-verification
type: Opaque
stringData:
  JWT_SECRET: "<generate-with-openssl-rand-base64-32>"
  DB_CONNECTION_STRING: "Host=postgres-service;Port=5432;Database=SlipVerificationDb;Username=postgres;Password=<strong-password>"
  REDIS_CONNECTION_STRING: "redis-service:6379,password=<strong-password>"
```

### Network Policies

Apply network policies to restrict pod-to-pod communication:

```bash
kubectl apply -f infrastructure/kubernetes/base/network-policy.yaml
```

### Image Scanning

Images are automatically scanned with Trivy in CI/CD pipeline.

## 📈 Scaling

### Manual Scaling

```bash
# Scale backend API
kubectl scale deployment/backend-api --replicas=5 -n slip-verification

# Scale OCR service
kubectl scale deployment/ocr-service --replicas=3 -n slip-verification
```

### Auto-Scaling (HPA)

HPA is configured in `infrastructure/kubernetes/base/hpa.yaml`:

- **Backend API**: 3-10 replicas (70% CPU, 80% Memory)
- **OCR Service**: 2-8 replicas (75% CPU, 80% Memory)
- **Frontend**: 2-5 replicas (70% CPU)

```bash
# Check HPA status
kubectl get hpa -n slip-verification

# Describe HPA
kubectl describe hpa backend-hpa -n slip-verification
```

## 🔍 Troubleshooting

### View Logs

```bash
# Application logs
kubectl logs -f deployment/backend-api -n slip-verification

# Previous crashed pod logs
kubectl logs --previous deployment/backend-api -n slip-verification

# All pods logs
kubectl logs -f -l app=backend-api -n slip-verification
```

### Debug Pod

```bash
# Get pod details
kubectl describe pod <pod-name> -n slip-verification

# Execute command in pod
kubectl exec -it <pod-name> -n slip-verification -- /bin/bash

# Check events
kubectl get events -n slip-verification --sort-by='.lastTimestamp'
```

### Health Checks

```bash
# Check service health
kubectl get pods -n slip-verification
kubectl get services -n slip-verification

# Test endpoint
curl http://your-domain.com/health
```

### Common Issues

#### Pods not starting
```bash
# Check pod status
kubectl describe pod <pod-name> -n slip-verification

# Check resource limits
kubectl top pods -n slip-verification
```

#### Database connection issues
```bash
# Check database pod
kubectl logs -f statefulset/postgres -n slip-verification

# Test connection
kubectl exec -it <backend-pod> -n slip-verification -- \
  psql -h postgres-service -U postgres -d SlipVerificationDb
```

#### Image pull errors
```bash
# Check image pull secret
kubectl get secrets -n slip-verification

# Verify image exists
docker pull ghcr.io/your-org/backend:tag
```

## 📝 Runbook

### Deployment Checklist

- [ ] Update version in code
- [ ] Run tests locally
- [ ] Update CHANGELOG.md
- [ ] Create release tag
- [ ] Push to develop branch (deploys to dev)
- [ ] Verify dev deployment
- [ ] Create PR to main
- [ ] Merge to main (deploys to prod)
- [ ] Monitor deployment
- [ ] Verify health checks
- [ ] Check logs for errors
- [ ] Update documentation

### Rollback Procedure

```bash
# Automatic rollback (if health check fails)
# CI/CD pipeline handles this automatically

# Manual rollback
kubectl rollout undo deployment/backend-api -n slip-verification
kubectl rollout undo deployment/frontend -n slip-verification
kubectl rollout undo deployment/ocr-service -n slip-verification

# Rollback to specific revision
kubectl rollout history deployment/backend-api -n slip-verification
kubectl rollout undo deployment/backend-api --to-revision=2 -n slip-verification
```

### Incident Response

1. **Identify** - Check monitoring alerts
2. **Assess** - Review logs and metrics
3. **Respond** - Apply fix or rollback
4. **Verify** - Confirm resolution
5. **Document** - Update runbook

## 🌐 Multi-Environment Setup

### Development
- Branch: `develop`
- URL: `https://dev.slipverification.com`
- Auto-deploy: Yes
- Namespace: `slip-verification-dev`

### Staging (Optional)
- Branch: `staging`
- URL: `https://staging.slipverification.com`
- Auto-deploy: Yes
- Namespace: `slip-verification-staging`

### Production
- Branch: `main`
- URL: `https://slipverification.com`
- Auto-deploy: Yes (with approval)
- Namespace: `slip-verification`

## 🔗 Useful Links

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Nginx Documentation](https://nginx.org/en/docs/)
- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)

## 📞 Support

For issues or questions:
- Create an issue in GitHub
- Contact DevOps team
- Check monitoring dashboards
- Review logs in Grafana/Prometheus

## 📄 License

See LICENSE file in repository root.
