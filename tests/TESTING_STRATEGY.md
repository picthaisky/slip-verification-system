# Comprehensive Testing Strategy - Slip Verification System

## Table of Contents
1. [Overview](#overview)
2. [Testing Approach](#testing-approach)
3. [Test Types](#test-types)
4. [Test Coverage Requirements](#test-coverage-requirements)
5. [Testing Tools](#testing-tools)
6. [Test Execution](#test-execution)
7. [CI/CD Integration](#cicd-integration)
8. [Reporting](#reporting)

## Overview

This document outlines the comprehensive testing strategy for the Slip Verification System, covering all testing types from unit tests to security testing.

### Goals
- Ensure code quality and reliability
- Achieve >80% backend code coverage
- Achieve >70% frontend code coverage
- Validate security requirements
- Performance benchmarking
- Continuous quality assurance

## Testing Approach

### Testing Pyramid
```
           /\
          /  \    E2E Tests (5%)
         /____\
        /      \   Integration Tests (15%)
       /________\
      /          \  Unit Tests (80%)
     /____________\
```

### Test-Driven Development (TDD)
1. Write failing test first
2. Implement minimal code to pass
3. Refactor while keeping tests green
4. Repeat

## Test Types

### 1. Unit Tests

#### Backend (.NET Core with xUnit)
**Location**: `slip-verification-api/tests/SlipVerification.UnitTests/`

**What to Test**:
- Business logic in command handlers
- Service methods
- Domain entities validation
- Utility functions
- Data transformations

**Example**:
```csharp
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccess()
{
    // Arrange
    var command = new VerifySlipCommand { ... };
    
    // Act
    var result = await _handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.True(result.IsSuccess);
}
```

**Running Tests**:
```bash
cd slip-verification-api
dotnet test tests/SlipVerification.UnitTests/
```

**Coverage Report**:
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
```

#### Frontend (Angular with Jasmine/Karma)
**Location**: `slip-verification-web/src/app/**/*.spec.ts`

**What to Test**:
- Component logic
- Service methods
- Pipes and filters
- Directives
- Guards and interceptors

**Running Tests**:
```bash
cd slip-verification-web
npm test
```

**Coverage Report**:
```bash
npm test -- --code-coverage
```

#### Mobile (React Native with Jest)
**Location**: `slip-verification-mobile/src/__tests__/`

**Running Tests**:
```bash
cd slip-verification-mobile
npm test
```

### 2. Integration Tests

#### Backend API Integration Tests
**Location**: `slip-verification-api/tests/SlipVerification.IntegrationTests/`

**What to Test**:
- API endpoints
- Database operations
- External service integrations
- Authentication flow
- File upload/download

**Example**:
```csharp
[Fact]
public async Task PostSlip_ValidData_Returns200()
{
    var response = await _client.PostAsync("/api/v1/slips/verify", content);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

**Running Tests**:
```bash
cd slip-verification-api
dotnet test tests/SlipVerification.IntegrationTests/
```

### 3. API Tests

#### REST Client Tests
**Location**: `tests/api/*.http`

**Files**:
- `auth.http` - Authentication endpoints
- `slips.http` - Slip verification endpoints
- `orders.http` - Order management endpoints

**Usage**:
1. Install REST Client extension in VS Code
2. Open `.http` files
3. Click "Send Request" above each request
4. Verify responses

**Tools**:
- VS Code REST Client
- Postman
- Insomnia

### 4. Performance Tests

#### K6 Load Testing
**Location**: `tests/performance/`

**Test Scenarios**:

1. **Load Test** (`load-test.js`)
   - Simulates normal expected load
   - 50-100 concurrent users
   - 5-10 minute duration
   
   ```bash
   k6 run tests/performance/load-test.js
   ```

2. **Stress Test** (`stress-test.js`)
   - Tests system under heavy load
   - 100-400 concurrent users
   - Identifies breaking point
   
   ```bash
   k6 run tests/performance/stress-test.js
   ```

3. **Spike Test** (`spike-test.js`)
   - Tests sudden traffic spikes
   - 20 → 500 → 20 users
   - Tests recovery capability
   
   ```bash
   k6 run tests/performance/spike-test.js
   ```

**Performance Metrics**:
- Response time (p95, p99)
- Throughput (requests/second)
- Error rate
- Resource utilization

**Thresholds**:
```javascript
thresholds: {
  http_req_duration: ['p(95)<500'],  // 95% < 500ms
  http_req_failed: ['rate<0.01'],    // Error rate < 1%
}
```

### 5. Security Tests

#### OWASP ZAP
**Location**: `tests/security/`

**Running ZAP Scan**:
```bash
cd tests/security
./run-zap-scan.sh
```

**Security Checklist**:
See `tests/security/SECURITY_TESTING_CHECKLIST.md` for comprehensive security testing checklist covering:
- Authentication & Authorization
- Input Validation
- SQL Injection
- XSS (Cross-Site Scripting)
- CSRF (Cross-Site Request Forgery)
- Sensitive Data Exposure
- Security Misconfiguration

**Manual Security Testing**:
1. Review checklist items
2. Execute tests manually
3. Document findings
4. Prioritize by severity
5. Create remediation plan

### 6. End-to-End Tests

#### Cypress (Web)
**Location**: `slip-verification-web/cypress/e2e/`

**Test Scenarios**:
- Complete slip upload flow
- Login and navigation
- Order creation and verification
- Admin workflows

**Running E2E Tests**:
```bash
cd slip-verification-web
npm run e2e
```

## Test Coverage Requirements

### Backend (.NET Core)
**Target**: >80% code coverage

**Measurement**:
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml \
  -targetdir:coverage \
  -reporttypes:"Html;Cobertura"
```

**Minimum Coverage**:
- Domain Layer: 90%
- Application Layer: 85%
- Infrastructure Layer: 70%
- API Controllers: 80%

### Frontend (Angular)
**Target**: >70% code coverage

**Measurement**:
```bash
npm test -- --code-coverage --watch=false
```

**Minimum Coverage**:
- Components: 75%
- Services: 85%
- Guards: 90%
- Pipes: 80%

### Critical Paths
**Target**: 100% coverage

Critical paths include:
- Authentication flow
- Slip verification process
- Payment processing
- Order management
- Security-related code

## Testing Tools

### Backend Testing
- **xUnit**: Unit and integration testing framework
- **Moq**: Mocking framework for .NET
- **FluentAssertions**: Assertion library
- **Coverlet**: Code coverage tool
- **ReportGenerator**: Coverage report generator

### Frontend Testing
- **Jest**: JavaScript testing framework
- **Karma**: Test runner for Angular
- **Jasmine**: BDD testing framework
- **Cypress**: E2E testing framework
- **Testing Library**: React component testing

### API Testing
- **REST Client**: VS Code extension
- **Postman**: API testing platform
- **Insomnia**: REST client

### Performance Testing
- **k6**: Load testing tool
- **Artillery**: Alternative load testing
- **Apache Bench**: Simple load testing

### Security Testing
- **OWASP ZAP**: Security scanner
- **SQLMap**: SQL injection tool
- **Nikto**: Web server scanner
- **Burp Suite**: Web security testing

## Test Execution

### Local Development
```bash
# Backend unit tests
cd slip-verification-api
dotnet test

# Frontend tests
cd slip-verification-web
npm test

# Mobile tests
cd slip-verification-mobile
npm test

# OCR service tests
cd ocr-service
pytest

# API tests
# Use REST Client in VS Code or Postman

# Performance tests
k6 run tests/performance/load-test.js

# Security tests
cd tests/security
./run-zap-scan.sh
```

### Test Data Management

#### Test Fixtures
Store test data in:
- `tests/fixtures/` - Shared test data
- `**/TestData/` - Component-specific data

#### Database Seeding
```csharp
public class TestDataSeeder
{
    public static void SeedTestData(DbContext context)
    {
        // Add test users, orders, slips, etc.
    }
}
```

### Mocking External Dependencies

#### Backend Mocks
```csharp
var mockRepository = new Mock<IRepository<Order>>();
mockRepository
    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(testOrder);
```

#### Frontend Mocks
```typescript
const mockApiService = jasmine.createSpyObj('ApiService', ['uploadSlip']);
mockApiService.uploadSlip.and.returnValue(of({ success: true }));
```

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: Test Suite

on: [push, pull_request]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '9.0.x'
      - name: Run Tests
        run: |
          cd slip-verification-api
          dotnet test --collect:"XPlat Code Coverage"
      - name: Generate Coverage Report
        run: |
          reportgenerator -reports:**/coverage.cobertura.xml \
            -targetdir:coverage
      - name: Upload Coverage
        uses: codecov/codecov-action@v2
        with:
          files: ./coverage/Cobertura.xml

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup Node
        uses: actions/setup-node@v2
        with:
          node-version: '20'
      - name: Install Dependencies
        run: |
          cd slip-verification-web
          npm ci
      - name: Run Tests
        run: |
          cd slip-verification-web
          npm test -- --code-coverage --watch=false
      - name: Upload Coverage
        uses: codecov/codecov-action@v2

  performance-tests:
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v2
      - name: Setup k6
        run: |
          sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6
      - name: Run Load Test
        run: k6 run tests/performance/load-test.js

  security-tests:
    runs-on: ubuntu-latest
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'
    steps:
      - uses: actions/checkout@v2
      - name: Run ZAP Scan
        run: |
          docker run --rm -v $(pwd):/zap/wrk:rw \
            owasp/zap2docker-stable zap-baseline.py \
            -t http://localhost:5000 \
            -r zap-report.html
```

### Test Stages in CI/CD

1. **Pre-commit**: Fast unit tests
2. **PR Validation**: Unit + Integration tests
3. **Main Branch**: All tests including E2E
4. **Scheduled**: Performance + Security tests (nightly)

## Reporting

### Test Reports

#### JUnit XML Reports
Generated by xUnit and Jest for CI/CD integration.

#### HTML Reports
```bash
# Backend
dotnet test --logger:"html;LogFileName=test-results.html"

# Frontend
npm test -- --reporters=default --reporters=html
```

#### Coverage Reports
```bash
# Backend
reportgenerator -reports:**/coverage.cobertura.xml \
  -targetdir:coverage \
  -reporttypes:"Html"

# Frontend
npm test -- --code-coverage
# Open coverage/index.html
```

### Dashboard Integration

#### Codecov
- Automatic coverage tracking
- PR comments with coverage diff
- Trend analysis

#### SonarQube
- Code quality metrics
- Security vulnerabilities
- Technical debt tracking

### Test Metrics

Track these metrics:
- Test count (total, passing, failing)
- Code coverage (line, branch, function)
- Test execution time
- Flaky test rate
- Test maintenance effort

## Best Practices

### Writing Good Tests

1. **Arrange-Act-Assert (AAA)**
   ```csharp
   // Arrange - Set up test data
   var command = new TestCommand { ... };
   
   // Act - Execute the operation
   var result = await handler.Handle(command);
   
   // Assert - Verify the outcome
   Assert.True(result.IsSuccess);
   ```

2. **Test One Thing**
   - Each test should verify one specific behavior
   - Avoid multiple assertions for different concerns

3. **Descriptive Names**
   ```csharp
   [Fact]
   public async Task Handle_OrderNotFound_ReturnsFailure()
   ```

4. **Use Test Data Builders**
   ```csharp
   var order = new OrderBuilder()
       .WithAmount(1000)
       .WithStatus(OrderStatus.Pending)
       .Build();
   ```

5. **Avoid Test Interdependence**
   - Tests should be independent
   - Can run in any order
   - No shared state

### Maintaining Tests

1. **Keep Tests Fast**
   - Unit tests: < 100ms
   - Integration tests: < 1s
   - E2E tests: < 10s

2. **Regular Refactoring**
   - Remove duplicate code
   - Update obsolete tests
   - Clean up test data

3. **Test Hygiene**
   - Delete unused tests
   - Fix flaky tests immediately
   - Update documentation

## Troubleshooting

### Common Issues

1. **Flaky Tests**
   - Check for timing issues
   - Review test dependencies
   - Verify test data setup

2. **Slow Tests**
   - Profile test execution
   - Reduce database calls
   - Use in-memory databases

3. **Coverage Gaps**
   - Identify uncovered code
   - Add targeted tests
   - Review critical paths

## Resources

### Documentation
- [xUnit Documentation](https://xunit.net/)
- [Jest Documentation](https://jestjs.io/)
- [k6 Documentation](https://k6.io/docs/)
- [OWASP Testing Guide](https://owasp.org/www-project-web-security-testing-guide/)

### Training
- TDD fundamentals
- Mock framework usage
- Security testing practices
- Performance testing strategies

## Conclusion

This comprehensive testing strategy ensures:
- High code quality
- Security compliance
- Performance benchmarks
- Continuous improvement
- Confidence in deployments

Regular review and updates of this strategy are essential to maintain testing effectiveness.
