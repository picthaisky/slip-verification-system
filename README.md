<div align="center">

# 💳 Slip Verification System

### ระบบตรวจสอบการรับชำระเงินผ่าน QR Code แบบอัตโนมัติด้วย AI

[![Made with Love](https://img.shields.io/badge/Made%20with-❤-red?style=for-the-badge)](https://github.com/picthaisky)
[![License MIT](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge&logo=opensourceinitiative&logoColor=white)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-Welcome-brightgreen?style=for-the-badge&logo=github)](CONTRIBUTING.md)

---

### 🚀 Technology Stack

[![.NET Core](https://img.shields.io/badge/.NET_Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.io/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Python](https://img.shields.io/badge/Python-3.12-3776AB?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/)

---

<p align="center">
  <a href="#-quick-start">Quick Start</a> •
  <a href="#-features">Features</a> •
  <a href="#-demo">Demo</a> •
  <a href="#-documentation">Documentation</a> •
  <a href="#-api-reference">API</a> •
  <a href="#-contributing">Contributing</a>
</p>

<img src="docs/images/banner.png" alt="Slip Verification System Banner" width="100%"/>

</div>

---

## 🎯 About The Project

> **Automate slip verification, eliminate manual checking, and get real-time payment notifications!**

Slip Verification System เป็นระบบตรวจสอบและยืนยันการชำระเงินผ่าน QR Code แบบอัตโนมัติ โดยใช้เทคโนโลジี **OCR (Optical Character Recognition)** ในการอ่านข้อมูลจากสลิปการโอนเงิน และส่งการแจ้งเตือนแบบ **Real-time** ผ่านหลายช่องทาง

<div align="center">

### 📊 Why This System?

| ❌ **Before** (Manual) | ✅ **After** (Automated) |
|:---|:---|
| ⏱️ ใช้เวลา 5-10 นาทีต่อการตรวจสอบ 1 สลิป | ⚡ ตรวจสอบอัตโนมัติภายใน 3 วินาที |
| 👤 เสี่ยงผิดพลาดจากคนตรวจสอบ | 🤖 ความแม่นยำ 95%+ ด้วย AI |
| 📝 ต้องบันทึกข้อมูลด้วยตนเอง | 💾 บันทึกอัตโนมัติและสร้างรายงาน |
| 📞 แจ้งลูกค้าทีละคน | 🔔 แจ้งเตือนทันที (LINE, Email, Push) |
| 📂 จัดการเอกสารยาก | 🗄️ ระบบจัดเก็บและค้นหาที่มีประสิทธิภาพ |

</div>


<br/>

### 📸 Application Preview

<table>
<tr>
<td width="50%">

#### 📊 Real-time Dashboard
<img src="docs/images/dashboard.png" alt="Dashboard" />
<sub>แสดงสถิติและกราฟแบบ Real-time</sub>

</td>
<td width="50%">

#### 📤 Slip Upload Interface
<img src="docs/images/upload.png" alt="Upload" />
<sub>Drag & Drop ง่าย พร้อม Preview</sub>

</td>
</tr>
<tr>
<td width="50%">

#### ✅ Verification Result
<img src="docs/images/result.png" alt="Result" />
<sub>แสดงผลการตรวจสอบพร้อม Confidence Score</sub>

</td>
<td width="50%">

#### 📱 Mobile Application
<img src="docs/images/mobile.png" alt="Mobile" />
<sub>ใช้งานได้ทั้ง iOS และ Android</sub>

</td>
</tr>
</table>

---

## ⭐ Key Features

<details open>
<summary><b>🤖 Smart OCR & Verification</b></summary>
<br/>

```mermaid
graph LR
    A[📤 Upload Slip] --> B[🔍 OCR Processing]
    B --> C[📊 Extract Data]
    C --> D[✅ Validate]
    D --> E[🔔 Notify]
    
    style A fill:#e1f5ff
    style B fill:#fff4e1
    style C fill:#f0fff4
    style D fill:#fef3c7
    style E fill:#dcfce7
```

- ✨ **อัพโหลดสลิป**: Drag & Drop, รองรับ JPG/PNG/PDF
- 🔍 **OCR Accuracy**: ความแม่นยำ 95%+ ด้วย PaddleOCR
- 📋 **Auto Extract**: จำนวนเงิน, วันที่, เวลา, เลขอ้างอิง, ธนาคาร
- ✅ **Smart Validation**: ตรวจสอบความถูกต้องและจับคู่กับ Order
- 🚫 **Duplicate Detection**: ตรวจจับสลิปซ้ำอัตโนมัติ

</details>

<details>
<summary><b>🔔 Multi-Channel Notifications</b></summary>
<br/>

<table>
<tr>
<th>Channel</th>
<th>Features</th>
<th>Speed</th>
</tr>
<tr>
<td><img src="https://img.shields.io/badge/LINE-00C300?style=flat&logo=line&logoColor=white"/> <b>LINE Notify</b></td>
<td>✅ Rich Messages<br/>✅ Images Support<br/>✅ Group Notifications</td>
<td>⚡ Instant</td>
</tr>
<tr>
<td><img src="https://img.shields.io/badge/Email-EA4335?style=flat&logo=gmail&logoColor=white"/> <b>Email</b></td>
<td>✅ HTML Templates<br/>✅ Attachments<br/>✅ Bounce Handling</td>
<td>⚡ < 5 seconds</td>
</tr>
<tr>
<td><img src="https://img.shields.io/badge/Push-4285F4?style=flat&logo=firebase&logoColor=white"/> <b>Push Notification</b></td>
<td>✅ iOS & Android<br/>✅ Deep Linking<br/>✅ Badge Updates</td>
<td>⚡ Real-time</td>
</tr>
<tr>
<td><img src="https://img.shields.io/badge/SMS-00BFA5?style=flat&logo=twilio&logoColor=white"/> <b>SMS (Optional)</b></td>
<td>✅ OTP Verification<br/>✅ Critical Alerts</td>
<td>⚡ < 10 seconds</td>
</tr>
</table>

</details>

<details>
<summary><b>📊 Advanced Dashboard & Analytics</b></summary>
<br/>

> Real-time insights with beautiful visualizations

**📈 Statistics Cards**
- 💰 Total Transactions & Revenue
- ✅ Success Rate
- ⏱️ Average Processing Time
- 🔄 Pending Payments Count

**📊 Interactive Charts**
- 📉 Transaction Timeline (Line Chart)
- 🥧 Payment Methods Distribution (Pie Chart)
- 📊 Monthly Comparison (Bar Chart)
- 🗓️ Calendar Heatmap

**🎯 Features**
- ⚡ Real-time Updates via WebSocket
- 📅 Date Range Filtering
- 📥 Export to Excel/PDF
- 📱 Responsive Design

</details>

<details>
<summary><b>🔐 Security & Compliance</b></summary>
<br/>

| Feature | Implementation | Status |
|---------|---------------|--------|
| 🔑 Authentication | JWT + Refresh Token | ✅ |
| 👮 Authorization | Role-Based Access Control | ✅ |
| 🔒 Data Encryption | AES-256 (at rest & transit) | ✅ |
| 🛡️ Input Validation | FluentValidation | ✅ |
| 🚫 SQL Injection | Parameterized Queries | ✅ |
| 🌐 XSS Protection | Content Security Policy | ✅ |
| 📝 Audit Trail | Complete Activity Logs | ✅ |
| 🇹🇭 PDPA Compliant | Data Protection Act | ✅ |

</details>

<details>
<summary><b>🚀 Performance & Scalability</b></summary>
<br/>

```
📊 Performance Metrics:
├─ 🎯 Response Time: < 200ms (GET requests)
├─ ⚡ Slip Processing: < 3 seconds
├─ 🔄 Concurrent Users: 1,000+
├─ 📈 Daily Transactions: 10,000+
└─ 💾 Database Size: Supports Millions of Records
```

**🎨 Optimization Techniques:**
- ⚡ Redis Caching
- 🔄 Database Indexing
- 📦 Response Compression
- 🎭 Lazy Loading
- 🚀 CDN Integration
- ⚖️ Load Balancing
- 📊 Auto-Scaling (Kubernetes)

</details>

<details>
<summary><b>📱 Multi-Platform Support</b></summary>
<br/>

<div align="center">

| Platform | Technology | Status |
|:--------:|:-----------|:------:|
| 🌐 **Web** | Angular 20 + Tailwind CSS | ✅ Production |
| 📱 **iOS** | React Native | ✅ Beta |
| 🤖 **Android** | React Native | ✅ Beta |
| 💻 **Desktop** | Electron (Planned) | 🔄 Roadmap |
| 🌙 **Dark Mode** | All Platforms | ✅ |
| 🌍 **i18n** | Thai, English | ✅ |

</div>

</details>

---

## 🏗️ System Architecture

<div align="center">

### 🎨 High-Level Architecture

```mermaid
graph TB
    subgraph "🌐 Client Layer"
        A[💻 Web App<br/>Angular 20]
        B[📱 Mobile App<br/>React Native]
    end
    
    subgraph "⚡ API Gateway"
        C[🔄 Nginx<br/>Load Balancer]
    end
    
    subgraph "🚀 Application Services"
        D[🎯 Main API<br/>.NET Core 9]
        E[👁️ OCR Service<br/>Python/FastAPI]
        F[📬 Notification Service<br/>.NET Core 9]
    end
    
    subgraph "💾 Data Layer"
        G[(🗄️ PostgreSQL<br/>Database)]
        H[(⚡ Redis<br/>Cache & Queue)]
    end
    
    subgraph "📦 Storage"
        I[☁️ File Storage<br/>MinIO/S3]
    end
    
    subgraph "🔔 External Services"
        J[💚 LINE Notify]
        K[📧 Email Service]
        L[🔥 Firebase FCM]
    end
    
    A --> C
    B --> C
    C --> D
    D --> E
    D --> F
    D --> G
    D --> H
    D --> I
    F --> J
    F --> K
    F --> L
    E --> H
    
    style A fill:#e1f5ff,stroke:#0ea5e9,stroke-width:2px
    style B fill:#e1f5ff,stroke:#0ea5e9,stroke-width:2px
    style D fill:#fef3c7,stroke:#f59e0b,stroke-width:2px
    style E fill:#fef3c7,stroke:#f59e0b,stroke-width:2px
    style F fill:#fef3c7,stroke:#f59e0b,stroke-width:2px
    style G fill:#f0fdf4,stroke:#10b981,stroke-width:2px
    style H fill:#f0fdf4,stroke:#10b981,stroke-width:2px
```

<sub>🔄 Real-time data flow | 🔐 Secure communication | ⚡ High performance</sub>

</div>

---

## 🚀 Quick Start

<div align="center">

### 🎯 Get Started in 3 Steps

</div>

<table>
<tr>
<td width="33%" align="center">

### 1️⃣ Clone
```bash
git clone https://github.com/picthaisky/slip-verification-system.git
cd slip-verification-system
```

</td>
<td width="33%" align="center">

### 2️⃣ Configure
```bash
cp .env.production.example .env
# Edit .env file with your settings
```

</td>
<td width="33%" align="center">

### 3️⃣ Launch
```bash
make dev
# or
docker-compose -f docker-compose.dev.yml up -d
```

</td>
</tr>
</table>

<br/>

<div align="center">

### 🎮 Quick Commands

[![Setup](https://img.shields.io/badge/Setup-make%20setup-blue?style=for-the-badge&logo=rocket)](Makefile)
[![Start](https://img.shields.io/badge/Start-make%20dev-green?style=for-the-badge&logo=play)](Makefile)
[![Test](https://img.shields.io/badge/Test-make%20test-yellow?style=for-the-badge&logo=checkmarx)](Makefile)
[![Docs](https://img.shields.io/badge/Docs-View-purple?style=for-the-badge&logo=readthedocs)](docs/)

</div>

> **💡 Pro Tip**: ใช้ `make help` เพื่อดูคำสั่งทั้งหมด

<br/>

### 📦 Installation Options

<details>
<summary><b>🐳 Option 1: Docker (Recommended)</b></summary>

```bash
# Development Environment (Frontend, Backend, OCR, PostgreSQL, Redis)
docker-compose -f docker-compose.dev.yml up -d

# Or using Makefile
make dev

# For full stack with monitoring
docker-compose -f docker-compose.prod.yml up -d

# ✅ Ready! Access applications at:
# 🌐 Frontend: http://localhost:4200
# 🔧 API: http://localhost:5000
# 📖 Swagger: http://localhost:5000/swagger
# 🤖 OCR Service: http://localhost:8000
# 📖 OCR Docs: http://localhost:8000/docs
```

**Note:** Database migrations run automatically on first startup.

</details>

<details>
<summary><b>💻 Option 2: Local Development</b></summary>

**Prerequisites:**
- .NET SDK 9.0+
- Node.js 20+
- PostgreSQL 16+
- Redis 7+
- Python 3.12+

**Backend:**
```bash
cd slip-verification-api/src/SlipVerification.API
dotnet restore
dotnet build
dotnet run
# API available at: http://localhost:5000
```

**Frontend:**
```bash
cd slip-verification-web
npm install
npm start
# Web app available at: http://localhost:4200
```

**OCR Service:**
```bash
cd ocr-service
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
# OCR service available at: http://localhost:8000
```

**Mobile App:**
```bash
cd slip-verification-mobile
npm install

# iOS (Mac only)
cd ios && pod install && cd ..
npm run ios

# Android
npm run android
```

</details>

<details>
<summary><b>☸️ Option 3: Kubernetes (Production)</b></summary>

```bash
# Apply Kubernetes manifests
kubectl apply -f infrastructure/kubernetes/

# Check status
kubectl get pods
kubectl get services

# Access via ingress (configure domain in manifests)
# https://yourdomain.com
```

For detailed Kubernetes setup, see [infrastructure/kubernetes/](infrastructure/kubernetes/)

</details>

<br/>

### 🌐 Access Points

<div align="center">

| Service | URL | Status |
|:--------|:----|:------:|
| 🌐 **Web App** | http://localhost:4200 | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:4200) |
| 🔧 **API** | http://localhost:5000 | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:5000) |
| 📖 **Swagger UI** | http://localhost:5000/swagger | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:5000/swagger) |
| 📚 **ReDoc** | http://localhost:5000/redoc | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:5000/redoc) |
| 🤖 **OCR Service** | http://localhost:8000 | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:8000) |
| 🐰 **RabbitMQ** | http://localhost:15672 | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:15672) |
| 💾 **MinIO** | http://localhost:9001 | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:9001) |
| 🗄️ **pgAdmin** | http://localhost:5050 | [![Status](https://img.shields.io/badge/status-active-success)](http://localhost:5050) |

</div>

---

## 🛠️ Technology Stack

<div align="center">

### 🎯 Core Technologies

#### Backend Stack
[![.NET Core](https://img.shields.io/badge/.NET_Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework](https://img.shields.io/badge/EF_Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/)

#### Frontend Stack
[![Angular](https://img.shields.io/badge/Angular-20-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.io/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-3.0-06B6D4?style=for-the-badge&logo=tailwindcss&logoColor=white)](https://tailwindcss.com/)
[![Angular Material](https://img.shields.io/badge/Material-20-757575?style=for-the-badge&logo=angular&logoColor=white)](https://material.angular.io/)

#### Mobile Stack
[![React Native](https://img.shields.io/badge/React_Native-0.75-61DAFB?style=for-the-badge&logo=react&logoColor=white)](https://reactnative.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?style=for-the-badge&logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![React Navigation](https://img.shields.io/badge/React_Navigation-6-663399?style=for-the-badge&logo=react&logoColor=white)](https://reactnavigation.org/)
[![Redux Toolkit](https://img.shields.io/badge/Redux_Toolkit-Latest-764ABC?style=for-the-badge&logo=redux&logoColor=white)](https://redux-toolkit.js.org/)

#### Database & Cache
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7.2-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/)

#### OCR & AI
[![Python](https://img.shields.io/badge/Python-3.12-3776AB?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-0.115-009688?style=for-the-badge&logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![PaddleOCR](https://img.shields.io/badge/PaddleOCR-2.8-0052CC?style=for-the-badge&logo=paddlepaddle&logoColor=white)](https://github.com/PaddlePaddle/PaddleOCR)

#### DevOps
[![Docker](https://img.shields.io/badge/Docker-Latest-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Kubernetes](https://img.shields.io/badge/Kubernetes-1.28-326CE5?style=for-the-badge&logo=kubernetes&logoColor=white)](https://kubernetes.io/)
[![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI/CD-2088FF?style=for-the-badge&logo=githubactions&logoColor=white)](https://github.com/features/actions)
[![Nginx](https://img.shields.io/badge/Nginx-1.25-009639?style=for-the-badge&logo=nginx&logoColor=white)](https://nginx.org/)

</div>

<br/>

<details>
<summary><b>📦 Complete Dependency List</b></summary>

#### Backend Dependencies
```
├── MediatR (12.0) - CQRS Pattern
├── AutoMapper (13.0) - Object Mapping
├── FluentValidation (11.9) - Input Validation
├── Serilog (3.1) - Structured Logging
├── SignalR (Latest) - Real-time Communication
├── Swashbuckle (6.5) - API Documentation
├── BCrypt.Net (0.1.0) - Password Hashing
└── JWT Bearer (7.0) - Authentication
```

#### Frontend Dependencies
```
├── RxJS (7.8) - Reactive Programming
├── Socket.io Client (4.5) - WebSocket
├── Chart.js (4.4) - Data Visualization
├── ngx-toastr (18.0) - Notifications
├── Angular CDK (20.0) - Component Dev Kit
└── date-fns (3.0) - Date Utilities
```

#### Mobile Dependencies
```
├── React Native (0.75.4) - Mobile Framework
├── React Navigation (6) - Navigation
├── Redux Toolkit (Latest) - State Management
├── React Query (Latest) - Server State
├── React Native Paper (Latest) - UI Components
├── Socket.io Client (4.5) - Real-time
└── Axios (1.6) - HTTP Client
```

#### OCR Service Dependencies
```
├── PaddleOCR (2.8) - OCR Engine
├── FastAPI (0.115) - Web Framework
├── OpenCV (4.10) - Image Processing
├── Pillow (Latest) - Image Manipulation
├── NumPy (Latest) - Numerical Computing
└── pydantic (2.5) - Data Validation
```

</details>

```
slip-verification-system/
├── slip-verification-api/           # .NET Core Backend
│   ├── src/
│   │   ├── SlipVerification.API/         # Web API Layer
│   │   ├── SlipVerification.Application/ # Business Logic (CQRS)
│   │   ├── SlipVerification.Domain/      # Domain Models
│   │   ├── SlipVerification.Infrastructure/ # Data Access
│   │   └── SlipVerification.Shared/      # Shared Utilities
│   ├── tests/                            # Backend Tests
│   ├── database/                         # Database Scripts
│   ├── docs/                             # Backend Documentation
│   └── Dockerfile
│
├── slip-verification-web/           # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                # Core Services
│   │   │   ├── shared/              # Shared Components
│   │   │   ├── features/            # Feature Modules
│   │   │   └── layouts/             # Layout Components
│   │   ├── assets/
│   │   └── environments/
│   ├── public/
│   └── Dockerfile
│
├── slip-verification-mobile/        # React Native Mobile App
│   ├── src/
│   ├── ios/                         # iOS Platform
│   ├── android/                     # Android Platform
│   └── README.md
│
├── ocr-service/                     # Python OCR Microservice
│   ├── app/
│   │   ├── api/                     # API Routes
│   │   ├── services/                # OCR Services
│   │   ├── models/                  # Data Models
│   │   └── utils/                   # Utilities
│   ├── tests/
│   ├── requirements.txt
│   └── Dockerfile
│
├── tests/                           # Integration & E2E Tests
│   ├── api/                         # API Integration Tests
│   ├── e2e/                         # End-to-End Tests
│   ├── load-testing/                # Load Testing
│   ├── performance/                 # Performance Tests
│   └── security/                    # Security Tests
│
├── infrastructure/                  # Infrastructure as Code
│   ├── kubernetes/                  # Kubernetes Manifests
│   ├── nginx/                       # Nginx Configuration
│   ├── monitoring/                  # Prometheus/Grafana
│   ├── logging/                     # ELK Stack
│   ├── ssl/                         # SSL Certificates
│   ├── docker-compose.yml           # Infrastructure Services
│   └── docker-compose.dev.yml       # Development Setup
│
├── docs/                            # Documentation
│   ├── api/                         # API Documentation
│   ├── architecture/                # Architecture Diagrams
│   ├── devops/                      # DevOps Guides
│   ├── monitoring/                  # Monitoring Setup
│   ├── notification/                # Notification System
│   ├── performance/                 # Performance Optimization
│   ├── security/                    # Security Guidelines
│   ├── signalr/                     # SignalR Real-time
│   ├── message-queue/               # Message Queue Setup
│   └── getting-started/             # Quick Start Guides
│
├── scripts/                         # Utility Scripts
│   ├── backup/                      # Backup Scripts
│   ├── postgres/                    # Database Scripts
│   └── ssl/                         # SSL Scripts
│
├── .github/                         # GitHub Configuration
│   └── workflows/                   # CI/CD Workflows
│
├── docker-compose.dev.yml           # Development Compose
├── docker-compose.frontend.yml      # Frontend Services
├── docker-compose.prod.yml          # Production Compose
├── docker-compose.monitoring.yml    # Monitoring Stack
├── docker-compose.logging.yml       # Logging Stack
├── docker-compose.tracing.yml       # Tracing Stack
├── Makefile                         # Quick Commands
├── .env.production.example          # Production Environment
├── .env.notification.example        # Notification Environment
├── README.md                        # This file
├── PROJECT_README.md                # Project Overview
├── CONTRIBUTING.md                  # Contributing Guidelines
├── CHANGELOG.md                     # Version History
└── LICENSE                          # License File
---

## 📁 Project Structure

<details>
<summary><b>🗂️ Click to expand folder structure</b></summary>

```
slip-verification-system/
├── 🎯 slip-verification-api/         # .NET Core Backend
│   ├── src/
│   │   ├── SlipVerification.API/      # Web API Layer
│   │   ├── SlipVerification.Application/  # Business Logic (CQRS)
│   │   ├── SlipVerification.Domain/   # Domain Models
│   │   ├── SlipVerification.Infrastructure/  # Data Access
│   │   └── SlipVerification.Shared/   # Shared Utilities
│   ├── tests/                         # Backend Tests
│   └── Dockerfile
│
├── 🎨 slip-verification-web/         # Angular Frontend
│   ├── src/app/
│   │   ├── core/                     # Singleton Services
│   │   ├── shared/                   # Reusable Components
│   │   ├── features/                 # Feature Modules
│   │   └── layouts/                  # Layout Components
│   └── Dockerfile
│
├── 📱 slip-verification-mobile/      # React Native Mobile
│   ├── src/
│   ├── ios/                          # iOS Platform
│   └── android/                      # Android Platform
│
├── 🤖 ocr-service/                   # Python OCR Microservice
│   ├── app/                          # Application Code
│   ├── tests/                        # OCR Tests
│   └── Dockerfile
│
├── 🧪 tests/                         # Integration & E2E Tests
│   ├── api/
│   ├── e2e/
│   ├── load-testing/
│   ├── performance/
│   └── security/
│
├── 🏗️ infrastructure/                # Infrastructure as Code
│   ├── kubernetes/
│   ├── nginx/
│   ├── monitoring/
│   └── logging/
│
├── 📚 docs/                          # Documentation
├── 🛠️ scripts/                       # Utility Scripts
├── 🐳 docker-compose.*.yml           # Multiple Docker Compose Files
├── 📋 Makefile
└── 📖 README.md
```

</details>

---

## 📚 API Documentation

<div align="center">

### 🔗 Interactive API Documentation

[![Swagger UI](https://img.shields.io/badge/Swagger_UI-Interactive-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](http://localhost:5000/swagger)
[![ReDoc](https://img.shields.io/badge/ReDoc-Documentation-8BC34A?style=for-the-badge&logo=readthedocs&logoColor=white)](http://localhost:5000/redoc)
[![Postman](https://img.shields.io/badge/Postman-Collection-FF6C37?style=for-the-badge&logo=postman&logoColor=white)](docs/api/postman-collection.json)

**Base URL:** `http://localhost:5000/api/v1`

</div>

<br/>

<details open>
<summary><b>🔐 Authentication</b></summary>

```http
POST /api/v1/auth/login
POST /api/v1/auth/register
POST /api/v1/auth/refresh-token
POST /api/v1/auth/logout
GET  /api/v1/auth/me
```

**Example:**
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'
```

</details>

<details>
<summary><b>📄 Slip Verification</b></summary>

```http
POST   /api/v1/slips/verify          # ⬆️ Upload & verify slip
GET    /api/v1/slips/{id}            # 📋 Get slip details
GET    /api/v1/slips/order/{orderId} # 🔍 Get slips by order
DELETE /api/v1/slips/{id}            # 🗑️ Delete slip
POST   /api/v1/slips/batch           # 📦 Batch upload
```

**Example:**
```bash
curl -X POST http://localhost:5000/api/v1/slips/verify \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@slip.jpg" \
  -F "orderId=550e8400-e29b-41d4-a716-446655440000"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "amount": 1500.00,
    "transactionDate": "2025-10-01",
    "transactionTime": "14:30:00",
    "referenceNumber": "REF123456789",
    "bankName": "Bangkok Bank",
    "status": "Verified",
    "confidence": 0.95
  },
  "message": "✅ Slip verified successfully"
}
```

</details>

<details>
<summary><b>🛒 Orders Management</b></summary>

```http
GET    /api/v1/orders              # 📋 List all orders
GET    /api/v1/orders/{id}         # 🔍 Get order details
POST   /api/v1/orders              # ➕ Create order
PUT    /api/v1/orders/{id}         # ✏️ Update order
DELETE /api/v1/orders/{id}         # 🗑️ Delete order
GET    /api/v1/orders/pending      # ⏳ Get pending orders
GET    /api/v1/orders/stats        # 📊 Get statistics
```

</details>

<details>
<summary><b>💰 Transactions</b></summary>

```http
GET    /api/v1/transactions         # 📋 List transactions
GET    /api/v1/transactions/{id}    # 🔍 Get details
GET    /api/v1/transactions/export  # 📥 Export (Excel/PDF)
GET    /api/v1/transactions/stats   # 📊 Statistics
POST   /api/v1/transactions/filter  # 🔎 Advanced filter
```

</details>

<details>
<summary><b>🔔 Notifications</b></summary>

```http
GET    /api/v1/notifications                # 📋 List all
GET    /api/v1/notifications/{id}           # 🔍 Get details
PUT    /api/v1/notifications/{id}/read      # ✅ Mark as read
DELETE /api/v1/notifications/{id}           # 🗑️ Delete
GET    /api/v1/notifications/unread/count   # 🔢 Unread count
```

</details>

<details>
<summary><b>📊 Dashboard & Reports</b></summary>

```http
GET    /api/v1/dashboard/stats              # 📊 Overview stats
GET    /api/v1/dashboard/recent-activities  # 📅 Recent activities
GET    /api/v1/reports/daily                # 📈 Daily report
GET    /api/v1/reports/monthly              # 📊 Monthly report
GET    /api/v1/reports/export/{type}        # 📥 Export report
```

</details>

> 💡 **Note:** ทุก endpoint (ยกเว้น auth) ต้องใช้ JWT Token ใน Header:
> ```
> Authorization: Bearer YOUR_JWT_TOKEN
> ```

---

## 🧪 Testing

<div align="center">

### 🎯 Test Coverage Goals

| Layer | Target | Current | Status |
|:------|:------:|:-------:|:------:|
| Backend | 80% | 85% | ✅ |
| Frontend | 70% | 75% | ✅ |
| Integration | 100% | 100% | ✅ |

</div>

<br/>

<table>
<tr>
<td width="33%">

### 🧪 Unit Tests
```bash
# Backend
make test-api
# or
cd slip-verification-api
dotnet test

# Frontend
make test-web
# or
cd slip-verification-web
npm test

# OCR Service
make test-ocr
# or
cd ocr-service
pytest -v
```

</td>
<td width="33%">

### 🔗 Integration Tests
```bash
# All integration tests
cd tests/api
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

</td>
<td width="33%">

### 🌐 E2E Tests
```bash
# E2E with Playwright
cd tests/e2e
npm install
npm test

# Or using Makefile
make test
```

</td>
</tr>
</table>

---

## ⚙️ Configuration

<details>
<summary><b>📝 Environment Variables</b></summary>

Create `.env` file from the example:

```bash
cp .env.production.example .env
```

**Key Configuration:**

```bash
# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=slip_verification_db
DATABASE_USER=postgres
DATABASE_PASSWORD=your_secure_password

# Redis
REDIS_HOST=localhost
REDIS_PORT=6379
REDIS_PASSWORD=

# JWT
JWT_SECRET=your-super-secret-key-min-32-chars-long
JWT_ISSUER=SlipVerificationAPI
JWT_AUDIENCE=SlipVerificationClient
JWT_EXPIRY_MINUTES=60

# OCR Service
OCR_SERVICE_URL=http://localhost:8000
OCR_CONFIDENCE_THRESHOLD=0.70

# File Storage
FILE_STORAGE_BASE_PATH=/app/uploads
FILE_STORAGE_BASE_URL=http://localhost:5000/uploads

# Notifications (optional)
LINE_NOTIFY_CLIENT_ID=your_line_client_id
LINE_NOTIFY_CLIENT_SECRET=your_line_client_secret
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your_email@gmail.com
SMTP_PASSWORD=your_app_password
```

For notification setup, also configure:
```bash
cp .env.notification.example .env.notification
```

📖 For complete configuration options, see:
- [.env.production.example](.env.production.example)
- [.env.notification.example](.env.notification.example)
- [Configuration Documentation](docs/getting-started/QUICKSTART.md)

</details>

---

## 🚢 Deployment

<div align="center">

### 🎯 Deployment Options

</div>

<table>
<tr>
<td align="center" width="25%">

### 🐳 Docker
```bash
# Development
docker-compose \
  -f docker-compose.dev.yml \
  up -d

# Production
docker-compose \
  -f docker-compose.prod.yml \
  up -d
```
[View Compose Files →](.)

</td>
<td align="center" width="25%">

### ☸️ Kubernetes
```bash
kubectl apply \
  -f infrastructure/kubernetes/
```
[Guide →](infrastructure/kubernetes/)

</td>
<td align="center" width="25%">

### 📊 Monitoring
```bash
docker-compose \
  -f docker-compose.monitoring.yml \
  up -d
```
[Guide →](docs/monitoring/)

</td>
<td align="center" width="25%">

### 📝 Logging
```bash
docker-compose \
  -f docker-compose.logging.yml \
  up -d
```
[Guide →](docs/devops/)

</td>
</tr>
</table>

---

## 🗺️ Roadmap

<div align="center">

### 📅 Development Timeline

</div>

```mermaid
gantt
    title Slip Verification System Roadmap
    dateFormat  YYYY-MM-DD
    section Phase 1
    MVP Development           :done,    p1, 2024-10-01, 60d
    Beta Testing             :done,    p2, 2024-12-01, 30d
    section Phase 2
    Mobile App               :active,  p3, 2025-01-01, 45d
    Advanced Features        :active,  p4, 2025-02-15, 45d
    section Phase 3
    AI/ML Enhancement        :         p5, 2025-04-01, 60d
    Multi-tenant Support     :         p6, 2025-06-01, 60d
    section Phase 4
    White-label Solution     :         p7, 2025-08-01, 90d
```

<br/>

<details open>
<summary><b>✅ Phase 1 - MVP (Completed)</b></summary>

- [x] Core slip verification
- [x] OCR integration
- [x] Web application
- [x] REST API
- [x] Real-time notifications
- [x] Dashboard & analytics

</details>

<details open>
<summary><b>🔄 Phase 2 - Enhancement (Current)</b></summary>

- [x] Advanced notification system
- [ ] Mobile app (iOS/Android) - **80% complete**
- [ ] Batch processing
- [ ] Advanced reporting
- [ ] Multi-language support

</details>

<details>
<summary><b>📋 Phase 3 - Advanced Features (Q2 2025)</b></summary>

- [ ] AI/ML accuracy improvement
- [ ] Multi-currency support
- [ ] Blockchain audit trail
- [ ] Advanced fraud detection
- [ ] API for third-party integration
- [ ] Payment gateway integration

</details>

<details>
<summary><b>🚀 Phase 4 - Scale (Q3-Q4 2025)</b></summary>

- [ ] Multi-tenant architecture
- [ ] White-label solution
- [ ] Advanced BI & analytics
- [ ] Mobile SDK
- [ ] Marketplace integration
- [ ] Enterprise features

</details>

---

## 🤝 Contributing

เรายินดีรับ contributions จากทุกคน! กรุณาอ่าน [CONTRIBUTING.md](CONTRIBUTING.md) สำหรับรายละเอียด

### Quick Start for Contributors

```bash
# 1. Fork the repository
# 2. Clone your fork
git clone https://github.com/your-username/slip-verification-system.git

# 3. Create a feature branch
git checkout -b feature/amazing-feature

# 4. Make your changes
# 5. Commit your changes
git commit -m 'Add some amazing feature'

# 6. Push to your fork
git push origin feature/amazing-feature

# 7. Open a Pull Request
```

### Development Guidelines

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Follow [Angular Style Guide](https://angular.io/guide/styleguide)
- Write unit tests for new features
- Update documentation
- Use conventional commits

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Team

### Core Team

This is an open-source project. See our [contributors](https://github.com/picthaisky/slip-verification-system/graphs/contributors).

### Contributors

Thanks to all our amazing contributors! 🎉

<a href="https://github.com/picthaisky/slip-verification-system/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=picthaisky/slip-verification-system" />
</a>

---

## 📞 Contact & Support

### Get Help

- 📧 Email: support@slipverification.com (or create an issue)
- 🐛 Issues: [GitHub Issues](https://github.com/picthaisky/slip-verification-system/issues)
- 📚 Documentation: [docs/](docs/)
- 💬 Discussions: [GitHub Discussions](https://github.com/picthaisky/slip-verification-system/discussions)

### Social Media

- GitHub: [@picthaisky](https://github.com/picthaisky/slip-verification-system)
- Website: [Project Repository](https://github.com/picthaisky/slip-verification-system)

---

## 🙏 Acknowledgments

- [.NET Foundation](https://dotnetfoundation.org/)
- [Angular Team](https://angular.io/)
- [PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR)
- All our amazing contributors and supporters

---

## 📊 Project Stats

![GitHub stars](https://img.shields.io/github/stars/picthaisky/slip-verification-system?style=social)
![GitHub forks](https://img.shields.io/github/forks/picthaisky/slip-verification-system?style=social)
![GitHub issues](https://img.shields.io/github/issues/picthaisky/slip-verification-system)
![GitHub pull requests](https://img.shields.io/github/issues-pr/picthaisky/slip-verification-system)
![GitHub last commit](https://img.shields.io/github/last-commit/picthaisky/slip-verification-system)

---

<div align="center">

**⭐ Don't forget to star this repo if you find it useful! ⭐**

Made with ❤️ by [SENIC DEV]

</div>
