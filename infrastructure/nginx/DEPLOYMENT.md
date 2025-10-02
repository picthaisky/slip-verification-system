# Nginx Deployment Guide

Complete guide for deploying Nginx with the Slip Verification System.

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Development Setup](#development-setup)
3. [Production Deployment](#production-deployment)
4. [SSL/TLS Configuration](#ssltls-configuration)
5. [Load Balancing Setup](#load-balancing-setup)
6. [Testing & Verification](#testing--verification)
7. [Troubleshooting](#troubleshooting)

## 🔧 Prerequisites

### System Requirements

- Docker Engine 20.10+
- Docker Compose 2.0+
- Minimum 2GB RAM
- 10GB disk space

### Domain Setup (Production)

- Domain name registered
- DNS records configured:
  - `A` record: `yourdomain.com` → Your server IP
  - `A` record: `www.yourdomain.com` → Your server IP
  - `A` record: `api.yourdomain.com` → Your server IP

## 🚀 Development Setup

### Step 1: Clone Repository

```bash
git clone https://github.com/picthaisky/slip-verification-system.git
cd slip-verification-system
```

### Step 2: Start Services

```bash
# Start all services including Nginx
docker-compose -f docker-compose.dev.yml up -d

# Check services are running
docker-compose -f docker-compose.dev.yml ps
```

### Step 3: Verify Nginx

```bash
# Test nginx configuration
docker exec slip-nginx-dev nginx -t

# Check nginx logs
docker logs slip-nginx-dev

# Test health endpoint
curl http://localhost/health
# Expected: "healthy"
```

### Step 4: Access Services

- **Frontend**: http://localhost:80
- **API**: http://localhost/api/
- **Health Check**: http://localhost/health
- **OCR Service**: http://localhost/ocr/

### Development with HTTPS

Generate self-signed certificate:

```bash
cd infrastructure/ssl
./generate-self-signed.sh
cd ../..
```

Restart nginx:

```bash
docker-compose -f docker-compose.dev.yml restart nginx
```

Access via: https://localhost (accept security warning)

## 🏭 Production Deployment

### Step 1: Prepare Server

```bash
# Update system
sudo apt-get update
sudo apt-get upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### Step 2: Configure Environment

```bash
# Clone repository
git clone https://github.com/picthaisky/slip-verification-system.git
cd slip-verification-system

# Copy environment file
cp .env.production.example .env.production

# Edit environment variables
nano .env.production
```

Required variables:
- `DB_PASSWORD`: PostgreSQL password
- `REDIS_PASSWORD`: Redis password
- `JWT_SECRET`: JWT signing key (32+ characters)
- `DB_CONNECTION_STRING`: Full database connection string

### Step 3: Update Nginx Configuration

Edit domain names in configuration files:

```bash
# Edit api.conf
nano infrastructure/nginx/conf.d/api.conf
# Change: server_name api.yourdomain.com;

# Edit frontend.conf
nano infrastructure/nginx/conf.d/frontend.conf
# Change: server_name yourdomain.com www.yourdomain.com;
```

### Step 4: Obtain SSL Certificates

See [SSL/TLS Configuration](#ssltls-configuration) below.

### Step 5: Deploy Services

```bash
# Pull latest images
docker-compose -f docker-compose.prod.yml pull

# Start services
docker-compose -f docker-compose.prod.yml up -d

# Check status
docker-compose -f docker-compose.prod.yml ps
```

### Step 6: Verify Deployment

```bash
# Check nginx configuration
docker exec slip-nginx-prod nginx -t

# Check logs
docker logs slip-nginx-prod

# Test health endpoint
curl https://yourdomain.com/health
```

## 🔒 SSL/TLS Configuration

### Option 1: Let's Encrypt (Recommended)

```bash
# Install certbot
sudo apt-get install certbot

# Stop nginx temporarily
docker-compose -f docker-compose.prod.yml stop nginx

# Generate certificate
sudo certbot certonly --standalone \
  -d yourdomain.com \
  -d www.yourdomain.com \
  -d api.yourdomain.com \
  --email your-email@example.com \
  --agree-tos \
  --non-interactive

# Copy certificates
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem infrastructure/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem infrastructure/ssl/
sudo chmod 644 infrastructure/ssl/fullchain.pem
sudo chmod 600 infrastructure/ssl/privkey.pem

# Start nginx
docker-compose -f docker-compose.prod.yml start nginx
```

### Option 2: Existing Certificate

```bash
# Copy your existing certificates
cp /path/to/fullchain.pem infrastructure/ssl/
cp /path/to/privkey.pem infrastructure/ssl/

# Set permissions
chmod 644 infrastructure/ssl/fullchain.pem
chmod 600 infrastructure/ssl/privkey.pem
```

### Auto-Renewal Setup

```bash
# Create renewal script
sudo nano /usr/local/bin/renew-certs.sh
```

Add:
```bash
#!/bin/bash
certbot renew --quiet
cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem /path/to/infrastructure/ssl/
cp /etc/letsencrypt/live/yourdomain.com/privkey.pem /path/to/infrastructure/ssl/
docker exec slip-nginx-prod nginx -s reload
```

```bash
# Make executable
sudo chmod +x /usr/local/bin/renew-certs.sh

# Add to crontab
sudo crontab -e
# Add: 0 3 * * * /usr/local/bin/renew-certs.sh
```

## ⚖️ Load Balancing Setup

### Deploy Multiple Backend Instances

Update `docker-compose.prod.yml`:

```yaml
services:
  backend-1:
    image: ${REGISTRY}/slip-backend:${TAG}
    container_name: slip-backend-1
    # ... same config as backend
    
  backend-2:
    image: ${REGISTRY}/slip-backend:${TAG}
    container_name: slip-backend-2
    # ... same config as backend
    
  backend-3:
    image: ${REGISTRY}/slip-backend:${TAG}
    container_name: slip-backend-3
    # ... same config as backend
```

### Update Nginx Upstream

Edit `infrastructure/nginx/nginx.prod.conf`:

```nginx
upstream api_backend {
    least_conn;
    server backend-1:8080 weight=1 max_fails=3 fail_timeout=30s;
    server backend-2:8080 weight=1 max_fails=3 fail_timeout=30s;
    server backend-3:8080 weight=1 max_fails=3 fail_timeout=30s;
    keepalive 32;
}
```

### Deploy

```bash
docker-compose -f docker-compose.prod.yml up -d --scale backend=3
docker exec slip-nginx-prod nginx -s reload
```

## ✅ Testing & Verification

### Configuration Test

```bash
# Test nginx configuration syntax
docker exec slip-nginx-prod nginx -t

# Expected output:
# nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
# nginx: configuration file /etc/nginx/nginx.conf test is successful
```

### SSL Test

```bash
# Test SSL certificate
openssl s_client -connect yourdomain.com:443 -servername yourdomain.com

# Test SSL configuration
curl -vI https://yourdomain.com

# Use SSL Labs
# Visit: https://www.ssllabs.com/ssltest/analyze.html?d=yourdomain.com
```

### Load Test

```bash
# Install Apache Bench
sudo apt-get install apache2-utils

# Simple load test (100 requests, 10 concurrent)
ab -n 100 -c 10 https://yourdomain.com/health

# More intensive test (1000 requests, 50 concurrent)
ab -n 1000 -c 50 https://yourdomain.com/api/health
```

### Rate Limiting Test

```bash
# Test API rate limit (100/minute)
for i in {1..120}; do
  curl -s -o /dev/null -w "%{http_code}\n" http://localhost/api/health
done

# Should see some 429 responses after 100 requests
```

### WebSocket Test

```bash
# Install wscat
npm install -g wscat

# Test WebSocket connection
wscat -c ws://localhost/socket.io/?EIO=4&transport=websocket
```

### Health Checks

```bash
# Nginx health
curl http://localhost/health

# Backend health
curl http://localhost/api/health

# Full system health
docker-compose -f docker-compose.prod.yml ps
```

## 🔧 Troubleshooting

### Common Issues

#### 1. Nginx Won't Start

```bash
# Check configuration
docker exec slip-nginx-prod nginx -t

# Check logs
docker logs slip-nginx-prod

# Check port conflicts
sudo netstat -tlnp | grep :80
sudo netstat -tlnp | grep :443
```

#### 2. SSL Certificate Error

```bash
# Verify certificate files exist
ls -la infrastructure/ssl/

# Check certificate validity
openssl x509 -in infrastructure/ssl/fullchain.pem -noout -dates

# Check certificate matches key
openssl x509 -noout -modulus -in infrastructure/ssl/fullchain.pem | openssl md5
openssl rsa -noout -modulus -in infrastructure/ssl/privkey.pem | openssl md5
# Checksums should match
```

#### 3. Backend Connection Failed (502 Bad Gateway)

```bash
# Check backend is running
docker ps | grep backend

# Check backend health
docker exec slip-backend-prod curl http://localhost:8080/health

# Check network connectivity
docker exec slip-nginx-prod ping backend

# Check logs
docker logs slip-backend-prod
```

#### 4. Rate Limiting Too Strict

Edit `infrastructure/nginx/nginx.conf`:

```nginx
# Increase rate limits
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=200r/m;
limit_req_zone $binary_remote_addr zone=upload_limit:10m rate=20r/m;
```

Reload:
```bash
docker exec slip-nginx-prod nginx -s reload
```

#### 5. WebSocket Connection Fails

Check configuration:

```bash
docker exec slip-nginx-prod cat /etc/nginx/conf.d/default.conf | grep -A 10 "socket.io"
```

Verify headers:

```bash
curl -i -N \
  -H "Connection: Upgrade" \
  -H "Upgrade: websocket" \
  -H "Sec-WebSocket-Version: 13" \
  -H "Sec-WebSocket-Key: test" \
  http://localhost/socket.io/
```

### Performance Issues

#### High CPU Usage

```bash
# Check worker processes
docker exec slip-nginx-prod ps aux | grep nginx

# Check connections
docker exec slip-nginx-prod netstat -an | grep ESTABLISHED | wc -l

# Consider increasing worker_connections
nano infrastructure/nginx/nginx.conf
# worker_connections 8192;
```

#### Slow Response Times

```bash
# Check upstream response times in logs
docker logs slip-nginx-prod | grep "upstream_response_time"

# Increase timeouts if needed
nano infrastructure/nginx/conf.d/default.conf
# proxy_read_timeout 120s;
```

### Maintenance Tasks

#### Reload Configuration

```bash
# After changing configuration files
docker exec slip-nginx-prod nginx -s reload
```

#### View Logs

```bash
# All logs
docker logs slip-nginx-prod

# Follow logs
docker logs -f slip-nginx-prod

# Access logs
docker exec slip-nginx-prod tail -f /var/log/nginx/access.log

# Error logs
docker exec slip-nginx-prod tail -f /var/log/nginx/error.log
```

#### Restart Nginx

```bash
# Graceful restart
docker-compose -f docker-compose.prod.yml restart nginx

# Force restart
docker-compose -f docker-compose.prod.yml stop nginx
docker-compose -f docker-compose.prod.yml start nginx
```

## 📚 Additional Resources

- [Nginx Documentation](https://nginx.org/en/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [SSL Labs Testing](https://www.ssllabs.com/ssltest/)
- [Security Headers](https://securityheaders.com/)

## 🆘 Getting Help

1. Check logs: `docker logs slip-nginx-prod`
2. Test configuration: `docker exec slip-nginx-prod nginx -t`
3. Review documentation in `infrastructure/nginx/README.md`
4. Check monitoring dashboard
5. Open an issue on GitHub

## 📝 Checklist

### Pre-Deployment
- [ ] Domain DNS configured
- [ ] SSL certificates obtained
- [ ] Environment variables set
- [ ] Configuration files updated with domain names
- [ ] Nginx configuration tested

### Post-Deployment
- [ ] All services started successfully
- [ ] Health checks passing
- [ ] SSL certificate valid
- [ ] Rate limiting working
- [ ] WebSocket connections working
- [ ] Monitoring configured
- [ ] Auto-renewal setup for certificates
- [ ] Backup strategy in place
- [ ] Load testing completed
- [ ] Documentation updated
