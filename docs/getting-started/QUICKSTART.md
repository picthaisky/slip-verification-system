# Quick Start Guide

Get the Slip Verification System up and running in 5 minutes!

## 🚀 Prerequisites

- Docker & Docker Compose (Recommended)
- OR Node.js 20+ and .NET 9 SDK (Manual setup)

## ⚡ Quick Start with Docker

### 1. Clone the Repository

```bash
git clone https://github.com/picthaisky/slip-verification-system.git
cd slip-verification-system
```

### 2. Start All Services

```bash
docker-compose -f docker-compose.frontend.yml up -d
```

### 3. Access Applications

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **pgAdmin**: http://localhost:5050

### 4. Login Credentials

```
Admin:
Email: admin@example.com
Password: Admin123!

User:
Email: user@example.com
Password: User123!
```

## 🛠️ Manual Development Setup

### Frontend Only

```bash
cd slip-verification-web
npm install
npm start
# Open http://localhost:4200
```

### Backend Only

```bash
cd slip-verification-api
dotnet restore
dotnet build
cd src/SlipVerification.API
dotnet run
# API runs on http://localhost:5000
```

### Full Stack Development

**Terminal 1 - Backend:**
```bash
cd slip-verification-api/src/SlipVerification.API
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd slip-verification-web
npm start
```

## 📋 What's Included

### ✅ Frontend (Angular 20)
- Modern SPA with Material Design
- Slip upload with drag & drop
- Real-time notifications
- Dashboard with statistics
- User authentication
- Role-based access control

### ✅ Backend (.NET 9)
- RESTful API with Swagger
- JWT authentication
- CQRS pattern
- PostgreSQL database
- Redis caching
- Health checks

## 🎯 Next Steps

1. **Explore the Application**
   - Login with provided credentials
   - Try uploading a slip
   - View dashboard statistics

2. **Read Documentation**
   - Frontend: `slip-verification-web/FRONTEND_README.md`
   - Backend: `slip-verification-api/docs/BACKEND_README.md`
   - Full project: `PROJECT_README.md`

3. **Configure for Your Use**
   - Update environment variables
   - Configure database connection
   - Set JWT secret
   - Update API URLs

## 🐛 Troubleshooting

### Port Already in Use
```bash
# Frontend on different port
cd slip-verification-web
ng serve --port 4201

# Backend on different port
cd slip-verification-api/src/SlipVerification.API
dotnet run --urls "http://localhost:5001"
```

### Docker Issues
```bash
# Stop all containers
docker-compose -f docker-compose.frontend.yml down

# Remove volumes and restart
docker-compose -f docker-compose.frontend.yml down -v
docker-compose -f docker-compose.frontend.yml up -d
```

### Database Issues
```bash
# Reset database (Development only!)
cd slip-verification-api
dotnet ef database drop --project src/SlipVerification.Infrastructure
dotnet ef database update --project src/SlipVerification.Infrastructure
```

## 📚 Additional Resources

- **API Documentation**: http://localhost:5000/swagger
- **Frontend Guide**: [FRONTEND_README.md](../slip-verification-web/FRONTEND_README.md)
- **Backend Guide**: [BACKEND_README.md](../slip-verification-api/docs/BACKEND_README.md)
- **Project Overview**: [PROJECT_README.md](../PROJECT_README.md)

## 🤝 Need Help?

- Check the [Troubleshooting](#troubleshooting) section
- Review the full documentation
- Create a GitHub issue
- Contact support

---

**Ready to go!** 🎉 Your Slip Verification System is now running!
