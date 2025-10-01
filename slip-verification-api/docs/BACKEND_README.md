# Slip Verification System - Backend API

## 🏗️ Architecture Overview

This backend API is built using **.NET Core 9** following **Clean Architecture** principles (Onion Architecture) with **CQRS pattern** using MediatR.

### Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    API Layer (Presentation)                  │
│  Controllers, Middleware, Filters, Configuration            │
└──────────────────────┬───────────────────────────────────────┘
                       │
┌──────────────────────▼───────────────────────────────────────┐
│              Application Layer (Business Logic)              │
│  CQRS Commands/Queries, DTOs, Validators, Services          │
└──────────────────────┬───────────────────────────────────────┘
                       │
┌──────────────────────▼───────────────────────────────────────┐
│              Domain Layer (Core Business)                    │
│  Entities, Value Objects, Domain Events, Interfaces         │
└──────────────────────────────────────────────────────────────┘
                       ▲
┌──────────────────────┴───────────────────────────────────────┐
│        Infrastructure Layer (External Concerns)              │
│  Data Access, External Services, File Storage, Cache        │
└──────────────────────────────────────────────────────────────┘
```

## 📦 Solution Structure

```
SlipVerification.sln
├── src/
│   ├── SlipVerification.API/              # Web API Layer
│   │   ├── Controllers/v1/                # API Controllers
│   │   ├── Middleware/                    # Custom Middleware
│   │   ├── Program.cs                     # Application Entry Point
│   │   └── appsettings.json              # Configuration
│   │
│   ├── SlipVerification.Application/      # Application Layer
│   │   ├── Features/                      # CQRS Features
│   │   │   ├── Slips/
│   │   │   │   ├── Commands/             # Write Operations
│   │   │   │   └── Queries/              # Read Operations
│   │   │   ├── Orders/
│   │   │   └── Transactions/
│   │   ├── DTOs/                         # Data Transfer Objects
│   │   ├── Interfaces/                   # Application Interfaces
│   │   └── Common/                       # Shared Application Logic
│   │
│   ├── SlipVerification.Domain/           # Domain Layer
│   │   ├── Entities/                     # Domain Entities
│   │   ├── Enums/                        # Enumerations
│   │   ├── Interfaces/                   # Domain Interfaces
│   │   └── Common/                       # Base Classes
│   │
│   ├── SlipVerification.Infrastructure/   # Infrastructure Layer
│   │   ├── Data/                         # Database Context
│   │   │   ├── Configurations/          # Entity Configurations
│   │   │   └── Repositories/            # Repository Implementations
│   │   └── Services/                     # External Services
│   │
│   └── SlipVerification.Shared/           # Shared Kernel
│       ├── Results/                      # Result Objects
│       ├── Exceptions/                   # Custom Exceptions
│       └── Extensions/                   # Extension Methods
│
└── tests/
    ├── SlipVerification.UnitTests/
    ├── SlipVerification.IntegrationTests/
    └── SlipVerification.FunctionalTests/
```

## 🚀 Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 16+](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download) (Optional, for caching)
- [Docker](https://www.docker.com/get-started) (Optional, for containerized deployment)

### Option 1: Run with Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/yourusername/slip-verification-system.git
cd slip-verification-system

# Start all services with Docker Compose
docker-compose up -d

# Check service health
docker-compose ps

# View logs
docker-compose logs -f api
```

Services will be available at:
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health
- **pgAdmin**: http://localhost:5050

### Option 2: Run Locally

```bash
# 1. Setup PostgreSQL and Redis
# Make sure PostgreSQL is running on localhost:5432
# Make sure Redis is running on localhost:6379

# 2. Update connection strings
cp .env.example .env
# Edit .env with your database credentials

# 3. Restore dependencies
dotnet restore

# 4. Build the solution
dotnet build

# 5. Run database migrations (if applicable)
# dotnet ef database update --project src/SlipVerification.Infrastructure

# 6. Run the API
cd src/SlipVerification.API
dotnet run

# API will start at https://localhost:5001 or http://localhost:5000
```

## 🔧 Configuration

### appsettings.json

Key configuration sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SlipVerificationDb;...",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "YourSecretKey",
    "Issuer": "SlipVerificationAPI",
    "Audience": "SlipVerificationClient",
    "ExpiryMinutes": "60"
  },
  "FileStorage": {
    "BasePath": "uploads",
    "BaseUrl": "http://localhost:5000/uploads"
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "Window": "1m"
  }
}
```

### Environment Variables

See `.env.example` for all available environment variables.

## 📚 API Documentation

### Swagger/OpenAPI

Once the application is running, access the interactive API documentation at:
- **Swagger UI**: http://localhost:5000/swagger

### Authentication

The API uses JWT Bearer authentication. To access protected endpoints:

1. Obtain a token from the authentication endpoint (implement as needed)
2. Add the token to request headers:
   ```
   Authorization: Bearer <your-token>
   ```

### API Endpoints

#### Slip Verification API

```http
POST   /api/v1/slips/verify              # Upload and verify a slip
GET    /api/v1/slips/{id}                # Get slip by ID
GET    /api/v1/slips/order/{orderId}     # Get slips by order ID
DELETE /api/v1/slips/{id}                # Delete slip (soft delete)
```

#### Order Management API

```http
GET    /api/v1/orders                    # Get all orders (paginated)
GET    /api/v1/orders/{id}               # Get order by ID
PUT    /api/v1/orders/{id}/status        # Update order status
GET    /api/v1/orders/pending-payment    # Get orders pending payment
```

#### Health Check

```http
GET    /health                           # Application health status
```

## 🏛️ Design Patterns

### CQRS (Command Query Responsibility Segregation)

Commands and queries are separated for better scalability and maintainability.

**Example Command:**
```csharp
public class VerifySlipCommand : IRequest<Result<SlipVerificationDto>>
{
    public Guid OrderId { get; set; }
    public byte[] ImageData { get; set; }
    // ...
}
```

**Example Query:**
```csharp
public class GetSlipByIdQuery : IRequest<Result<SlipVerificationDto>>
{
    public Guid Id { get; set; }
}
```

### Repository Pattern + Unit of Work

Data access is abstracted through repositories and managed via Unit of Work.

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    // ...
}
```

### Result Pattern

Operations return `Result<T>` objects instead of throwing exceptions for business logic errors.

```csharp
var result = await _mediator.Send(command);
if (!result.IsSuccess)
{
    return BadRequest(result.ErrorMessage);
}
return Ok(result.Data);
```

## 🔐 Security Features

- **JWT Authentication** - Token-based authentication with refresh tokens
- **Role-Based Authorization** - Admin, User, and Guest roles
- **Input Validation** - FluentValidation for request validation
- **SQL Injection Prevention** - Parameterized queries via EF Core
- **XSS Protection** - Built-in ASP.NET Core protections
- **Rate Limiting** - Configurable request throttling
- **CORS** - Configured allowed origins

## 🚦 Performance Features

- **Response Compression** - Gzip and Brotli compression
- **Caching with Redis** - Distributed caching for frequently accessed data
- **Async/Await** - Asynchronous operations throughout
- **Connection Pooling** - Database connection pooling
- **Query Optimization** - Indexed database columns

## 📊 Monitoring & Logging

### Serilog

Structured logging with multiple sinks:
- Console output for development
- File logging with rolling files
- PostgreSQL logging (configurable)

### Health Checks

Health check endpoint monitors:
- Database connectivity
- Redis connectivity
- Application health

Access at: `GET /health`

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/SlipVerification.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 🐳 Docker Commands

```bash
# Build images
docker-compose build

# Start services
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f api

# Execute commands in container
docker-compose exec api bash

# Rebuild and restart
docker-compose up -d --build
```

## 🔧 Development Workflow

1. **Create a Feature Branch**
   ```bash
   git checkout -b feature/new-feature
   ```

2. **Implement Changes**
   - Add entities to Domain layer
   - Create commands/queries in Application layer
   - Implement handlers
   - Add controllers in API layer

3. **Build and Test**
   ```bash
   dotnet build
   dotnet test
   ```

4. **Commit and Push**
   ```bash
   git add .
   git commit -m "feat: add new feature"
   git push origin feature/new-feature
   ```

## 📝 Database Migrations

### Entity Framework Core Migrations

```bash
# Add new migration
dotnet ef migrations add InitialCreate \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# Update database
dotnet ef database update \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# Remove last migration
dotnet ef migrations remove \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Troubleshooting

### Common Issues

**Database Connection Error**
```bash
# Check PostgreSQL is running
docker-compose ps postgres

# Check connection string in appsettings.json
```

**Redis Connection Error**
```bash
# Check Redis is running
docker-compose ps redis

# Redis is optional - the app will work without it
```

**Port Already in Use**
```bash
# Change port in docker-compose.yml or appsettings.json
```

## 📞 Support

For issues and questions:
- Open an issue on GitHub
- Contact: support@slipverification.com

---

**Built with ❤️ using .NET Core 9**
