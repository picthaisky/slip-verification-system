#!/bin/bash
#
# SSL Certificate Setup Script using Let's Encrypt
# Automatically obtains and configures SSL certificates
#

set -e

# Configuration
DOMAIN="${1:-slipverification.com}"
EMAIL="${2:-admin@slipverification.com}"
WEBROOT="${3:-/var/www/html}"
CERT_DIR="/etc/letsencrypt/live/${DOMAIN}"
NGINX_SSL_DIR="/etc/nginx/ssl"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"
}

warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    error "Please run as root or with sudo"
    exit 1
fi

log "═══════════════════════════════════════"
log "SSL Certificate Setup"
log "═══════════════════════════════════════"
log "Domain: $DOMAIN"
log "Email: $EMAIL"
log "═══════════════════════════════════════"

# Install Certbot if not installed
if ! command -v certbot &> /dev/null; then
    log "Installing Certbot..."
    
    if command -v apt-get &> /dev/null; then
        apt-get update
        apt-get install -y certbot python3-certbot-nginx
    elif command -v yum &> /dev/null; then
        yum install -y certbot python3-certbot-nginx
    else
        error "Package manager not supported. Please install certbot manually."
        exit 1
    fi
    
    log "✓ Certbot installed"
else
    log "✓ Certbot already installed"
fi

# Create webroot directory
mkdir -p "$WEBROOT"

# Obtain certificate
log "Obtaining SSL certificate from Let's Encrypt..."

if certbot certonly \
    --webroot \
    --webroot-path="$WEBROOT" \
    --email "$EMAIL" \
    --agree-tos \
    --no-eff-email \
    --preferred-challenges http \
    -d "$DOMAIN" \
    -d "www.$DOMAIN" \
    -d "api.$DOMAIN"; then
    log "✓ Certificate obtained successfully"
else
    error "Failed to obtain certificate"
    exit 1
fi

# Create Nginx SSL directory
mkdir -p "$NGINX_SSL_DIR"

# Copy certificates to Nginx directory
log "Configuring certificates for Nginx..."
cp -f "${CERT_DIR}/fullchain.pem" "${NGINX_SSL_DIR}/${DOMAIN}.crt"
cp -f "${CERT_DIR}/privkey.pem" "${NGINX_SSL_DIR}/${DOMAIN}.key"
chmod 600 "${NGINX_SSL_DIR}/${DOMAIN}.key"

log "✓ Certificates configured"

# Generate Diffie-Hellman parameters
if [ ! -f "${NGINX_SSL_DIR}/dhparam.pem" ]; then
    log "Generating Diffie-Hellman parameters (this may take a while)..."
    openssl dhparam -out "${NGINX_SSL_DIR}/dhparam.pem" 2048
    log "✓ DH parameters generated"
else
    log "✓ DH parameters already exist"
fi

# Create Nginx SSL configuration snippet
log "Creating Nginx SSL configuration..."
cat > /etc/nginx/snippets/ssl-params.conf <<'EOF'
# SSL Configuration
ssl_protocols TLSv1.2 TLSv1.3;
ssl_prefer_server_ciphers on;
ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384';

# SSL Session
ssl_session_cache shared:SSL:50m;
ssl_session_timeout 1d;
ssl_session_tickets off;

# OCSP Stapling
ssl_stapling on;
ssl_stapling_verify on;
resolver 8.8.8.8 8.8.4.4 valid=300s;
resolver_timeout 5s;

# Security Headers
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
add_header X-Frame-Options "SAMEORIGIN" always;
add_header X-Content-Type-Options "nosniff" always;
add_header X-XSS-Protection "1; mode=block" always;
EOF

log "✓ SSL configuration created"

# Setup automatic renewal
log "Setting up automatic certificate renewal..."

# Create renewal script
cat > /usr/local/bin/renew-ssl.sh <<'RENEWAL_SCRIPT'
#!/bin/bash
# SSL Certificate Renewal Script

certbot renew --quiet --post-hook "systemctl reload nginx"

# Copy renewed certificates
DOMAIN="slipverification.com"
CERT_DIR="/etc/letsencrypt/live/${DOMAIN}"
NGINX_SSL_DIR="/etc/nginx/ssl"

if [ -f "${CERT_DIR}/fullchain.pem" ]; then
    cp -f "${CERT_DIR}/fullchain.pem" "${NGINX_SSL_DIR}/${DOMAIN}.crt"
    cp -f "${CERT_DIR}/privkey.pem" "${NGINX_SSL_DIR}/${DOMAIN}.key"
    chmod 600 "${NGINX_SSL_DIR}/${DOMAIN}.key"
fi
RENEWAL_SCRIPT

chmod +x /usr/local/bin/renew-ssl.sh

# Add to crontab
(crontab -l 2>/dev/null; echo "0 3 * * 1 /usr/local/bin/renew-ssl.sh >> /var/log/ssl-renewal.log 2>&1") | crontab -

log "✓ Automatic renewal configured (runs weekly at 3 AM)"

# Test Nginx configuration
log "Testing Nginx configuration..."
if nginx -t; then
    log "✓ Nginx configuration is valid"
    
    # Reload Nginx
    log "Reloading Nginx..."
    systemctl reload nginx || service nginx reload
    log "✓ Nginx reloaded"
else
    error "Nginx configuration test failed"
    exit 1
fi

# Display certificate information
log "═══════════════════════════════════════"
log "Certificate Information:"
log "═══════════════════════════════════════"
certbot certificates -d "$DOMAIN"
log "═══════════════════════════════════════"

log "✓ SSL setup completed successfully!"
log ""
log "Certificate files:"
log "  - Certificate: ${NGINX_SSL_DIR}/${DOMAIN}.crt"
log "  - Private Key: ${NGINX_SSL_DIR}/${DOMAIN}.key"
log "  - DH Params: ${NGINX_SSL_DIR}/dhparam.pem"
log ""
log "To use in Nginx configuration:"
log "  ssl_certificate ${NGINX_SSL_DIR}/${DOMAIN}.crt;"
log "  ssl_certificate_key ${NGINX_SSL_DIR}/${DOMAIN}.key;"
log "  ssl_dhparam ${NGINX_SSL_DIR}/dhparam.pem;"
log "  include snippets/ssl-params.conf;"
log ""
log "Test your SSL configuration at: https://www.ssllabs.com/ssltest/"

exit 0
