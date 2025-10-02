# SSL Certificate Directory

This directory contains SSL/TLS certificates for HTTPS configuration.

## 📁 Required Files

For production deployment, you need:
- `fullchain.pem` - Full certificate chain
- `privkey.pem` - Private key

## 🔒 Security

**⚠️ IMPORTANT:** Never commit real SSL certificates to version control!

Add to `.gitignore`:
```
infrastructure/ssl/*.pem
infrastructure/ssl/*.key
infrastructure/ssl/*.crt
```

## 🛠️ Generate Self-Signed Certificate (Development)

For development and testing purposes only:

```bash
# Run from repository root
./infrastructure/ssl/generate-self-signed.sh
```

Or manually:

```bash
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout infrastructure/ssl/privkey.pem \
  -out infrastructure/ssl/fullchain.pem \
  -subj "/C=TH/ST=Bangkok/L=Bangkok/O=Development/CN=localhost"
```

## 🌐 Let's Encrypt (Production)

### Option 1: Certbot Standalone

```bash
# Install certbot
sudo apt-get update
sudo apt-get install certbot

# Stop nginx temporarily
docker-compose -f docker-compose.prod.yml stop nginx

# Generate certificate
sudo certbot certonly --standalone \
  -d yourdomain.com \
  -d www.yourdomain.com \
  -d api.yourdomain.com \
  --email your-email@example.com \
  --agree-tos

# Copy certificates
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem infrastructure/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem infrastructure/ssl/
sudo chmod 644 infrastructure/ssl/*.pem

# Restart nginx
docker-compose -f docker-compose.prod.yml start nginx
```

### Option 2: Certbot with Nginx

```bash
# Install certbot nginx plugin
sudo apt-get install python3-certbot-nginx

# Generate and install certificate
sudo certbot --nginx \
  -d yourdomain.com \
  -d www.yourdomain.com \
  -d api.yourdomain.com \
  --email your-email@example.com \
  --agree-tos

# Copy to project directory
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem infrastructure/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem infrastructure/ssl/
```

### Option 3: DNS Challenge (Wildcard)

```bash
# For wildcard certificate (*.yourdomain.com)
sudo certbot certonly --manual --preferred-challenges dns \
  -d yourdomain.com \
  -d *.yourdomain.com \
  --email your-email@example.com \
  --agree-tos

# Follow DNS challenge instructions
# Then copy certificates as shown above
```

## 🔄 Auto-Renewal

### Setup Certbot Auto-Renewal

```bash
# Test renewal
sudo certbot renew --dry-run

# Setup cron job for auto-renewal
sudo crontab -e

# Add this line (runs twice daily)
0 0,12 * * * certbot renew --quiet --post-hook "docker-compose -f /path/to/docker-compose.prod.yml restart nginx"
```

### Docker Compose with Auto-Renewal

Create a certbot service in `docker-compose.prod.yml`:

```yaml
certbot:
  image: certbot/certbot
  volumes:
    - ./infrastructure/ssl:/etc/letsencrypt
  command: renew --webroot --webroot-path=/var/www/certbot
```

## 🛡️ Certificate Verification

```bash
# Check certificate expiration
openssl x509 -in infrastructure/ssl/fullchain.pem -noout -dates

# Check certificate details
openssl x509 -in infrastructure/ssl/fullchain.pem -noout -text

# Verify certificate chain
openssl verify -CAfile infrastructure/ssl/fullchain.pem infrastructure/ssl/fullchain.pem

# Test SSL configuration
curl -vI https://yourdomain.com
```

## 📋 File Permissions

Ensure proper permissions:

```bash
chmod 600 infrastructure/ssl/privkey.pem
chmod 644 infrastructure/ssl/fullchain.pem
```

## 🚀 Quick Start

### Development (Self-Signed)

```bash
# Generate self-signed certificate
cd infrastructure/ssl
./generate-self-signed.sh

# Start services
cd ../..
docker-compose -f docker-compose.dev.yml up -d
```

### Production (Let's Encrypt)

```bash
# Generate Let's Encrypt certificate (follow instructions above)
# Start services
docker-compose -f docker-compose.prod.yml up -d

# Verify HTTPS is working
curl -I https://yourdomain.com
```

## 📚 Additional Resources

- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [Certbot Documentation](https://certbot.eff.org/docs/)
- [SSL Labs Server Test](https://www.ssllabs.com/ssltest/)
- [Mozilla SSL Configuration Generator](https://ssl-config.mozilla.org/)
