# Nginx Configuration for Slip Verification System

This directory contains complete Nginx configuration for reverse proxy, load balancing, and API gateway for the Slip Verification System.

## 📁 Structure

```
infrastructure/nginx/
├── nginx.conf                  # Main configuration (development)
├── nginx.prod.conf            # Production configuration with SSL/TLS
├── conf.d/
│   ├── default.conf           # Default server (HTTP - development)
│   ├── api.conf               # API server with SSL/TLS (production)
│   └── frontend.conf          # Frontend server with SSL/TLS (production)
└── README.md                  # This file
```

## 🔧 Configuration Files

### 1. Main Configuration (`nginx.conf`)

**Features:**
- Worker processes: `auto` (matches CPU cores)
- Worker connections: `4096` per worker
- Rate limiting zones:
  - `api_limit`: 100 requests/minute
  - `upload_limit`: 10 requests/minute
- Gzip compression enabled
- Upstream servers with load balancing

**Upstream Servers:**
- `api_backend`: Backend API (least connections)
- `frontend_backend`: Angular frontend
- `ocr_backend`: OCR service

### 2. Production Configuration (`nginx.prod.conf`)

**Additional Features:**
- SSL/TLS configuration (TLSv1.2, TLSv1.3)
- Enhanced logging with upstream metrics
- Server tokens disabled
- SSL session cache

### 3. Default Configuration (`default.conf`)

**HTTP Server (Development):**
- Port: 80
- Health check endpoint: `/health`
- API proxy: `/api/`
- File upload: `/api/slips/verify`
- WebSocket support: `/socket.io/`
- OCR service: `/ocr/`
- Static files: `/uploads/`

**Features:**
- Rate limiting on API and upload endpoints
- CORS headers for development
- Security headers
- WebSocket upgrade support
- Long-lived connections for WebSocket (7 days)

### 4. API Configuration (`api.conf`)

**HTTPS Server (Production):**
- Port: 443
- Domain: `api.yourdomain.com`
- SSL/TLS termination
- HTTP to HTTPS redirect

**Features:**
- SSL certificate configuration
- Security headers (HSTS, CSP, etc.)
- Rate limiting (100 req/min for API, 10 req/min for uploads)
- WebSocket support for SignalR
- Health check endpoint

### 5. Frontend Configuration (`frontend.conf`)

**HTTPS Server (Production):**
- Port: 443
- Domain: `yourdomain.com`, `www.yourdomain.com`
- Angular SPA support

**Features:**
- Static asset caching (1 year)
- Angular routing support
- Gzip compression
- API proxy to backend

## 🚀 Usage

### Development

```bash
# Using Docker Compose
docker-compose -f docker-compose.dev.yml up -d

# The nginx service will use nginx.conf and conf.d/default.conf
```

### Production

```bash
# Using Docker Compose
docker-compose -f docker-compose.prod.yml up -d

# The nginx service will use nginx.prod.conf and all conf.d/*.conf files
```

### Testing Configuration

```bash
# Test nginx configuration
docker exec slip-nginx-dev nginx -t

# Reload nginx
docker exec slip-nginx-dev nginx -s reload
```

## 🔒 SSL/TLS Setup

### Generate Self-Signed Certificate (Development)

```bash
mkdir -p infrastructure/ssl
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout infrastructure/ssl/privkey.pem \
  -out infrastructure/ssl/fullchain.pem \
  -subj "/CN=localhost"
```

### Let's Encrypt (Production)

```bash
# Install certbot
apt-get install certbot python3-certbot-nginx

# Generate certificate
certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com -d api.yourdomain.com

# Copy certificates
cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem infrastructure/ssl/
cp /etc/letsencrypt/live/yourdomain.com/privkey.pem infrastructure/ssl/
```

## 📊 Rate Limiting

### API Endpoints
- **Rate**: 100 requests/minute per IP
- **Burst**: 20 requests
- **Applies to**: `/api/*`

### Upload Endpoints
- **Rate**: 10 requests/minute per IP
- **Burst**: 5 requests
- **Applies to**: `/api/slips/verify`

### Customization

Edit rate limiting zones in `nginx.conf`:

```nginx
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;
limit_req_zone $binary_remote_addr zone=upload_limit:10m rate=10r/m;
```

## ⚖️ Load Balancing

### Strategy: Least Connections

Configured in upstream blocks:

```nginx
upstream api_backend {
    least_conn;
    server backend:8080 weight=1 max_fails=3 fail_timeout=30s;
    keepalive 32;
}
```

### Multiple Backend Instances

To add more backend instances:

```nginx
upstream api_backend {
    least_conn;
    server api1:5000 weight=1 max_fails=3 fail_timeout=30s;
    server api2:5000 weight=1 max_fails=3 fail_timeout=30s;
    server api3:5000 weight=1 max_fails=3 fail_timeout=30s;
    keepalive 32;
}
```

Update `docker-compose.yml` to deploy multiple backend replicas.

## 🔌 WebSocket Support

### Socket.IO Configuration

WebSocket connections are supported via `/socket.io/` path:

```nginx
location /socket.io/ {
    proxy_pass http://api_backend/socket.io/;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_cache_bypass $http_upgrade;
    
    # Long-lived connection timeouts
    proxy_connect_timeout 7d;
    proxy_send_timeout 7d;
    proxy_read_timeout 7d;
}
```

### SignalR Configuration

For SignalR, use `/ws` path (configured in `api.conf`).

## 🗜️ Compression

### Gzip Compression

Enabled for:
- `text/plain`
- `text/css`
- `text/xml`
- `text/javascript`
- `application/json`
- `application/javascript`
- `application/xml+rss`

### Configuration

```nginx
gzip on;
gzip_vary on;
gzip_proxied any;
gzip_comp_level 6;
gzip_types text/plain text/css text/xml text/javascript 
           application/json application/javascript application/xml+rss;
```

## 🛡️ Security

### Security Headers

All servers include:
- `X-Frame-Options: SAMEORIGIN`
- `X-Content-Type-Options: nosniff`
- `X-XSS-Protection: 1; mode=block`
- `Referrer-Policy: no-referrer-when-downgrade` (HTTPS)
- `Content-Security-Policy` (HTTPS)
- `Strict-Transport-Security` (HTTPS only)

### CORS Configuration

Development server includes CORS headers:
- `Access-Control-Allow-Origin: *`
- `Access-Control-Allow-Methods: GET, POST, OPTIONS, PUT, DELETE`
- `Access-Control-Allow-Headers: ...`

For production, configure CORS in the backend API.

## 📈 Monitoring

### Access Logs

**Location:** `/var/log/nginx/access.log`

**Format:**
```
$remote_addr - $remote_user [$time_local] "$request" 
$status $body_bytes_sent "$http_referer" 
"$http_user_agent" "$http_x_forwarded_for"
```

**Production adds:**
```
rt=$request_time uct="$upstream_connect_time" 
uht="$upstream_header_time" urt="$upstream_response_time"
```

### Error Logs

**Location:** `/var/log/nginx/error.log`

**Level:** `warn`

### Health Check

**Endpoint:** `http://localhost:80/health`

**Response:** `200 OK` with body `"healthy\n"`

### Prometheus Integration

To add Prometheus metrics:

1. Install nginx-module-vts or use nginx-prometheus-exporter
2. Add metrics endpoint in configuration
3. Configure Prometheus to scrape nginx metrics

## 🔧 Troubleshooting

### Check Configuration

```bash
# Test configuration syntax
docker exec slip-nginx-dev nginx -t

# View current configuration
docker exec slip-nginx-dev cat /etc/nginx/nginx.conf
```

### View Logs

```bash
# Access logs
docker logs slip-nginx-dev

# Error logs
docker exec slip-nginx-dev tail -f /var/log/nginx/error.log

# Access logs
docker exec slip-nginx-dev tail -f /var/log/nginx/access.log
```

### Reload Configuration

```bash
# Graceful reload
docker exec slip-nginx-dev nginx -s reload

# Restart container
docker restart slip-nginx-dev
```

### Common Issues

#### 1. 502 Bad Gateway
- Backend service is not running
- Check `docker ps` to verify all services are up
- Check backend health: `docker exec slip-backend-dev curl http://localhost:8080/health`

#### 2. 504 Gateway Timeout
- Backend is slow to respond
- Increase timeout values in proxy configuration
- Check backend logs for performance issues

#### 3. WebSocket Connection Failed
- Verify `Upgrade` and `Connection` headers are set
- Check timeout values (should be long for WebSocket)
- Verify backend WebSocket endpoint is working

#### 4. Rate Limiting (429 Too Many Requests)
- Increase rate limit or burst values
- Implement proper retry logic in client
- Use authentication to increase limits per user

## 📝 Customization

### Change Domain Names

Edit `api.conf` and `frontend.conf`:

```nginx
server_name yourdomain.com www.yourdomain.com;
```

Replace with your actual domain.

### Adjust Rate Limits

Edit `nginx.conf`:

```nginx
# Increase API rate limit to 200/minute
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=200r/m;

# Increase upload rate limit to 20/minute
limit_req_zone $binary_remote_addr zone=upload_limit:10m rate=20r/m;
```

### Add More Upstream Servers

Edit the upstream blocks:

```nginx
upstream api_backend {
    least_conn;
    server api1:5000 weight=1 max_fails=3 fail_timeout=30s;
    server api2:5000 weight=1 max_fails=3 fail_timeout=30s;
    server api3:5000 weight=2 max_fails=3 fail_timeout=30s;  # Higher weight
    keepalive 32;
}
```

### Change Load Balancing Strategy

**Round Robin (default):**
```nginx
upstream api_backend {
    server api1:5000;
    server api2:5000;
}
```

**Least Connections (recommended for API):**
```nginx
upstream api_backend {
    least_conn;
    server api1:5000;
    server api2:5000;
}
```

**IP Hash (sticky sessions):**
```nginx
upstream api_backend {
    ip_hash;
    server api1:5000;
    server api2:5000;
}
```

## 🚦 Performance Tuning

### Worker Processes

```nginx
worker_processes auto;  # Use number of CPU cores
```

### Worker Connections

```nginx
events {
    worker_connections 4096;  # Increase for high traffic
}
```

### Keepalive Connections

```nginx
keepalive_timeout 65;

upstream api_backend {
    server backend:8080;
    keepalive 32;  # Keep 32 connections alive
}
```

### Buffer Sizes

```nginx
client_body_buffer_size 128k;
client_max_body_size 10M;
large_client_header_buffers 4 16k;
```

## 📚 References

- [Nginx Documentation](https://nginx.org/en/docs/)
- [Nginx WebSocket Proxying](https://nginx.org/en/docs/http/websocket.html)
- [Nginx Load Balancing](https://nginx.org/en/docs/http/load_balancing.html)
- [Nginx Rate Limiting](https://nginx.org/en/docs/http/ngx_http_limit_req_module.html)
- [Nginx SSL/TLS Configuration](https://nginx.org/en/docs/http/configuring_https_servers.html)
