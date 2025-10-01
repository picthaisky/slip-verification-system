# Slip Verification System

Complete payment slip verification system with QR code support, featuring a modern Angular 20 frontend and .NET Core backend.

## 🎯 Overview

A comprehensive web application for real-time payment slip verification with automatic OCR processing, order management, and transaction tracking.

## 🏗️ Architecture

```
slip-verification-system/
├── slip-verification-web/       # Angular 20 Web Frontend
├── slip-verification-mobile/    # React Native Mobile App (iOS & Android)
├── slip-verification-api/       # .NET Core Backend API
└── ocr-service/                 # Python OCR Microservice
```

## 🚀 Quick Start

### Prerequisites
- **Backend**: .NET 9 SDK, PostgreSQL, Redis
- **Frontend**: Node.js 20+, npm 9+
- **Mobile**: Node.js 20+, React Native CLI, Xcode (iOS), Android Studio
- **OCR Service**: Python 3.12+, Docker (optional)

### Start Full Stack with Docker

```bash
# Start all services (Frontend, Backend, OCR, PostgreSQL, Redis)
docker-compose -f docker-compose.frontend.yml up -d

# Access applications
# Frontend: http://localhost:4200
# Backend API: http://localhost:5000
# OCR Service: http://localhost:8000
# Swagger: http://localhost:5000/swagger
# OCR API Docs: http://localhost:8000/docs
```

### Development Setup

#### Backend
```bash
cd slip-verification-api
dotnet restore
dotnet build
cd src/SlipVerification.API
dotnet run
```

#### OCR Service
```bash
cd ocr-service
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
```

#### Frontend
```bash
cd slip-verification-web
npm install
npm start
# Open http://localhost:4200
```

#### Mobile App
```bash
cd slip-verification-mobile
npm install

# iOS (Mac only)
cd ios && pod install && cd ..
npm run ios

# Android
npm run android
```

## 📚 Documentation

- **Backend**: See [slip-verification-api/docs/](slip-verification-api/docs/)
  - [Backend README](slip-verification-api/docs/BACKEND_README.md)
  - [API Documentation](slip-verification-api/docs/API_DOCUMENTATION.md)
  - [Project Summary](slip-verification-api/docs/PROJECT_SUMMARY.md)

- **Web Frontend**: See [slip-verification-web/FRONTEND_README.md](slip-verification-web/FRONTEND_README.md)

- **Mobile App**: See [slip-verification-mobile/README.md](slip-verification-mobile/README.md)
  - [Implementation Summary](slip-verification-mobile/IMPLEMENTATION_SUMMARY.md)
  - [Quick Start](slip-verification-mobile/QUICKSTART.md)
  - [Deployment Guide](slip-verification-mobile/DEPLOYMENT.md)

- **OCR Service**: See [ocr-service/README.md](ocr-service/README.md)

## 🛠️ Technology Stack

### Web Frontend
- **Angular 20** - Modern standalone components architecture
- **TypeScript 5.6+** - Type-safe development
- **Tailwind CSS 3** - Utility-first styling
- **Angular Material 20** - Material Design components
- **RxJS 7** - Reactive programming
- **Socket.io Client** - Real-time communication
- **Chart.js** - Data visualization

### Mobile App
- **React Native 0.75.4** - Cross-platform mobile development
- **TypeScript 5.6+** - Type-safe development
- **React Navigation 6** - Navigation library
- **React Native Paper** - Material Design components
- **Redux Toolkit** - State management
- **React Query** - Server state caching
- **Socket.io Client** - Real-time updates
- **Axios** - HTTP client

### Backend
- **.NET 9** - Latest .NET framework
- **PostgreSQL** - Primary database
- **Redis** - Caching layer
- **EF Core 9** - ORM
- **MediatR** - CQRS pattern
- **JWT** - Authentication
- **Serilog** - Logging
- **Swagger** - API documentation

### OCR Service
- **Python 3.12** - Modern Python
- **FastAPI** - High-performance web framework
- **PaddleOCR** - OCR engine
- **EasyOCR** - Fallback OCR engine
- **OpenCV** - Image processing
- **Pillow** - Image manipulation
- **NumPy** - Numerical computing
- **Redis** - Queue and caching

## ✨ Features

### Frontend Features
- ✅ Slip Upload with drag & drop
- ✅ Real-time notifications
- ✅ Dashboard with statistics
- ✅ Order management
- ✅ Transaction history
- ✅ Role-based access control
- ✅ Responsive design
- ✅ Loading states
- ✅ Error handling

### Backend Features
- ✅ Slip verification API
- ✅ JWT authentication
- ✅ Role-based authorization
- ✅ CQRS pattern
- ✅ Repository pattern
- ✅ Unit of Work
- ✅ Redis caching
- ✅ Rate limiting
- ✅ Health checks
- ✅ API versioning
- ✅ Comprehensive logging

## 📁 Project Structure

### Frontend (slip-verification-web/)
```
src/app/
├── core/                    # Core services & guards
│   ├── services/           # API, Auth, WebSocket, Notification
│   ├── guards/             # Auth & Role guards
│   ├── interceptors/       # HTTP interceptors
│   └── models/             # Domain models
├── shared/                 # Shared components
│   ├── components/         # Reusable UI components
│   └── pipes/              # Custom pipes
├── features/               # Feature modules
│   ├── auth/              # Authentication
│   ├── dashboard/         # Dashboard
│   ├── slip-upload/       # Slip upload
│   ├── order-management/  # Orders
│   └── transaction-history/ # Transactions
└── layouts/               # Layout components
```

### Backend (slip-verification-api/)
```
src/
├── SlipVerification.API/           # Web API Layer
├── SlipVerification.Application/   # Application Layer (CQRS)
├── SlipVerification.Domain/        # Domain Layer (Entities)
├── SlipVerification.Infrastructure/ # Infrastructure (Data Access)
└── SlipVerification.Shared/        # Shared Utilities
```

## 🔧 Configuration

### Frontend Environment
Edit `slip-verification-web/src/environments/environment.ts`:
```typescript
export const environment = {
  apiUrl: 'http://localhost:5000/api/v1',
  wsUrl: 'http://localhost:5000',
  uploadMaxSize: 5 * 1024 * 1024,
};
```

### Backend Configuration
Edit `slip-verification-api/src/SlipVerification.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SlipVerificationDb;..."
  },
  "Redis": {
    "Configuration": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-secret-key"
  }
}
```

## 🧪 Testing

### Frontend
```bash
cd slip-verification-web
npm test              # Unit tests
npm run e2e          # E2E tests
```

### Backend
```bash
cd slip-verification-api
dotnet test          # All tests
```

## 🚢 Deployment

### Docker Deployment
```bash
# Build and run all services
docker-compose -f docker-compose.frontend.yml up --build

# Production build
docker-compose -f docker-compose.frontend.yml -f docker-compose.prod.yml up -d
```

### Manual Deployment

#### Frontend
```bash
cd slip-verification-web
npm run build
# Deploy dist/slip-verification-web/ to web server
```

#### Backend
```bash
cd slip-verification-api
dotnet publish -c Release
# Deploy to IIS or Linux server
```

## 📊 API Endpoints

Base URL: `http://localhost:5000/api/v1`

- `POST /auth/login` - User authentication
- `POST /slips/verify` - Upload and verify slip
- `GET /slips/{id}` - Get slip details
- `GET /orders` - List orders
- `GET /transactions` - Transaction history
- `GET /health` - Health check

See [API Documentation](slip-verification-api/docs/API_DOCUMENTATION.md) for complete endpoint list.

## 🔐 Default Credentials

For development/testing only:

```
Admin:
- Email: admin@example.com
- Password: Admin123!

User:
- Email: user@example.com
- Password: User123!
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'feat: add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

Follow [Conventional Commits](https://www.conventionalcommits.org/) for commit messages.

## 📝 Development Guidelines

### Code Style
- **Frontend**: Follow Angular style guide
- **Backend**: Follow C# coding conventions
- Use ESLint/Prettier for TypeScript
- Use EditorConfig for consistency

### Git Workflow
```bash
# Feature development
git checkout -b feature/my-feature
git commit -m "feat: add my feature"

# Bug fixes
git checkout -b fix/bug-description
git commit -m "fix: resolve bug"

# Documentation
git commit -m "docs: update readme"
```

## 🐛 Troubleshooting

### Frontend Issues
```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install

# Build issues
ng build --verbose
```

### Backend Issues
```bash
# Database issues
dotnet ef database drop
dotnet ef database update

# Build issues
dotnet clean
dotnet restore
dotnet build
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Team

- Backend Development: .NET Core API
- Frontend Development: Angular 20 SPA
- Database: PostgreSQL + Redis

## 📞 Support

For issues and questions:
- GitHub Issues: [Create an issue](https://github.com/picthaisky/slip-verification-system/issues)
- Email: support@slipverification.com

## 🗺️ Roadmap

- [ ] Mobile application (React Native)
- [x] OCR service integration
- [ ] Webhook notifications
- [ ] Multi-language support (TH/EN)
- [ ] Advanced analytics
- [ ] Export reports (PDF/Excel)
- [ ] Bulk operations
- [ ] Admin dashboard enhancements

## 📈 Status

- ✅ Backend API - Complete
- ✅ Frontend Application - Complete
- ✅ OCR Service - Complete
- ⏳ Mobile App - Planned
- ⏳ Advanced Analytics - Planned
