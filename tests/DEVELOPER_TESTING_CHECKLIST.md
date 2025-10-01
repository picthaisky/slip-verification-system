# Developer Testing Checklist

## Pre-Development

Before starting any new feature or bug fix:

- [ ] Review existing tests for similar functionality
- [ ] Understand test coverage requirements
- [ ] Plan test cases for the feature
- [ ] Set up local test environment

## During Development

### Writing Tests

- [ ] Write failing tests first (TDD approach)
- [ ] Create unit tests for new business logic
- [ ] Add integration tests for API endpoints
- [ ] Include error handling test cases
- [ ] Test edge cases and boundary conditions
- [ ] Mock external dependencies
- [ ] Use descriptive test names

### Test Quality

- [ ] Follow AAA pattern (Arrange-Act-Assert)
- [ ] Keep tests independent
- [ ] Ensure tests are fast (< 100ms for unit tests)
- [ ] Avoid hard-coded values (use constants or test data)
- [ ] Clean up resources after tests
- [ ] Use appropriate assertions

### Code Coverage

- [ ] Run coverage report locally
- [ ] Ensure new code has >80% coverage (backend)
- [ ] Ensure new code has >70% coverage (frontend)
- [ ] Cover critical paths 100%

## Backend Testing (.NET)

### Unit Tests

- [ ] Test all public methods
- [ ] Test service methods with mocked dependencies
- [ ] Test command/query handlers
- [ ] Test domain entity validation
- [ ] Test business logic edge cases

**Command**:
```bash
cd slip-verification-api
dotnet test tests/SlipVerification.UnitTests/
```

### Integration Tests

- [ ] Test API endpoints
- [ ] Test database operations
- [ ] Test authentication/authorization
- [ ] Test file operations
- [ ] Test external service integrations

**Command**:
```bash
dotnet test tests/SlipVerification.IntegrationTests/
```

### Coverage Report

```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
```

## Frontend Testing (Angular)

### Service Tests

- [ ] Test all service methods
- [ ] Test HTTP requests (GET, POST, PUT, DELETE)
- [ ] Test error handling
- [ ] Test data transformations
- [ ] Mock HTTP responses

**Example**:
```typescript
it('should fetch data successfully', () => {
  service.getData().subscribe(data => {
    expect(data).toBeDefined();
  });
  
  const req = httpMock.expectOne('/api/data');
  req.flush(mockData);
});
```

### Component Tests

- [ ] Test component initialization
- [ ] Test user interactions
- [ ] Test form validation
- [ ] Test data binding
- [ ] Test event handlers
- [ ] Mock dependencies

**Example**:
```typescript
it('should submit form on valid input', () => {
  component.form.setValue({ field: 'value' });
  component.onSubmit();
  expect(component.submitted).toBe(true);
});
```

### Guard Tests

- [ ] Test authentication checks
- [ ] Test authorization (role-based)
- [ ] Test redirect logic

### Run Tests

```bash
cd slip-verification-web
npm test
npm test -- --code-coverage
```

## API Testing

### REST Client Tests

- [ ] Test authentication endpoints
- [ ] Test CRUD operations
- [ ] Test error responses (400, 401, 404, 500)
- [ ] Test pagination
- [ ] Test filtering and sorting
- [ ] Test file uploads

**Files**: `tests/api/*.http`

### Verification Steps

- [ ] Verify response status codes
- [ ] Verify response body structure
- [ ] Verify response headers
- [ ] Verify error messages
- [ ] Test with valid and invalid data

## Performance Testing

### Before Running

- [ ] Ensure target system is running
- [ ] Set appropriate VUs (virtual users)
- [ ] Set appropriate duration
- [ ] Define performance thresholds

### k6 Tests

- [ ] Run load test (normal load)
- [ ] Run stress test (heavy load)
- [ ] Run spike test (sudden traffic)
- [ ] Analyze results
- [ ] Document performance issues

**Commands**:
```bash
k6 run tests/performance/load-test.js
k6 run tests/performance/stress-test.js
k6 run tests/performance/spike-test.js
```

### Performance Metrics to Check

- [ ] Response time (p95, p99)
- [ ] Throughput (requests/second)
- [ ] Error rate
- [ ] Resource utilization

## Security Testing

### Manual Checklist

- [ ] Review `SECURITY_TESTING_CHECKLIST.md`
- [ ] Test authentication
- [ ] Test authorization
- [ ] Test input validation
- [ ] Test for SQL injection
- [ ] Test for XSS
- [ ] Test file upload security
- [ ] Test sensitive data exposure

### Automated Scan

- [ ] Run OWASP ZAP scan
- [ ] Review scan results
- [ ] Address high/critical findings
- [ ] Document false positives

**Command**:
```bash
cd tests/security
./run-zap-scan.sh
```

## Pre-Commit

Before committing code:

- [ ] Run all affected tests locally
- [ ] Ensure all tests pass
- [ ] Check code coverage
- [ ] Run linter/formatter
- [ ] Review test output for warnings
- [ ] Update test documentation if needed

**Quick Test Commands**:
```bash
# Backend
cd slip-verification-api && dotnet test

# Frontend
cd slip-verification-web && npm test

# All
make test  # if Makefile exists
```

## Pre-Pull Request

Before creating a pull request:

- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] Code coverage meets requirements
- [ ] No test warnings or errors
- [ ] Tests added for new features
- [ ] Tests updated for bug fixes
- [ ] Documentation updated
- [ ] CI/CD pipeline passing

## Post-Merge

After merging to main:

- [ ] Verify CI/CD tests pass
- [ ] Check coverage reports
- [ ] Monitor for flaky tests
- [ ] Update test documentation
- [ ] Schedule performance tests (if needed)

## Continuous Monitoring

### Daily

- [ ] Check CI/CD test results
- [ ] Monitor test execution time
- [ ] Fix failing tests immediately

### Weekly

- [ ] Review test coverage trends
- [ ] Identify flaky tests
- [ ] Update test data/fixtures
- [ ] Review test documentation

### Monthly

- [ ] Run full security scan
- [ ] Run performance tests
- [ ] Review and update test strategy
- [ ] Clean up obsolete tests
- [ ] Refactor test code

## Test Troubleshooting

### Common Issues

#### Flaky Tests
- [ ] Check for timing issues
- [ ] Review test dependencies
- [ ] Add appropriate waits/delays
- [ ] Check test isolation

#### Slow Tests
- [ ] Profile test execution
- [ ] Reduce database calls
- [ ] Use in-memory databases
- [ ] Optimize setup/teardown

#### Coverage Gaps
- [ ] Identify uncovered code
- [ ] Add targeted tests
- [ ] Review critical paths
- [ ] Update coverage thresholds

## Test Data Management

### Test Fixtures

- [ ] Use consistent test data
- [ ] Store fixtures in appropriate location
- [ ] Document test data requirements
- [ ] Clean up test data after tests

### Database Seeding

- [ ] Create test data seeder
- [ ] Use factories for object creation
- [ ] Reset database between tests
- [ ] Use transactions for cleanup

## Documentation

### Required Documentation

- [ ] Test purpose and scope
- [ ] Setup instructions
- [ ] Expected behavior
- [ ] Test data requirements
- [ ] Known limitations

### Update When

- [ ] Adding new tests
- [ ] Modifying existing tests
- [ ] Changing test infrastructure
- [ ] Discovering issues

## Best Practices

### DO

✅ Write tests before/during development (TDD)
✅ Keep tests simple and focused
✅ Use descriptive test names
✅ Mock external dependencies
✅ Test error scenarios
✅ Maintain test code quality
✅ Run tests frequently
✅ Fix failing tests immediately

### DON'T

❌ Skip writing tests
❌ Write tests that depend on each other
❌ Use hard-coded values
❌ Ignore failing tests
❌ Write tests without assertions
❌ Test implementation details
❌ Commit failing tests
❌ Skip code reviews for tests

## Resources

### Documentation

- [Testing Strategy](./TESTING_STRATEGY.md)
- [Test Implementation Summary](./TEST_IMPLEMENTATION_SUMMARY.md)
- [Security Testing Checklist](./security/SECURITY_TESTING_CHECKLIST.md)
- [Tests README](./README.md)

### Tools

- xUnit: https://xunit.net/
- Moq: https://github.com/moq/moq4
- Jasmine: https://jasmine.github.io/
- Karma: https://karma-runner.github.io/
- k6: https://k6.io/docs/
- OWASP ZAP: https://www.zaproxy.org/

### Support

- Team chat: #dev-testing
- Code reviews: Ask questions in PR
- Documentation: See links above

## Quick Reference

### Running Tests

| Platform | Command | Coverage |
|----------|---------|----------|
| Backend | `dotnet test` | `dotnet test --collect:"XPlat Code Coverage"` |
| Frontend | `npm test` | `npm test -- --code-coverage` |
| Mobile | `npm test` | `npm test -- --coverage` |
| OCR | `pytest` | `pytest --cov=app` |
| API | Open `.http` files in VS Code | N/A |
| Performance | `k6 run tests/performance/load-test.js` | N/A |
| Security | `./tests/security/run-zap-scan.sh` | N/A |

### Test Naming Convention

```
MethodName_Scenario_ExpectedResult

Examples:
- Handle_ValidCommand_ReturnsSuccess
- Login_InvalidCredentials_ReturnsUnauthorized
- UploadFile_NoFile_ReturnsBadRequest
```

### Assertion Examples

**Backend (xUnit)**:
```csharp
Assert.True(result.IsSuccess);
Assert.Equal(expected, actual);
Assert.NotNull(result.Data);
Assert.Throws<Exception>(() => method());
```

**Frontend (Jasmine)**:
```typescript
expect(result).toBe(true);
expect(data).toEqual(mockData);
expect(service).toBeDefined();
expect(() => method()).toThrow();
```

---

**Remember**: Good tests are as important as good code! 🧪✅
