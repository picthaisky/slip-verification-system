# Test Implementation Guide

## Overview

This document provides comprehensive guidance on implementing and running tests for the Slip Verification System.

## Table of Contents

1. [Backend Tests (.NET)](#backend-tests-net)
2. [Frontend Tests (Angular)](#frontend-tests-angular)
3. [Mobile Tests (React Native)](#mobile-tests-react-native)
4. [API Tests](#api-tests)
5. [E2E Tests](#e2e-tests)
6. [Performance Tests](#performance-tests)
7. [Security Tests](#security-tests)
8. [Test Data Management](#test-data-management)
9. [CI/CD Integration](#cicd-integration)

---

## Backend Tests (.NET)

### Unit Tests

Located in: `slip-verification-api/tests/SlipVerification.UnitTests/`

#### Running Tests

```bash
cd slip-verification-api

# Run all unit tests
dotnet test tests/SlipVerification.UnitTests/

# Run with coverage
dotnet test tests/SlipVerification.UnitTests/ --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~VerifySlipCommandHandlerTests"
```

#### Writing New Tests

```csharp
using Xunit;
using Moq;

public class MyServiceTests
{
    private readonly Mock<IDependency> _dependencyMock;
    private readonly MyService _service;

    public MyServiceTests()
    {
        _dependencyMock = new Mock<IDependency>();
        _service = new MyService(_dependencyMock.Object);
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        _dependencyMock.Setup(x => x.DoSomething()).ReturnsAsync(true);

        // Act
        var result = await _service.DoWork();

        // Assert
        Assert.True(result);
        _dependencyMock.Verify(x => x.DoSomething(), Times.Once);
    }

    [Theory]
    [InlineData(1000, "SCB", true)]
    [InlineData(-100, "KBANK", false)]
    public void Validate_DifferentInputs_ReturnsExpected(
        decimal amount, 
        string bank, 
        bool expected)
    {
        // Arrange & Act
        var result = _service.Validate(amount, bank);

        // Assert
        Assert.Equal(expected, result);
    }
}
```

#### Using Test Data Factory

```csharp
using SlipVerification.UnitTests.Helpers;

// Create test entities easily
var order = TestDataFactory.CreateOrder(amount: 1000m);
var slip = TestDataFactory.CreateSlipVerification(bankName: "SCB");
var user = TestDataFactory.CreateUser(role: "Admin");
```

### Integration Tests

Located in: `slip-verification-api/tests/SlipVerification.IntegrationTests/`

#### Running Tests

```bash
cd slip-verification-api

# Run integration tests
dotnet test tests/SlipVerification.IntegrationTests/

# With test database
docker-compose -f docker-compose.test.yml up -d postgres
dotnet test tests/SlipVerification.IntegrationTests/
docker-compose -f docker-compose.test.yml down
```

#### Writing Integration Tests

```csharp
using SlipVerification.IntegrationTests.Helpers;

public class SlipsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SlipsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostSlip_ValidData_Returns200()
    {
        // Arrange
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("order-id"), "orderId");
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "test.jpg");

        // Act
        var response = await _client.PostAsync("/api/v1/slips/verify", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

---

## Frontend Tests (Angular)

### Component Tests

Located in: `slip-verification-web/src/app/**/*.spec.ts`

#### Running Tests

```bash
cd slip-verification-web

# Run all tests
npm test

# Run with coverage
npm test -- --code-coverage --watch=false

# Run specific test
npm test -- --include='**/slip-upload.spec.ts'
```

#### Writing Component Tests

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MyComponent } from './my-component';

describe('MyComponent', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent],
      providers: [
        { provide: MyService, useValue: mockService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should upload file successfully', async () => {
    const mockFile = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
    
    component.onFileSelect(mockFile);
    
    expect(component.selectedFile).toBe(mockFile);
  });
});
```

### Service Tests

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ApiService]
    });

    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call verify endpoint with correct params', () => {
    service.verifySlip('order-id', mockFile).subscribe();

    const req = httpMock.expectOne('/api/v1/slips/verify');
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });
  });
});
```

---

## Mobile Tests (React Native)

### Running Tests

```bash
cd slip-verification-mobile

# Run all tests
npm test

# Run with coverage
npm test -- --coverage --watchAll=false

# Run specific test
npm test -- auth.test.ts
```

### Writing Tests

```typescript
import { render, fireEvent } from '@testing-library/react-native';
import SlipUploadScreen from '../screens/SlipUploadScreen';

describe('SlipUploadScreen', () => {
  it('should render correctly', () => {
    const { getByText } = render(<SlipUploadScreen />);
    expect(getByText('Upload Slip')).toBeTruthy();
  });

  it('should handle file selection', () => {
    const { getByTestId } = render(<SlipUploadScreen />);
    const fileInput = getByTestId('file-input');
    
    fireEvent.press(fileInput);
    
    // Assert file picker is opened
  });
});
```

---

## API Tests

### REST Client Tests (.http files)

Located in: `tests/api/*.http`

#### Running with VS Code REST Client

1. Install REST Client extension
2. Open `.http` file
3. Click "Send Request" above each test

### Postman/Newman Tests

Located in: `tests/api/postman-collection.json`

#### Running Newman Tests

```bash
# Install Newman
npm install -g newman newman-reporter-htmlextra

# Run collection
cd tests/api
bash run-newman-tests.sh

# Or manually
newman run postman-collection.json \
  --environment postman-environment.json \
  --reporters cli,html \
  --reporter-html-export results.html
```

#### Writing Postman Tests

```javascript
// In Postman test script
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response has correct structure", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('data');
    pm.expect(jsonData.data).to.have.property('id');
});

pm.test("Response time is less than 500ms", function () {
    pm.expect(pm.response.responseTime).to.be.below(500);
});
```

---

## E2E Tests

### Playwright Tests

Located in: `tests/e2e/*.spec.ts`

#### Setup

```bash
# Install Playwright
npm install -D @playwright/test
npx playwright install

# Install browsers
npx playwright install chromium firefox webkit
```

#### Running E2E Tests

```bash
# Run all E2E tests
npx playwright test

# Run in specific browser
npx playwright test --project=chromium

# Run with UI
npx playwright test --ui

# Debug mode
npx playwright test --debug

# Generate report
npx playwright show-report
```

#### Writing E2E Tests

See `tests/e2e/slip-upload.e2e.spec.ts` for complete examples.

---

## Performance Tests

### k6 Load Tests

Located in: `tests/performance/*.js`

#### Running Performance Tests

```bash
# Install k6
brew install k6  # macOS
choco install k6  # Windows
# See README.md for Linux

# Run load test
k6 run tests/performance/load-test.js

# With custom settings
k6 run --vus 100 --duration 5m tests/performance/load-test.js

# With custom base URL
k6 run -e BASE_URL=https://api.example.com tests/performance/load-test.js
```

#### Writing Performance Tests

```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '2m', target: 100 },
    { duration: '5m', target: 100 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const res = http.get('http://localhost:5000/api/v1/slips');
  
  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
  
  sleep(1);
}
```

---

## Security Tests

### OWASP ZAP Scans

Located in: `tests/security/`

#### Running Security Tests

```bash
cd tests/security

# Baseline scan
./run-zap-scan.sh baseline

# Full scan
./run-zap-scan.sh full

# API scan
./run-zap-scan.sh api
```

#### Manual Security Testing

See `SECURITY_TESTING_CHECKLIST.md` for comprehensive checklist.

---

## Test Data Management

### Using Test Data Factory

```csharp
// Backend (.NET)
using SlipVerification.UnitTests.Helpers;

var order = TestDataFactory.CreateOrder(
    amount: 1000m,
    status: OrderStatus.PendingPayment
);

var slip = TestDataFactory.CreateSlipVerification(
    orderId: order.Id,
    bankName: "Kasikorn Bank"
);
```

### Test Fixtures

```csharp
// Shared fixture across tests
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        Context = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}

// Use in tests
public class MyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

---

## CI/CD Integration

### GitHub Actions

Tests run automatically on:
- Pull requests
- Push to main branch
- Scheduled (nightly)

See `.github/workflows/comprehensive-tests.yml` for configuration.

### Running CI Tests Locally

```bash
# Backend
dotnet test

# Frontend
cd slip-verification-web && npm test -- --watch=false --code-coverage

# Mobile
cd slip-verification-mobile && npm test -- --coverage --watchAll=false

# API
cd tests/api && bash run-newman-tests.sh

# Performance (optional)
k6 run tests/performance/load-test.js

# Security (requires Docker)
cd tests/security && ./run-zap-scan.sh baseline
```

---

## Best Practices

### Test Naming

Use the pattern: `MethodName_Scenario_ExpectedResult`

Examples:
- `Handle_ValidCommand_ReturnsSuccess`
- `PostSlip_WithoutAuth_ReturnsUnauthorized`
- `UploadFile_InvalidType_ReturnsError`

### Test Organization

```
tests/
├── UnitTests/           # Fast, isolated tests
├── IntegrationTests/    # Tests with dependencies
├── FunctionalTests/     # End-to-end functional tests
├── api/                 # API contract tests
├── e2e/                 # UI E2E tests
├── performance/         # Load and performance tests
└── security/            # Security tests
```

### Test Independence

- Each test should be independent
- Use `beforeEach` to set up clean state
- Don't rely on test execution order
- Clean up after tests

### Mocking

```csharp
// Mock external dependencies
var httpClientMock = new Mock<IHttpClient>();
httpClientMock
    .Setup(x => x.GetAsync(It.IsAny<string>()))
    .ReturnsAsync(mockResponse);
```

### Assertions

```csharp
// Be specific with assertions
Assert.Equal(expected, actual);  // Good
Assert.True(result);             // Less informative

// Multiple assertions
Assert.True(result.IsSuccess);
Assert.NotNull(result.Data);
Assert.Equal(expectedId, result.Data.Id);
```

---

## Troubleshooting

### Common Issues

1. **Tests fail in CI but pass locally**
   - Check environment variables
   - Verify dependencies are installed
   - Review test isolation

2. **Slow test execution**
   - Run tests in parallel
   - Use in-memory databases for integration tests
   - Profile test execution times

3. **Flaky tests**
   - Add proper waits/delays
   - Fix race conditions
   - Ensure proper cleanup

4. **Coverage not generating**
   - Install coverage tools
   - Check file paths in configuration
   - Verify test project references

---

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Angular Testing Guide](https://angular.io/guide/testing)
- [Jest Documentation](https://jestjs.io/)
- [Playwright Documentation](https://playwright.dev/)
- [k6 Documentation](https://k6.io/docs/)
- [Newman Documentation](https://learning.postman.com/docs/running-collections/using-newman-cli/command-line-integration-with-newman/)

---

## Support

For questions or issues:
- Review existing test examples
- Check [TESTING_STRATEGY.md](./TESTING_STRATEGY.md)
- Open an issue on GitHub
