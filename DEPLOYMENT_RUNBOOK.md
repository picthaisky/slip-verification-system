# Deployment Runbook

## 📋 Table of Contents

1. [Pre-Deployment](#pre-deployment)
2. [Development Deployment](#development-deployment)
3. [Production Deployment](#production-deployment)
4. [Post-Deployment Verification](#post-deployment-verification)
5. [Rollback Procedures](#rollback-procedures)
6. [Emergency Procedures](#emergency-procedures)
7. [Common Issues](#common-issues)

---

## Pre-Deployment

### Checklist

- [ ] All tests passing
- [ ] Code reviewed and approved
- [ ] Version number updated
- [ ] CHANGELOG.md updated
- [ ] Security scan completed
- [ ] Backup completed
- [ ] Stakeholders notified
- [ ] Deployment window scheduled

### Version Control

```bash
# Check current version
git describe --tags

# Create release tag
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### Pre-Deployment Backup

```bash
# Backup database
./scripts/backup/backup-database.sh

# Verify backup
ls -lh /backups/
```

---

## Development Deployment

### Deploy to Dev Environment

```bash
# 1. Push to develop branch
git checkout develop
git merge feature/your-feature
git push origin develop

# 2. Monitor CI/CD pipeline
# GitHub Actions will automatically:
# - Run tests
# - Build images
# - Deploy to dev cluster

# 3. Check deployment status
kubectl get pods -n slip-verification-dev
kubectl get deployments -n slip-verification-dev
```

### Verify Dev Deployment

```bash
# Check pod status
kubectl get pods -n slip-verification-dev

# Check logs
kubectl logs -f deployment/backend-api -n slip-verification-dev

# Test health endpoint
curl https://dev.slipverification.com/health
```

---

## Production Deployment

### Prerequisites

- All dev tests passed
- PR approved and merged to main
- Deployment window scheduled
- Team on standby

### Deploy to Production

#### Option 1: Automated (CI/CD)

```bash
# 1. Merge to main branch
git checkout main
git merge develop
git push origin main

# 2. CI/CD pipeline will automatically:
# - Run security scans
# - Build production images
# - Deploy to production with rolling update
# - Perform health checks
# - Auto-rollback on failure
```

#### Option 2: Manual Deployment

```bash
# 1. Set environment variables
export TAG=v1.0.0
export REGISTRY=ghcr.io/your-org

# 2. Deploy to Kubernetes
kubectl set image deployment/backend-api \
  backend=${REGISTRY}/backend:${TAG} \
  -n slip-verification

kubectl set image deployment/frontend \
  frontend=${REGISTRY}/frontend:${TAG} \
  -n slip-verification

kubectl set image deployment/ocr-service \
  ocr=${REGISTRY}/ocr-service:${TAG} \
  -n slip-verification

# 3. Monitor rollout
kubectl rollout status deployment/backend-api -n slip-verification
kubectl rollout status deployment/frontend -n slip-verification
kubectl rollout status deployment/ocr-service -n slip-verification
```

### Blue-Green Deployment (Advanced)

```bash
# 1. Deploy green version
kubectl apply -f infrastructure/kubernetes/overlays/prod/green/

# 2. Wait for green to be ready
kubectl wait --for=condition=ready pod \
  -l version=green -n slip-verification --timeout=300s

# 3. Test green deployment
kubectl port-forward service/backend-service-green 8080:8080 -n slip-verification
curl http://localhost:8080/health

# 4. Switch traffic to green
kubectl patch service backend-service -n slip-verification \
  -p '{"spec":{"selector":{"version":"green"}}}'

# 5. Monitor for issues (5-10 minutes)
kubectl logs -f -l version=green -n slip-verification

# 6. Scale down blue deployment
kubectl scale deployment/backend-api-blue --replicas=0 -n slip-verification
```

---

## Post-Deployment Verification

### Health Checks

```bash
# 1. Check pod status
kubectl get pods -n slip-verification

# Expected output:
# NAME                           READY   STATUS    RESTARTS   AGE
# backend-api-xxxxx              1/1     Running   0          2m
# frontend-xxxxx                 1/1     Running   0          2m
# ocr-service-xxxxx              1/1     Running   0          2m

# 2. Check services
kubectl get services -n slip-verification

# 3. Test endpoints
curl https://slipverification.com/health
curl https://api.slipverification.com/api/v1/health

# Expected response:
# {"status":"Healthy","checks":[...]}
```

### Application Tests

```bash
# 1. Frontend smoke test
curl -I https://slipverification.com
# Expected: HTTP/2 200

# 2. Backend API test
curl https://api.slipverification.com/api/v1/health
# Expected: {"status":"Healthy"}

# 3. OCR service test
curl https://api.slipverification.com/ocr/health
# Expected: {"status":"ok"}

# 4. Database connectivity
kubectl exec -it deployment/backend-api -n slip-verification -- \
  sh -c 'curl http://localhost:8080/health'
```

### Monitoring Checks

```bash
# 1. Check Prometheus targets
open http://prometheus.your-domain.com/targets

# 2. Check Grafana dashboards
open http://grafana.your-domain.com/dashboards

# 3. Check logs
kubectl logs -f deployment/backend-api -n slip-verification --tail=100

# 4. Check metrics
kubectl top pods -n slip-verification
```

### Performance Verification

```bash
# 1. Response time check
time curl https://slipverification.com

# 2. Load test (optional)
ab -n 100 -c 10 https://api.slipverification.com/health

# 3. Database performance
kubectl exec -it statefulset/postgres -n slip-verification -- \
  psql -U postgres -d SlipVerificationDb -c "SELECT COUNT(*) FROM \"Orders\";"
```

---

## Rollback Procedures

### Automatic Rollback

CI/CD pipeline automatically rolls back if:
- Health checks fail
- Pods don't reach ready state
- Error rate exceeds threshold

### Manual Rollback

#### Quick Rollback (Last Version)

```bash
# 1. Rollback all services
kubectl rollout undo deployment/backend-api -n slip-verification
kubectl rollout undo deployment/frontend -n slip-verification
kubectl rollout undo deployment/ocr-service -n slip-verification

# 2. Check rollout status
kubectl rollout status deployment/backend-api -n slip-verification
```

#### Rollback to Specific Version

```bash
# 1. Check deployment history
kubectl rollout history deployment/backend-api -n slip-verification

# Output:
# REVISION  CHANGE-CAUSE
# 1         Initial deployment
# 2         Update to v1.0.0
# 3         Update to v1.0.1

# 2. Rollback to specific revision
kubectl rollout undo deployment/backend-api \
  --to-revision=2 -n slip-verification

# 3. Verify rollback
kubectl rollout status deployment/backend-api -n slip-verification
```

#### Database Rollback

```bash
# 1. Stop application
kubectl scale deployment/backend-api --replicas=0 -n slip-verification

# 2. Restore database
./scripts/backup/restore-database.sh /backups/SlipVerificationDb_TIMESTAMP.backup.gz

# 3. Restart application
kubectl scale deployment/backend-api --replicas=3 -n slip-verification
```

---

## Emergency Procedures

### Service Outage

#### 1. Immediate Response

```bash
# Check pod status
kubectl get pods -n slip-verification

# Check events
kubectl get events -n slip-verification --sort-by='.lastTimestamp'

# Quick restart if needed
kubectl rollout restart deployment/backend-api -n slip-verification
```

#### 2. Database Issues

```bash
# Check database logs
kubectl logs -f statefulset/postgres -n slip-verification

# Check connections
kubectl exec -it statefulset/postgres -n slip-verification -- \
  psql -U postgres -c "SELECT count(*) FROM pg_stat_activity;"

# Restart database (last resort)
kubectl delete pod postgres-0 -n slip-verification
```

#### 3. High Memory/CPU

```bash
# Check resource usage
kubectl top pods -n slip-verification

# Scale up replicas
kubectl scale deployment/backend-api --replicas=5 -n slip-verification

# Check HPA status
kubectl get hpa -n slip-verification
```

### Security Incident

```bash
# 1. Isolate affected pods
kubectl cordon <node-name>

# 2. Collect logs
kubectl logs deployment/backend-api -n slip-verification > incident-logs.txt

# 3. Rotate secrets
kubectl delete secret backend-secret -n slip-verification
kubectl create secret generic backend-secret \
  --from-literal=JWT_SECRET=new-secret \
  -n slip-verification

# 4. Force restart
kubectl rollout restart deployment/backend-api -n slip-verification
```

---

## Common Issues

### Issue 1: Pods Not Starting

**Symptoms:**
```
kubectl get pods -n slip-verification
NAME                       READY   STATUS             RESTARTS   AGE
backend-api-xxxxx          0/1     CrashLoopBackOff   3          2m
```

**Solution:**
```bash
# 1. Check logs
kubectl logs backend-api-xxxxx -n slip-verification

# 2. Check events
kubectl describe pod backend-api-xxxxx -n slip-verification

# 3. Common causes:
# - Missing environment variables
# - Database connection issues
# - Resource limits too low
```

### Issue 2: Image Pull Errors

**Symptoms:**
```
Events:
  Warning  Failed     1m    kubelet  Failed to pull image "registry/image:tag"
```

**Solution:**
```bash
# 1. Check image exists
docker pull ghcr.io/your-org/backend:v1.0.0

# 2. Verify image pull secret
kubectl get secrets -n slip-verification

# 3. Create/update secret
kubectl create secret docker-registry regcred \
  --docker-server=ghcr.io \
  --docker-username=your-username \
  --docker-password=your-token \
  -n slip-verification
```

### Issue 3: High Error Rate

**Symptoms:**
- Prometheus alert: HighErrorRate
- Grafana shows increased 5xx responses

**Solution:**
```bash
# 1. Check application logs
kubectl logs -f deployment/backend-api -n slip-verification

# 2. Check database connectivity
kubectl exec -it deployment/backend-api -n slip-verification -- \
  curl http://localhost:8080/health

# 3. Scale up if needed
kubectl scale deployment/backend-api --replicas=5 -n slip-verification

# 4. Rollback if persistent
kubectl rollout undo deployment/backend-api -n slip-verification
```

### Issue 4: Database Connection Pool Exhausted

**Symptoms:**
- Errors: "Connection pool exhausted"
- High number of connections

**Solution:**
```bash
# 1. Check connection count
kubectl exec -it statefulset/postgres -n slip-verification -- \
  psql -U postgres -c "SELECT count(*) FROM pg_stat_activity;"

# 2. Kill idle connections
kubectl exec -it statefulset/postgres -n slip-verification -- \
  psql -U postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE state = 'idle';"

# 3. Restart backend pods
kubectl rollout restart deployment/backend-api -n slip-verification
```

---

## Contact Information

### On-Call Team

- **Primary**: DevOps Team - devops@slipverification.com
- **Secondary**: Backend Team - backend@slipverification.com
- **Emergency**: +66-xxx-xxx-xxxx

### Escalation Path

1. Level 1: On-call engineer
2. Level 2: Team lead
3. Level 3: CTO

### Communication Channels

- **Slack**: #incidents
- **Phone**: Emergency hotline
- **Email**: ops-team@slipverification.com

---

## Additional Resources

- [Infrastructure Documentation](./README.md)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Dashboards](http://grafana.your-domain.com)
- [CI/CD Pipeline](.github/workflows/ci-cd.yml)

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | 2024-01-01 | DevOps Team | Initial version |
