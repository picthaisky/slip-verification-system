# Deployment Guide

## 📋 Prerequisites

Before deploying, ensure you have:
- Docker and Docker Compose installed
- PostgreSQL 16+ database server
- Redis server (optional but recommended)
- Valid SSL certificates for HTTPS (production)
- Domain name configured (production)

---

## 🐳 Docker Deployment (Recommended)

### Development Environment

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/slip-verification-system.git
cd slip-verification-system

# 2. Create environment file
cp .env.example .env
# Edit .env with your configuration

# 3. Start all services
docker-compose up -d

# 4. Check service health
docker-compose ps

# 5. View logs
docker-compose logs -f api
```

### Production Environment

```bash
# 1. Set environment to production
export ASPNETCORE_ENVIRONMENT=Production

# 2. Update production settings
# Edit .env with production values:
# - Strong JWT secret
# - Production database credentials
# - SSL certificates paths
# - External service URLs

# 3. Build production images
docker-compose -f docker-compose.prod.yml build

# 4. Start services
docker-compose -f docker-compose.prod.yml up -d

# 5. Apply database migrations
docker-compose exec api dotnet ef database update

# 6. Verify deployment
curl https://yourdomain.com/health
```

---

## 🖥️ Manual Deployment

### On Ubuntu/Debian Server

#### 1. Install Prerequisites

```bash
# Install .NET Runtime
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-9.0

# Install PostgreSQL
sudo apt-get install -y postgresql postgresql-contrib

# Install Redis
sudo apt-get install -y redis-server
```

#### 2. Setup Database

```bash
# Create database user
sudo -u postgres psql -c "CREATE USER slipverification WITH PASSWORD 'your_password';"
sudo -u postgres psql -c "CREATE DATABASE SlipVerificationDb OWNER slipverification;"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE SlipVerificationDb TO slipverification;"
```

#### 3. Deploy Application

```bash
# 1. Publish the application locally
dotnet publish src/SlipVerification.API/SlipVerification.API.csproj \
  -c Release \
  -o ./publish

# 2. Copy to server
scp -r ./publish user@server:/var/www/slipverification/

# 3. SSH to server
ssh user@server

# 4. Create service file
sudo nano /etc/systemd/system/slipverification.service
```

**Service File Content:**
```ini
[Unit]
Description=Slip Verification API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/slipverification
ExecStart=/usr/bin/dotnet /var/www/slipverification/SlipVerification.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=slipverification
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# 5. Enable and start service
sudo systemctl enable slipverification
sudo systemctl start slipverification

# 6. Check status
sudo systemctl status slipverification
```

#### 4. Setup Nginx Reverse Proxy

```bash
# Install Nginx
sudo apt-get install -y nginx

# Create Nginx configuration
sudo nano /etc/nginx/sites-available/slipverification
```

**Nginx Configuration:**
```nginx
upstream slipverification_api {
    server localhost:5000;
}

server {
    listen 80;
    server_name yourdomain.com;

    location / {
        return 301 https://$server_name$request_uri;
    }
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    ssl_certificate /etc/ssl/certs/your_cert.pem;
    ssl_certificate_key /etc/ssl/private/your_key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://slipverification_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 600;
        proxy_send_timeout 600;
        proxy_read_timeout 600;
        send_timeout 600;
    }

    # Static files (uploads)
    location /uploads {
        alias /var/www/slipverification/uploads;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

```bash
# Enable site and restart Nginx
sudo ln -s /etc/nginx/sites-available/slipverification /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

---

## ☁️ Cloud Deployment

### AWS Deployment

#### Using AWS Elastic Beanstalk

```bash
# 1. Install AWS CLI and EB CLI
pip install awscli awsebcli

# 2. Configure AWS credentials
aws configure

# 3. Initialize Elastic Beanstalk
eb init -p docker slipverification-api

# 4. Create environment
eb create production-env

# 5. Deploy
eb deploy

# 6. Open application
eb open
```

#### Using AWS ECS

```bash
# 1. Build and tag Docker image
docker build -t slipverification-api:latest .

# 2. Tag for ECR
docker tag slipverification-api:latest \
  YOUR_AWS_ACCOUNT.dkr.ecr.YOUR_REGION.amazonaws.com/slipverification-api:latest

# 3. Push to ECR
aws ecr get-login-password --region YOUR_REGION | \
  docker login --username AWS --password-stdin \
  YOUR_AWS_ACCOUNT.dkr.ecr.YOUR_REGION.amazonaws.com

docker push YOUR_AWS_ACCOUNT.dkr.ecr.YOUR_REGION.amazonaws.com/slipverification-api:latest

# 4. Update ECS service
aws ecs update-service --cluster your-cluster \
  --service slipverification-api \
  --force-new-deployment
```

### Azure Deployment

```bash
# 1. Create resource group
az group create --name SlipVerificationRG --location eastus

# 2. Create Azure Container Registry
az acr create --resource-group SlipVerificationRG \
  --name slipverificationacr --sku Basic

# 3. Build and push image
az acr build --registry slipverificationacr \
  --image slipverification-api:latest .

# 4. Create App Service Plan
az appservice plan create --name SlipVerificationPlan \
  --resource-group SlipVerificationRG \
  --is-linux --sku B1

# 5. Create Web App
az webapp create --resource-group SlipVerificationRG \
  --plan SlipVerificationPlan \
  --name slipverification-api \
  --deployment-container-image-name \
  slipverificationacr.azurecr.io/slipverification-api:latest

# 6. Configure environment variables
az webapp config appsettings set \
  --resource-group SlipVerificationRG \
  --name slipverification-api \
  --settings ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING"
```

### Google Cloud Platform

```bash
# 1. Build image
docker build -t gcr.io/YOUR_PROJECT_ID/slipverification-api:latest .

# 2. Push to Container Registry
docker push gcr.io/YOUR_PROJECT_ID/slipverification-api:latest

# 3. Deploy to Cloud Run
gcloud run deploy slipverification-api \
  --image gcr.io/YOUR_PROJECT_ID/slipverification-api:latest \
  --platform managed \
  --region us-central1 \
  --allow-unauthenticated
```

---

## 🔒 Security Checklist

### Pre-Deployment

- [ ] Change default JWT secret to a strong random value
- [ ] Use strong database passwords
- [ ] Enable SSL/TLS for all connections
- [ ] Configure CORS to allow only trusted domains
- [ ] Set up firewall rules
- [ ] Enable rate limiting
- [ ] Review and minimize exposed ports
- [ ] Implement proper logging (exclude sensitive data)
- [ ] Set up monitoring and alerting
- [ ] Configure backup strategy

### Environment Variables

Production `.env` should include:

```bash
# Strong secrets
JWT_SECRET=<generate-strong-random-key>
DB_PASSWORD=<strong-password>

# Production URLs
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80

# SSL Configuration
ASPNETCORE_Kestrel__Certificates__Default__Path=/path/to/cert.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=<cert-password>

# Trusted proxies
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
```

---

## 📊 Monitoring & Logging

### Application Insights (Azure)

```csharp
// Add to Program.cs
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["ApplicationInsights:InstrumentationKey"]);
```

### Sentry Error Tracking

```bash
# Set DSN in .env
SENTRY_DSN=https://your-sentry-dsn
```

### Prometheus Metrics

```bash
# Expose metrics endpoint
dotnet add package prometheus-net.AspNetCore
```

---

## 🔄 CI/CD Pipeline

### GitHub Actions Example

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore -c Release
      
      - name: Test
        run: dotnet test --no-build -c Release
      
      - name: Publish
        run: dotnet publish -c Release -o ./publish
      
      - name: Deploy to server
        uses: appleboy/scp-action@master
        with:
          host: ${{ secrets.SERVER_HOST }}
          username: ${{ secrets.SERVER_USER }}
          key: ${{ secrets.SERVER_SSH_KEY }}
          source: "./publish/*"
          target: "/var/www/slipverification"
```

---

## 🗄️ Database Migration

### Production Database Update

```bash
# 1. Backup existing database
pg_dump -h localhost -U slipverification SlipVerificationDb > backup.sql

# 2. Run migrations
dotnet ef database update --project src/SlipVerification.Infrastructure

# 3. Verify
psql -h localhost -U slipverification -d SlipVerificationDb -c "\dt"
```

---

## 🔧 Post-Deployment Verification

```bash
# 1. Check service status
systemctl status slipverification

# 2. Test health endpoint
curl https://yourdomain.com/health

# 3. Check logs
journalctl -u slipverification -f

# 4. Test API endpoints
curl -X GET https://yourdomain.com/api/v1/orders \
  -H "Authorization: Bearer YOUR_TOKEN"

# 5. Monitor resource usage
top
htop
```

---

## 🆘 Rollback Procedure

```bash
# 1. Stop current service
sudo systemctl stop slipverification

# 2. Restore previous version
sudo cp -r /var/www/slipverification.backup/* /var/www/slipverification/

# 3. Restore database if needed
psql -h localhost -U slipverification SlipVerificationDb < backup.sql

# 4. Start service
sudo systemctl start slipverification

# 5. Verify
sudo systemctl status slipverification
```

---

## 📞 Support

For deployment support:
- Email: devops@slipverification.com
- Slack: #deployment-support
- On-call: +66-xxx-xxx-xxxx
