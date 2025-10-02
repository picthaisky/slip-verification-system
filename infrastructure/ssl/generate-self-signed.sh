#!/bin/bash

# Generate Self-Signed SSL Certificate for Development
# This script creates a self-signed certificate for local development and testing.
# DO NOT use self-signed certificates in production!

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CERT_DIR="$SCRIPT_DIR"

echo "🔐 Generating self-signed SSL certificate for development..."
echo ""

# Certificate details
COUNTRY="TH"
STATE="Bangkok"
CITY="Bangkok"
ORGANIZATION="Development"
COMMON_NAME="localhost"
DAYS_VALID=365

# Prompt for custom domain (optional)
read -p "Enter domain name (default: localhost): " DOMAIN
DOMAIN=${DOMAIN:-localhost}

echo ""
echo "📝 Certificate details:"
echo "  Country: $COUNTRY"
echo "  State: $STATE"
echo "  City: $CITY"
echo "  Organization: $ORGANIZATION"
echo "  Domain: $DOMAIN"
echo "  Valid for: $DAYS_VALID days"
echo ""

# Generate private key and certificate
openssl req -x509 -nodes -days $DAYS_VALID -newkey rsa:2048 \
  -keyout "$CERT_DIR/privkey.pem" \
  -out "$CERT_DIR/fullchain.pem" \
  -subj "/C=$COUNTRY/ST=$STATE/L=$CITY/O=$ORGANIZATION/CN=$DOMAIN"

# Set proper permissions
chmod 600 "$CERT_DIR/privkey.pem"
chmod 644 "$CERT_DIR/fullchain.pem"

echo "✅ Certificate generated successfully!"
echo ""
echo "📁 Files created:"
echo "  - $CERT_DIR/privkey.pem (private key)"
echo "  - $CERT_DIR/fullchain.pem (certificate)"
echo ""
echo "⚠️  This is a self-signed certificate for development only."
echo "   Browsers will show a security warning. This is expected."
echo ""
echo "🚀 You can now start the services with:"
echo "   docker-compose -f docker-compose.prod.yml up -d"
echo ""
