# Testing Suite - Slip Verification System

This directory contains all testing resources for the Slip Verification System.

## 📁 Directory Structure

```
tests/
├── api/                          # API/REST tests
│   ├── auth.http                 # Authentication endpoint tests
│   ├── slips.http                # Slip verification endpoint tests
│   └── orders.http               # Order management endpoint tests
│
├── performance/                  # Performance/Load tests
│   ├── load-test.js             # Normal load testing (k6)
│   ├── stress-test.js           # Stress testing (k6)
│   └── spike-test.js            # Spike testing (k6)
│
├── security/                     # Security tests
│   ├── SECURITY_TESTING_CHECKLIST.md
│   └── run-zap-scan.sh          # OWASP ZAP automation script
│
├── TESTING_STRATEGY.md          # Comprehensive testing guide
└── README.md                    # This file
```

## 🧪 Test Types

### 1. Unit Tests
Located in respective project directories:
- **Backend**: `slip-verification-api/tests/SlipVerification.UnitTests/`
- **Frontend**: `slip-verification-web/src/app/**/*.spec.ts`
- **Mobile**: `slip-verification-mobile/src/__tests__/`
- **OCR Service**: `ocr-service/tests/`

### 2. Integration Tests
- **Backend**: `slip-verification-api/tests/SlipVerification.IntegrationTests/`

### 3. API Tests
- **REST Client**: `tests/api/*.http`
- Use with VS Code REST Client extension or Postman

### 4. Performance Tests
- **k6 Scripts**: `tests/performance/*.js`
- Tests load, stress, and spike scenarios

### 5. Security Tests
- **OWASP ZAP**: Automated security scanning
- **Checklist**: Manual security testing guide

## 🚀 Quick Start

### Running Unit Tests

#### Backend (.NET)
```bash
cd slip-verification-api
dotnet test
```

#### Frontend (Angular)
```bash
cd slip-verification-web
npm test
```

#### Mobile (React Native)
```bash
cd slip-verification-mobile
npm test
```

#### OCR Service (Python)
```bash
cd ocr-service
pytest
```

### Running API Tests

1. **Install REST Client** extension in VS Code
2. Open any `.http` file in `tests/api/`
3. Click "Send Request" above each test
4. Verify responses

Or use Postman:
1. Import the `.http` files
2. Set environment variables
3. Run collection

### Running Performance Tests

#### Prerequisites
```bash
# Install k6
# macOS
brew install k6

# Linux
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 \
  --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb https://dl.k6.io/deb stable main" | \
  sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6

# Windows
choco install k6
```

#### Run Tests
```bash
# Load test
k6 run tests/performance/load-test.js

# Stress test
k6 run tests/performance/stress-test.js

# Spike test
k6 run tests/performance/spike-test.js
```

#### With Custom Configuration
```bash
# Override default settings
k6 run --vus 200 --duration 10m tests/performance/load-test.js

# Set target URL
k6 run -e BASE_URL=https://api.example.com tests/performance/load-test.js
```

### Running Security Tests

#### OWASP ZAP Scan
```bash
cd tests/security
./run-zap-scan.sh
```

Or manually:
```bash
# Baseline scan
docker run -t owasp/zap2docker-stable zap-baseline.py \
  -t http://localhost:5000 \
  -r zap-report.html

# Full scan
docker run -t owasp/zap2docker-stable zap-full-scan.py \
  -t http://localhost:5000 \
  -r zap-full-report.html
```

## 📊 Test Coverage

### Generate Coverage Reports

#### Backend
```bash
cd slip-verification-api
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator \
  -reports:**/coverage.cobertura.xml \
  -targetdir:coverage \
  -reporttypes:"Html"

# Open coverage/index.html in browser
```

#### Frontend
```bash
cd slip-verification-web
npm test -- --code-coverage --watch=false

# Open coverage/index.html in browser
```

### Coverage Targets
- **Backend**: >80%
- **Frontend**: >70%
- **Critical Paths**: 100%

## 🔧 Configuration

### Environment Variables

Create `.env` files for testing:

#### Backend Tests
```env
# slip-verification-api/tests/.env
ConnectionStrings__DefaultConnection=Host=localhost;Database=slip_test;
ConnectionStrings__Redis=localhost:6379
Jwt__Secret=your-test-secret-key
```

#### Performance Tests
```env
# tests/performance/.env
BASE_URL=http://localhost:5000/api/v1
AUTH_TOKEN=your-test-token
```

## 📈 CI/CD Integration

### GitHub Actions

Tests are automatically run on:
- Pull requests (unit + integration)
- Push to main (all tests)
- Scheduled (nightly for performance + security)

### Manual Trigger
```bash
# Trigger test workflow manually
gh workflow run tests.yml
```

## 📝 Writing Tests

### Backend Unit Test Example
```csharp
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccess()
{
    // Arrange
    var command = new VerifySlipCommand
    {
        OrderId = Guid.NewGuid(),
        ImageData = new byte[] { 0x01, 0x02, 0x03 }
    };

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
}
```

### Frontend Unit Test Example
```typescript
it('should upload file successfully', async () => {
  // Arrange
  const mockFile = new File(['content'], 'test.jpg', { type: 'image/jpeg' });
  
  // Act
  component.onFileSelect(mockFile);
  
  // Assert
  expect(component.selectedFile).toBe(mockFile);
});
```

### API Test Example
```http
### Upload Slip
POST {{baseUrl}}/slips/verify HTTP/1.1
Authorization: Bearer {{authToken}}
Content-Type: multipart/form-data

{
  "orderId": "{{orderId}}",
  "file": "@./test-slip.jpg"
}

### Expected Response: 200 OK
```

## 🐛 Debugging Tests

### Backend
```bash
# Run specific test
dotnet test --filter "FullyQualifiedName~VerifySlipCommandHandlerTests"

# Run with verbose output
dotnet test --verbosity detailed
```

### Frontend
```bash
# Run specific test file
npm test -- SlipUploadComponent

# Debug in browser
npm test -- --browsers=Chrome --watch
```

### Performance Tests
```bash
# Enable detailed logging
k6 run --http-debug tests/performance/load-test.js

# Save results to file
k6 run --out json=results.json tests/performance/load-test.js
```

## 📚 Documentation

- [Testing Strategy](./TESTING_STRATEGY.md) - Comprehensive testing guide
- [Security Checklist](./security/SECURITY_TESTING_CHECKLIST.md) - Security testing guide
- [Backend Tests](../slip-verification-api/tests/README.md) - Backend test documentation
- [Frontend Tests](../slip-verification-web/README.md) - Frontend test documentation

## 🤝 Contributing

When adding new tests:

1. Follow existing test structure
2. Write descriptive test names
3. Follow AAA pattern (Arrange-Act-Assert)
4. Update documentation
5. Ensure tests are independent
6. Keep tests fast and focused

### Test Naming Convention
```
MethodName_Scenario_ExpectedResult
```

Examples:
- `Handle_ValidCommand_ReturnsSuccess`
- `PostSlip_WithoutAuth_ReturnsUnauthorized`
- `UploadFile_InvalidType_ReturnsError`

## 🔍 Troubleshooting

### Common Issues

1. **Tests failing locally but passing in CI**
   - Check environment variables
   - Verify dependencies are installed
   - Review test isolation

2. **Slow test execution**
   - Run tests in parallel: `dotnet test --parallel`
   - Use in-memory databases for integration tests
   - Profile test execution times

3. **Flaky tests**
   - Add proper waits/delays
   - Fix race conditions
   - Ensure proper cleanup

4. **Coverage not generating**
   - Install coverage tools: `dotnet tool install --global coverlet.console`
   - Check file paths in configuration
   - Verify test project references

## 📞 Support

For questions or issues:
- Open an issue on GitHub
- Review [Testing Strategy](./TESTING_STRATEGY.md)
- Check existing test examples

## 📄 License

Same as the main project license.
