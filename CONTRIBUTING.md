# Contributing to Slip Verification System

Thank you for your interest in contributing to the Slip Verification System! This document provides guidelines and instructions for contributing.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Testing](#testing)

## Code of Conduct

Please be respectful and constructive in all interactions. We are committed to providing a welcoming and inspiring community for all.

## Getting Started

### Prerequisites

- .NET SDK 9.0+
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16+ (or use Docker)
- Redis 7+ (or use Docker)

### Setting Up Development Environment

1. **Clone the repository**
   ```bash
   git clone https://github.com/picthaisky/slip-verification-system.git
   cd slip-verification-system
   ```

2. **Set up environment variables**
   ```bash
   cp .env.production.example .env
   # Edit .env with your configuration
   ```

3. **Start infrastructure services**
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

4. **Run the API**
   ```bash
   cd slip-verification-api/src/SlipVerification.API
   dotnet run
   ```

5. **Run the Web frontend**
   ```bash
   cd slip-verification-web
   npm install
   npm start
   ```

## Development Workflow

### Branch Naming Convention

- `feature/` - New features (e.g., `feature/add-report-export`)
- `fix/` - Bug fixes (e.g., `fix/login-redirect-issue`)
- `docs/` - Documentation changes
- `refactor/` - Code refactoring
- `test/` - Adding or updating tests

### Commit Messages

Use conventional commits format:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Examples:
- `feat(api): add dashboard statistics endpoint`
- `fix(web): resolve login redirect issue`
- `docs: update API documentation`

## Pull Request Process

1. Ensure all tests pass
2. Update documentation if needed
3. Request review from at least one maintainer
4. Squash commits before merging

## Coding Standards

### Backend (.NET)

- Follow Microsoft C# Coding Conventions
- Use async/await for I/O operations
- Implement CQRS pattern with MediatR
- Add XML documentation for public APIs

### Frontend (Angular)

- Follow Angular Style Guide
- Use standalone components
- Implement lazy loading for feature modules
- Add unit tests for services and components

### Mobile (React Native)

- Follow React Native best practices
- Use TypeScript for type safety
- Implement proper error handling
- Support both iOS and Android

## Testing

### Running Tests

```bash
# API Unit Tests
cd slip-verification-api
dotnet test

# Web Tests
cd slip-verification-web
npm test

# Mobile Tests
cd slip-verification-mobile
npm test
```

### Test Coverage Requirements

- Minimum 70% code coverage for new features
- All critical paths must have tests
- Integration tests for API endpoints

---

Thank you for contributing! 🎉
