# Nginx Architecture Diagram

```
                                    Internet
                                       │
                                       │
                            ┌──────────▼──────────┐
                            │   Port 80/443       │
                            │   Nginx Reverse     │
                            │   Proxy & Load      │
                            │   Balancer          │
                            └──────────┬──────────┘
                                       │
                    ┌──────────────────┼──────────────────┐
                    │                  │                  │
         ┌──────────▼─────────┐ ┌─────▼──────┐  ┌───────▼──────┐
         │   Frontend         │ │    API     │  │  OCR Service │
         │   (Angular)        │ │  Backend   │  │   (Python)   │
         │   Port 80          │ │  Port 8080 │  │  Port 8000   │
         └────────────────────┘ └─────┬──────┘  └──────────────┘
                                      │
                         ┌────────────┼────────────┐
                         │            │            │
                    ┌────▼────┐  ┌───▼────┐  ┌───▼────┐
                    │  API 1  │  │ API 2  │  │ API 3  │
                    │  :5000  │  │ :5000  │  │ :5000  │
                    └─────────┘  └────────┘  └────────┘
                         │            │            │
                         └────────────┼────────────┘
                                      │
                         ┌────────────┼────────────┐
                         │            │            │
                    ┌────▼────┐  ┌───▼────┐  ┌───▼─────┐
                    │PostgreSQL│ │ Redis  │  │ Storage │
                    │   :5432  │ │ :6379  │  │ /uploads│
                    └──────────┘ └────────┘  └─────────┘
```

## Request Flow

### 1. Frontend Request
```
Browser → Nginx:443 (HTTPS)
        → Frontend:80
        → Angular SPA
        → Browser
```

### 2. API Request (with Load Balancing)
```
Browser → Nginx:443/api (HTTPS)
        → Load Balancer (least_conn)
        → API 1/2/3:8080 (selected)
        → PostgreSQL/Redis
        → Response → Nginx → Browser
```

### 3. File Upload
```
Browser → Nginx:443/api/slips/verify (HTTPS)
        → Rate Limiter (10r/m)
        → Upload Handler (10MB max)
        → API Backend:8080
        → Storage (/uploads)
        → Response → Browser
```

### 4. WebSocket Connection
```
Browser → Nginx:443/socket.io (WSS)
        → WebSocket Upgrade
        → API Backend:8080
        → Socket.IO Connection
        → Real-time Updates
```

### 5. OCR Processing
```
Browser → Nginx:443/ocr (HTTPS)
        → OCR Service:8000
        → Python FastAPI
        → Image Processing
        → Response → Browser
```

## Load Balancing Strategies

### Least Connections (Default for API)
```
┌─────────┐
│ Request │
└────┬────┘
     │
     ▼
┌─────────────────┐
│ Load Balancer   │
│ (least_conn)    │
└────┬────────────┘
     │
     ├─ Select server with fewest active connections
     │
     ▼
┌────────────────────────────┐
│ API 1: 5 connections  ◄─── Selected!
│ API 2: 10 connections
│ API 3: 8 connections
└────────────────────────────┘
```

### Round Robin
```
Request 1 → API 1
Request 2 → API 2
Request 3 → API 3
Request 4 → API 1  (repeat)
```

### IP Hash (Sticky Sessions)
```
Client IP: 192.168.1.100 → hash → Always API 1
Client IP: 192.168.1.101 → hash → Always API 2
Client IP: 192.168.1.102 → hash → Always API 3
```

## Rate Limiting Flow

```
┌─────────┐
│ Request │
└────┬────┘
     │
     ▼
┌─────────────────────┐
│ Rate Limiter Check  │
│ (per IP)            │
└────┬────────────────┘
     │
     ├─ Within limit? ─────► Allow ──► Process ──► Response
     │                                      
     └─ Exceeded limit? ──► Deny ──► 429 Too Many Requests
```

### API Endpoints
- Rate: 100 requests/minute
- Burst: 20 requests
- Zone: 10MB

### Upload Endpoints
- Rate: 10 requests/minute
- Burst: 5 requests
- Zone: 10MB

## SSL/TLS Flow

```
┌─────────┐
│ Browser │
└────┬────┘
     │
     ▼ HTTP (Port 80)
┌────────────┐
│   Nginx    │
│  Redirect  │
└────┬───────┘
     │
     ▼ 301 Redirect
┌────────────┐
│  Browser   │
└────┬───────┘
     │
     ▼ HTTPS (Port 443)
┌─────────────────┐
│ Nginx SSL/TLS   │
│ - Verify cert   │
│ - Decrypt       │
└────┬────────────┘
     │
     ▼ HTTP (Internal)
┌─────────────────┐
│ Backend Service │
└─────────────────┘
```

## Security Layers

```
┌──────────────────────────────────────┐
│          Internet (Untrusted)        │
└────────────────┬─────────────────────┘
                 │
         ┌───────▼────────┐
         │  Rate Limiting │  ◄── 100 req/min
         └───────┬────────┘
                 │
         ┌───────▼────────┐
         │   SSL/TLS      │  ◄── TLSv1.2/1.3
         └───────┬────────┘
                 │
         ┌───────▼────────┐
         │ Security       │  ◄── Headers (HSTS, CSP, etc.)
         │ Headers        │
         └───────┬────────┘
                 │
         ┌───────▼────────┐
         │ Request        │  ◄── Validation, Size limits
         │ Validation     │
         └───────┬────────┘
                 │
         ┌───────▼────────┐
         │ Backend        │  ◄── Internal network
         │ Services       │
         └────────────────┘
```

## Monitoring & Logging

```
┌─────────────┐
│   Requests  │
└──────┬──────┘
       │
       ├──► Access Log ──► File ──► Filebeat ──► Elasticsearch
       │                                              │
       │                                              ▼
       ├──► Error Log ───► File ──► Filebeat ──► Kibana Dashboard
       │
       └──► Metrics ─────► Prometheus ──► Grafana Dashboard
                              │
                              └──► Alertmanager ──► Notifications
```

### Metrics Collected
- Request rate
- Response time
- Error rate (4xx, 5xx)
- Upstream health
- Connection count
- Bandwidth usage

## Health Check System

```
┌──────────────┐
│ Docker       │
│ Health Check │
└──────┬───────┘
       │ Every 30s
       ▼
┌──────────────┐
│ Nginx        │
│ /health      │
└──────┬───────┘
       │
       ├─ OK (200) ──► Service Healthy
       │
       └─ Fail ──► Retry (3 times) ──► Mark Unhealthy ──► Restart Container
```

## Caching Strategy

```
┌─────────────────────────────────────┐
│           Static Assets             │
│  (.js, .css, .png, .svg, etc.)     │
│        Cache: 1 year                │
│     Cache-Control: immutable        │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│           HTML Files                │
│       Cache: no-cache               │
│     (Always revalidate)             │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│           API Responses             │
│       Cache: bypass                 │
│     (No caching)                    │
└─────────────────────────────────────┘
```

## Configuration Hierarchy

```
nginx.conf (Main)
├── worker_processes: auto
├── worker_connections: 4096
├── Rate limiting zones
├── Upstream definitions
│   ├── api_backend (least_conn)
│   ├── frontend_backend
│   └── ocr_backend
└── Include: /etc/nginx/conf.d/*.conf
    │
    ├── default.conf (HTTP - Development)
    │   ├── Health check: /health
    │   ├── Frontend: /
    │   ├── API: /api/
    │   ├── Upload: /api/slips/verify
    │   ├── WebSocket: /socket.io/
    │   └── OCR: /ocr/
    │
    ├── api.conf (HTTPS - Production)
    │   ├── SSL/TLS termination
    │   ├── API endpoints
    │   ├── File upload
    │   └── WebSocket (SignalR)
    │
    └── frontend.conf (HTTPS - Production)
        ├── Static assets
        ├── Angular routing
        └── API proxy
```

## Deployment Modes

### Development Mode
- HTTP only (port 80)
- CORS enabled
- Detailed logging
- No SSL required
- Single backend instance

### Production Mode
- HTTPS (port 443)
- HTTP redirect to HTTPS
- SSL/TLS termination
- HSTS enabled
- Multiple backend instances
- Rate limiting enforced
- Monitoring enabled

---

**Legend:**
- `→` Request flow
- `◄──` Configuration/Feature
- `├──` Branch/Option
- `└──` End/Result
- `▼` Next step
