# Nginx Quick Reference

Quick command reference for managing Nginx in the Slip Verification System.

## 🚀 Quick Start

### Development
```bash
docker-compose -f docker-compose.dev.yml up -d
curl http://localhost/health
```

### Production
```bash
# Generate SSL cert
cd infrastructure/ssl && ./generate-self-signed.sh && cd ../..

# Start services
docker-compose -f docker-compose.prod.yml up -d

# Test
curl https://yourdomain.com/health
```

## 🔧 Common Commands

### Testing Configuration
```bash
# Test syntax
docker exec slip-nginx-dev nginx -t

# Test and show config
docker exec slip-nginx-dev nginx -T
```

### Reload/Restart
```bash
# Reload configuration (no downtime)
docker exec slip-nginx-dev nginx -s reload

# Restart container
docker-compose restart nginx
```

### View Logs
```bash
# Container logs
docker logs slip-nginx-dev

# Access logs
docker exec slip-nginx-dev tail -f /var/log/nginx/access.log

# Error logs
docker exec slip-nginx-dev tail -f /var/log/nginx/error.log

# Last 100 lines
docker exec slip-nginx-dev tail -n 100 /var/log/nginx/access.log
```

### Check Status
```bash
# Container status
docker ps | grep nginx

# Health check
curl http://localhost/health

# Full status
docker-compose ps nginx
```

## 🔍 Debugging

### Check Configuration Files
```bash
# Main config
docker exec slip-nginx-dev cat /etc/nginx/nginx.conf

# Server configs
docker exec slip-nginx-dev ls /etc/nginx/conf.d/
docker exec slip-nginx-dev cat /etc/nginx/conf.d/default.conf
```

### Check Connections
```bash
# Active connections
docker exec slip-nginx-dev netstat -an | grep ESTABLISHED | wc -l

# All connections
docker exec slip-nginx-dev netstat -tuln
```

### Test Endpoints
```bash
# Health check
curl http://localhost/health

# API endpoint
curl http://localhost/api/health

# Frontend
curl -I http://localhost/

# With headers
curl -H "Authorization: Bearer TOKEN" http://localhost/api/users
```

## 🔒 SSL/TLS

### Generate Self-Signed Certificate
```bash
cd infrastructure/ssl
./generate-self-signed.sh
```

### Check Certificate
```bash
# View certificate
openssl x509 -in infrastructure/ssl/fullchain.pem -text -noout

# Check expiration
openssl x509 -in infrastructure/ssl/fullchain.pem -noout -dates

# Verify certificate
openssl verify infrastructure/ssl/fullchain.pem
```

### Test SSL
```bash
# Simple test
curl -vI https://yourdomain.com

# Detailed test
openssl s_client -connect yourdomain.com:443 -servername yourdomain.com
```

## 📊 Rate Limiting

### Test Rate Limits
```bash
# API (should fail after 100 requests)
for i in {1..120}; do 
  curl -s -o /dev/null -w "%{http_code}\n" http://localhost/api/health
done

# Upload (should fail after 10 requests)
for i in {1..15}; do
  curl -s -o /dev/null -w "%{http_code}\n" \
    -F "file=@test.jpg" http://localhost/api/slips/verify
done
```

### Check Rate Limit Status
```bash
# View rate limit config
docker exec slip-nginx-dev cat /etc/nginx/nginx.conf | grep limit_req
```

## 🌐 WebSocket

### Test WebSocket (Socket.IO)
```bash
# Install wscat
npm install -g wscat

# Test connection
wscat -c ws://localhost/socket.io/?EIO=4&transport=websocket
```

### Check WebSocket Logs
```bash
docker logs slip-nginx-dev | grep -i upgrade
```

## ⚖️ Load Balancing

### Check Upstream Status
```bash
# View upstream config
docker exec slip-nginx-dev cat /etc/nginx/nginx.conf | grep -A 10 "upstream"
```

### Test Load Distribution
```bash
# Multiple requests to see distribution
for i in {1..10}; do
  curl -s http://localhost/api/health | grep -o "backend-[0-9]"
done
```

## 📈 Performance Testing

### Apache Bench
```bash
# Install
sudo apt-get install apache2-utils

# 100 requests, 10 concurrent
ab -n 100 -c 10 http://localhost/

# With keep-alive
ab -n 1000 -c 50 -k http://localhost/api/health
```

### wrk (Advanced)
```bash
# Install
sudo apt-get install wrk

# 10 connections, 2 threads, 30 seconds
wrk -t2 -c10 -d30s http://localhost/

# With custom header
wrk -t2 -c10 -d30s -H "Authorization: Bearer TOKEN" http://localhost/api/health
```

## 🛠️ Configuration Changes

### Update Domain Names
```bash
# Edit API domain
nano infrastructure/nginx/conf.d/api.conf
# Change: server_name api.yourdomain.com;

# Edit frontend domain
nano infrastructure/nginx/conf.d/frontend.conf
# Change: server_name yourdomain.com www.yourdomain.com;

# Reload
docker exec slip-nginx-dev nginx -s reload
```

### Adjust Rate Limits
```bash
# Edit main config
nano infrastructure/nginx/nginx.conf
# Change: limit_req_zone ... rate=200r/m;

# Reload
docker exec slip-nginx-dev nginx -s reload
```

### Add Backend Instance
```bash
# Edit main config
nano infrastructure/nginx/nginx.conf
# Add: server api4:5000 weight=1 max_fails=3 fail_timeout=30s;

# Reload
docker exec slip-nginx-dev nginx -s reload
```

## 🔐 Security

### Check Security Headers
```bash
curl -I https://yourdomain.com | grep -E "(Strict-Transport|X-Frame|X-Content|X-XSS)"
```

### Test HTTPS Redirect
```bash
curl -I http://yourdomain.com
# Should see: 301 Moved Permanently
# Location: https://yourdomain.com
```

### SSL Labs Test
```bash
# Visit in browser:
https://www.ssllabs.com/ssltest/analyze.html?d=yourdomain.com
```

## 📦 Backup & Restore

### Backup Configuration
```bash
# Backup all configs
tar -czf nginx-backup-$(date +%Y%m%d).tar.gz infrastructure/nginx/

# Backup with SSL
tar -czf nginx-full-backup-$(date +%Y%m%d).tar.gz infrastructure/nginx/ infrastructure/ssl/
```

### Restore Configuration
```bash
# Extract backup
tar -xzf nginx-backup-20231002.tar.gz

# Reload
docker exec slip-nginx-dev nginx -s reload
```

## 🆘 Emergency Procedures

### Nginx Not Starting
```bash
# 1. Check logs
docker logs slip-nginx-dev

# 2. Test config
docker run --rm -v $(pwd)/infrastructure/nginx/nginx.conf:/etc/nginx/nginx.conf nginx:alpine nginx -t

# 3. Check port conflicts
sudo netstat -tlnp | grep :80
sudo netstat -tlnp | grep :443
```

### High CPU Usage
```bash
# 1. Check processes
docker exec slip-nginx-dev top

# 2. Check connections
docker exec slip-nginx-dev netstat -an | grep ESTABLISHED | wc -l

# 3. Temporarily reduce worker connections
# Edit nginx.conf: worker_connections 2048;
docker exec slip-nginx-dev nginx -s reload
```

### 502 Bad Gateway
```bash
# 1. Check backend
docker ps | grep backend

# 2. Test backend health
docker exec slip-backend-dev curl http://localhost:8080/health

# 3. Check nginx logs
docker logs slip-nginx-dev | tail -20

# 4. Check network
docker exec slip-nginx-dev ping backend
```

## 📱 Mobile Testing

### Test from Mobile Device
```bash
# Get server IP
hostname -I

# Access from mobile
http://YOUR_SERVER_IP

# For HTTPS (accept certificate warning)
https://YOUR_SERVER_IP
```

## 📚 Documentation Links

- **Full Guide**: `infrastructure/nginx/README.md`
- **Deployment**: `infrastructure/nginx/DEPLOYMENT.md`
- **Monitoring**: `infrastructure/nginx/MONITORING.md`
- **Architecture**: `infrastructure/nginx/ARCHITECTURE.md`
- **SSL Setup**: `infrastructure/ssl/README.md`

## 🎯 Common Tasks Checklist

### Daily Operations
- [ ] Check health endpoints
- [ ] Review error logs
- [ ] Monitor rate limit hits
- [ ] Check SSL certificate expiry

### Weekly Maintenance
- [ ] Review access logs
- [ ] Analyze performance metrics
- [ ] Check disk space for logs
- [ ] Test failover scenarios

### Monthly Tasks
- [ ] Update SSL certificates (if needed)
- [ ] Review and tune rate limits
- [ ] Performance testing
- [ ] Security audit
- [ ] Backup configurations

## 💡 Pro Tips

1. **Always test config before reload**: `nginx -t`
2. **Use reload, not restart**: `nginx -s reload` (no downtime)
3. **Monitor logs during changes**: `tail -f /var/log/nginx/error.log`
4. **Keep backups of working configs**
5. **Document custom changes**
6. **Test in development first**
7. **Use version control for configs**
8. **Monitor rate limit 429 responses**
9. **Check SSL expiry dates monthly**
10. **Load test before production deployment**

## 🔗 Quick Links

### Local Access
- Frontend: http://localhost
- API: http://localhost/api
- Health: http://localhost/health
- OCR: http://localhost/ocr

### Production Access (after setup)
- Frontend: https://yourdomain.com
- API: https://api.yourdomain.com
- Health: https://yourdomain.com/health

---

**Need help?** Check the full documentation in `infrastructure/nginx/README.md`
