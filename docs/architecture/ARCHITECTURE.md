# Infrastructure Architecture Diagram

## System Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         External Users                               │
│                    (Web Browsers / Mobile Apps)                      │
└───────────────────────────┬─────────────────────────────────────────┘
                            │ HTTPS
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      Load Balancer / CDN                             │
│                    (CloudFlare / AWS ELB)                            │
└───────────────────────────┬─────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   Kubernetes Ingress Controller                      │
│                      (NGINX Ingress)                                 │
│    ┌──────────────────────────────────────────────────────────┐    │
│    │  - SSL/TLS Termination                                    │    │
│    │  - Rate Limiting                                          │    │
│    │  - Load Balancing                                         │    │
│    └──────────────────────────────────────────────────────────┘    │
└─────┬─────────────────┬─────────────────┬─────────────────┬─────────┘
      │                 │                 │                 │
      │ /               │ /api            │ /ocr            │
      ▼                 ▼                 ▼                 │
┌─────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  Frontend   │  │  Backend API │  │ OCR Service  │      │
│  Service    │  │   Service    │  │   Service    │      │
│  (Angular)  │  │   (.NET 9)   │  │  (Python)    │      │
└─────────────┘  └──────────────┘  └──────────────┘      │
      │                 │                 │                 │
      │                 │                 │                 │
      ▼                 ▼                 ▼                 ▼
┌─────────────────────────────────────────────────────────────────────┐
│                  Application Pods (Kubernetes)                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐             │
│  │  Frontend    │  │  Backend     │  │  OCR         │             │
│  │  Replicas:2-5│  │  Replicas:3-10│ │  Replicas:2-8│             │
│  │  HPA Enabled │  │  HPA Enabled │  │  HPA Enabled │             │
│  └──────────────┘  └──────────────┘  └──────────────┘             │
└───────┬───────────────────┬───────────────────┬─────────────────────┘
        │                   │                   │
        │                   ▼                   │
        │          ┌─────────────────┐          │
        │          │  Redis Cache    │          │
        │          │  (Session Store)│          │
        │          └─────────────────┘          │
        │                   │                   │
        └───────────────────┴───────────────────┘
                            │
                            ▼
        ┌───────────────────────────────────────┐
        │     PostgreSQL Database               │
        │     (StatefulSet)                     │
        │  ┌─────────────────────────────────┐  │
        │  │ - Primary/Replica Setup         │  │
        │  │ - Persistent Volume (10Gi)      │  │
        │  │ - Automated Backups             │  │
        │  └─────────────────────────────────┘  │
        └───────────────────────────────────────┘
```

## Monitoring & Observability Stack

```
┌─────────────────────────────────────────────────────────────────────┐
│                     Monitoring Infrastructure                        │
│                                                                      │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐         │
│  │  Prometheus  │───▶│   Grafana    │◀───│ Alertmanager │         │
│  │  (Metrics)   │    │ (Dashboards) │    │  (Alerts)    │         │
│  └──────┬───────┘    └──────────────┘    └──────┬───────┘         │
│         │                                         │                 │
│         │ Scrapes Metrics                        │ Sends Alerts    │
│         ▼                                         ▼                 │
│  ┌──────────────────────────────────────────────────────┐         │
│  │            Application Metrics                        │         │
│  │  - Request Rate, Response Time                       │         │
│  │  - Error Rate, Success Rate                          │         │
│  │  - Database Connections                              │         │
│  │  - Cache Hit Rate                                    │         │
│  └──────────────────────────────────────────────────────┘         │
│                                                                      │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐         │
│  │ Node Exporter│    │ Postgres     │    │ Redis        │         │
│  │ (System)     │    │ Exporter     │    │ Exporter     │         │
│  └──────────────┘    └──────────────┘    └──────────────┘         │
└─────────────────────────────────────────────────────────────────────┘
```

## CI/CD Pipeline Flow

```
┌────────────────────────────────────────────────────────────────────┐
│                      GitHub Repository                              │
└────────┬───────────────────────────────────────────────────────────┘
         │
         │ Push to Branch
         ▼
┌────────────────────────────────────────────────────────────────────┐
│                    GitHub Actions Workflow                          │
│                                                                     │
│  1. Security Scan                                                   │
│     ├─ Trivy Vulnerability Scan                                    │
│     └─ Code Security Analysis                                      │
│                                                                     │
│  2. Test & Build                                                    │
│     ├─ Backend Tests (.NET)                                        │
│     ├─ Frontend Tests (Angular)                                    │
│     ├─ OCR Service Tests (Python)                                  │
│     └─ Code Coverage Reports                                       │
│                                                                     │
│  3. Build Docker Images                                             │
│     ├─ Backend Image                                               │
│     ├─ Frontend Image                                              │
│     └─ OCR Service Image                                           │
│                                                                     │
│  4. Push to Registry                                                │
│     └─ GitHub Container Registry (ghcr.io)                         │
│                                                                     │
│  5. Deploy                                                          │
│     ├─ Development (develop branch)                                │
│     ├─ Staging (staging branch)                                    │
│     └─ Production (main branch)                                    │
│                                                                     │
│  6. Post-Deploy                                                     │
│     ├─ Health Checks                                               │
│     ├─ Smoke Tests                                                 │
│     └─ Rollback on Failure                                         │
└────────────────────────────────────────────────────────────────────┘
         │
         │ Deployment Successful
         ▼
┌────────────────────────────────────────────────────────────────────┐
│                   Kubernetes Cluster                                │
│                   (Production Environment)                          │
└────────────────────────────────────────────────────────────────────┘
```

## Data Flow Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                          User Request Flow                            │
└──────────────────────────────────────────────────────────────────────┘

User Browser
    │
    │ 1. HTTPS Request
    ▼
Nginx Ingress
    │
    │ 2. Route to Service
    ├──────────────┬──────────────┬───────────────┐
    ▼              ▼              ▼               ▼
Frontend       Backend API    OCR Service    Static Files
    │              │              │               │
    │              │ 3. Check     │               │
    │              ├─────────────▶│               │
    │              │    Cache     │               │
    │              ◀─────────────┤               │
    │              │   Redis      │               │
    │              │              │               │
    │              │ 4. Query DB  │               │
    │              ├──────────────┤               │
    │              │              │               │
    │              ▼              │               │
    │         PostgreSQL          │               │
    │              │              │               │
    │              │ 5. OCR       │               │
    │              ├─────────────▶│               │
    │              │  Processing  │               │
    │              ◀─────────────┤               │
    │              │              │               │
    │ 6. Response  │              │               │
    ◀──────────────┤              │               │
    │              │              │               │
    ▼              ▼              ▼               ▼
Return to User
```

## Security Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                       Security Layers                                 │
│                                                                       │
│  Layer 1: Network Security                                           │
│  ┌────────────────────────────────────────────────────────┐         │
│  │ - Firewall Rules                                        │         │
│  │ - DDoS Protection (CloudFlare)                         │         │
│  │ - WAF (Web Application Firewall)                       │         │
│  └────────────────────────────────────────────────────────┘         │
│                                                                       │
│  Layer 2: Transport Security                                         │
│  ┌────────────────────────────────────────────────────────┐         │
│  │ - TLS 1.3 Encryption                                   │         │
│  │ - Let's Encrypt Certificates                           │         │
│  │ - HSTS Headers                                         │         │
│  └────────────────────────────────────────────────────────┘         │
│                                                                       │
│  Layer 3: Application Security                                       │
│  ┌────────────────────────────────────────────────────────┐         │
│  │ - JWT Authentication                                   │         │
│  │ - Rate Limiting                                        │         │
│  │ - CORS Configuration                                   │         │
│  │ - Input Validation                                     │         │
│  └────────────────────────────────────────────────────────┘         │
│                                                                       │
│  Layer 4: Data Security                                              │
│  ┌────────────────────────────────────────────────────────┐         │
│  │ - Encrypted Database Connections                       │         │
│  │ - Encrypted Backups                                    │         │
│  │ - Secret Management (K8s Secrets)                      │         │
│  └────────────────────────────────────────────────────────┘         │
│                                                                       │
│  Layer 5: Container Security                                         │
│  ┌────────────────────────────────────────────────────────┐         │
│  │ - Image Vulnerability Scanning (Trivy)                │         │
│  │ - Non-root Containers                                  │         │
│  │ - Pod Security Policies                                │         │
│  │ - Network Policies                                     │         │
│  └────────────────────────────────────────────────────────┘         │
└──────────────────────────────────────────────────────────────────────┘
```

## Disaster Recovery Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                      Backup & Recovery Strategy                       │
│                                                                       │
│  Primary Site                          Backup Site                   │
│  ┌─────────────────────┐              ┌─────────────────────┐       │
│  │  Production Cluster │              │  DR Cluster         │       │
│  │  ┌────────────────┐ │              │  ┌────────────────┐ │       │
│  │  │ PostgreSQL     │ │──Replication─▶│  │ PostgreSQL     │ │       │
│  │  │ (Primary)      │ │              │  │ (Standby)      │ │       │
│  │  └────────────────┘ │              │  └────────────────┘ │       │
│  └─────────────────────┘              └─────────────────────┘       │
│           │                                                           │
│           │ Automated Backups                                        │
│           ▼                                                           │
│  ┌─────────────────────────────────────────────────────┐            │
│  │              Cloud Storage                           │            │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐   │            │
│  │  │ AWS S3     │  │ Azure Blob │  │ GCS        │   │            │
│  │  │ (Daily)    │  │ (Weekly)   │  │ (Monthly)  │   │            │
│  │  └────────────┘  └────────────┘  └────────────┘   │            │
│  │                                                      │            │
│  │  Retention Policy: 30 days daily, 90 days weekly,  │            │
│  │                   12 months monthly                 │            │
│  └─────────────────────────────────────────────────────┘            │
│                                                                       │
│  Recovery Time Objective (RTO): < 1 hour                             │
│  Recovery Point Objective (RPO): < 15 minutes                        │
└──────────────────────────────────────────────────────────────────────┘
```

## Scaling Strategy

```
┌──────────────────────────────────────────────────────────────────────┐
│                    Auto-Scaling Configuration                         │
│                                                                       │
│  Horizontal Pod Autoscaler (HPA)                                     │
│                                                                       │
│  Backend API                                                          │
│  ┌────────────────────────────────────────────────────┐             │
│  │ Min Replicas: 3                                     │             │
│  │ Max Replicas: 10                                    │             │
│  │ Target CPU: 70%                                     │             │
│  │ Target Memory: 80%                                  │             │
│  └────────────────────────────────────────────────────┘             │
│                                                                       │
│  OCR Service                                                          │
│  ┌────────────────────────────────────────────────────┐             │
│  │ Min Replicas: 2                                     │             │
│  │ Max Replicas: 8                                     │             │
│  │ Target CPU: 75%                                     │             │
│  │ Target Memory: 80%                                  │             │
│  └────────────────────────────────────────────────────┘             │
│                                                                       │
│  Frontend                                                             │
│  ┌────────────────────────────────────────────────────┐             │
│  │ Min Replicas: 2                                     │             │
│  │ Max Replicas: 5                                     │             │
│  │ Target CPU: 70%                                     │             │
│  └────────────────────────────────────────────────────┘             │
│                                                                       │
│  Scaling Behavior                                                     │
│  ┌────────────────────────────────────────────────────┐             │
│  │ Scale Up:                                           │             │
│  │   - 50% increase every 60 seconds                   │             │
│  │   - Or max 2 pods every 60 seconds                 │             │
│  │                                                      │             │
│  │ Scale Down:                                         │             │
│  │   - 50% decrease every 120 seconds                  │             │
│  │   - Or max 1 pod every 120 seconds                 │             │
│  │   - Stabilization window: 5 minutes                │             │
│  └────────────────────────────────────────────────────┘             │
└──────────────────────────────────────────────────────────────────────┘
```

## Technology Stack Summary

```
┌──────────────────────────────────────────────────────────────────────┐
│                         Technology Stack                              │
│                                                                       │
│  Frontend:                                                            │
│  ├─ Angular 20                                                        │
│  ├─ TypeScript 5.6+                                                   │
│  ├─ Tailwind CSS 3                                                    │
│  └─ Nginx (Web Server)                                                │
│                                                                       │
│  Backend:                                                             │
│  ├─ .NET 9                                                            │
│  ├─ Entity Framework Core 9                                           │
│  ├─ MediatR (CQRS)                                                    │
│  └─ Serilog (Logging)                                                 │
│                                                                       │
│  OCR Service:                                                         │
│  ├─ Python 3.12                                                       │
│  ├─ FastAPI                                                           │
│  ├─ PaddleOCR                                                         │
│  └─ OpenCV                                                            │
│                                                                       │
│  Databases:                                                           │
│  ├─ PostgreSQL 16                                                     │
│  └─ Redis 7                                                           │
│                                                                       │
│  Infrastructure:                                                      │
│  ├─ Kubernetes 1.24+                                                  │
│  ├─ Docker & Docker Compose                                           │
│  ├─ Nginx Ingress Controller                                          │
│  └─ Let's Encrypt (SSL/TLS)                                           │
│                                                                       │
│  Monitoring:                                                          │
│  ├─ Prometheus                                                        │
│  ├─ Grafana                                                           │
│  ├─ Alertmanager                                                      │
│  └─ Node Exporter                                                     │
│                                                                       │
│  CI/CD:                                                               │
│  ├─ GitHub Actions                                                    │
│  ├─ GitHub Container Registry                                         │
│  └─ Trivy (Security Scanning)                                         │
└──────────────────────────────────────────────────────────────────────┘
```

## Network Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                      Kubernetes Network                               │
│                                                                       │
│  External Network (Internet)                                         │
│           │                                                           │
│           │ HTTPS (443)                                               │
│           ▼                                                           │
│  ┌────────────────────────────────┐                                  │
│  │   LoadBalancer Service         │                                  │
│  │   (Cloud Provider LB)          │                                  │
│  └────────────────────────────────┘                                  │
│           │                                                           │
│           ▼                                                           │
│  ┌────────────────────────────────┐                                  │
│  │   Ingress Controller           │                                  │
│  │   IP: Cluster External IP      │                                  │
│  └────────────────────────────────┘                                  │
│           │                                                           │
│  ─────────┴──────────────────────────────────────────                │
│  │      Kubernetes Internal Network (ClusterIP)     │                │
│  │                                                   │                │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────┐│                │
│  │  │ Frontend     │  │ Backend      │  │ OCR    ││                │
│  │  │ 10.96.1.x:80 │  │ 10.96.2.x:80 │  │ 10.x   ││                │
│  │  └──────────────┘  └──────┬───────┘  └────┬───┘│                │
│  │                            │                │     │                │
│  │                            ▼                ▼     │                │
│  │                    ┌──────────────┐  ┌─────────┐│                │
│  │                    │ PostgreSQL   │  │ Redis   ││                │
│  │                    │ 10.96.3.x:5432│ │10.96.4.x││                │
│  │                    └──────────────┘  └─────────┘│                │
│  └───────────────────────────────────────────────────                │
│                                                                       │
│  Network Policies:                                                    │
│  ├─ Frontend can only communicate with Backend                       │
│  ├─ Backend can access Database and Cache                            │
│  ├─ OCR Service can access Cache only                                │
│  └─ All services can be accessed by Ingress                          │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Deployment Environments

| Environment | Branch    | URL                              | Auto-Deploy | Approval Required |
|------------|-----------|----------------------------------|-------------|-------------------|
| Development| develop   | dev.slipverification.com         | Yes         | No                |
| Staging    | staging   | staging.slipverification.com     | Yes         | No                |
| Production | main      | slipverification.com             | Yes         | Yes               |

---

## Resource Requirements

### Minimum Requirements (Development)
- **CPU**: 4 cores
- **Memory**: 8 GB RAM
- **Storage**: 50 GB SSD

### Recommended Requirements (Production)
- **CPU**: 16 cores
- **Memory**: 32 GB RAM
- **Storage**: 200 GB SSD (with auto-expansion)
- **Network**: 1 Gbps

---

## High Availability Configuration

- **Frontend**: 2-5 replicas across multiple nodes
- **Backend**: 3-10 replicas across multiple nodes
- **OCR Service**: 2-8 replicas across multiple nodes
- **Database**: Primary with standby replica
- **Cache**: Redis Sentinel for high availability

---

*Last Updated: 2024-01-01*
*Version: 1.0.0*
