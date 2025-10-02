# Test Coverage Configuration Guide

This document outlines how to generate and maintain test coverage for the Slip Verification System.

## Backend Coverage (.NET Core)

### Running Tests with Coverage

```bash
cd slip-verification-api

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Run specific test project with coverage
dotnet test tests/SlipVerification.UnitTests/*.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults
```

### Generate HTML Coverage Report

#### Install ReportGenerator

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
```

#### Generate Report

```bash
cd slip-verification-api

# Generate HTML report from coverage data
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:"Html;Badges;Cobertura"

# Open the report
open TestResults/CoverageReport/index.html
```

### Coverage Configuration

Add to `Directory.Build.props` in the API root:

```xml
<Project>
  <PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>cobertura,opencover</CoverletOutputFormat>
    <CoverletOutput>./TestResults/</CoverletOutput>
    <Exclude>[*.Tests]*,[*.UnitTests]*</Exclude>
    <ExcludeByFile>**/Migrations/*.cs</ExcludeByFile>
    <Include>[SlipVerification.*]*</Include>
  </PropertyGroup>
</Project>
```

### Coverage Thresholds

Add to test project `.csproj`:

```xml
<PropertyGroup>
  <Threshold>80</Threshold>
  <ThresholdType>line,branch</ThresholdType>
  <ThresholdStat>total</ThresholdStat>
</PropertyGroup>
```

## Frontend Coverage (Angular)

### Running Tests with Coverage

```bash
cd slip-verification-web

# Run tests with coverage
npm test -- --code-coverage --watch=false

# Run tests with coverage and specific threshold
npm test -- --code-coverage --watch=false --codeCoverageExclude=**/*.spec.ts
```

### Coverage Configuration

Update `angular.json`:

```json
{
  "projects": {
    "slip-verification-web": {
      "architect": {
        "test": {
          "options": {
            "codeCoverage": true,
            "codeCoverageExclude": [
              "**/*.spec.ts",
              "**/*.mock.ts",
              "**/test/**"
            ]
          }
        }
      }
    }
  }
}
```

### Karma Configuration

Update `karma.conf.js`:

```javascript
module.exports = function(config) {
  config.set({
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage'),
      subdir: '.',
      reporters: [
        { type: 'html' },
        { type: 'text-summary' },
        { type: 'lcovonly' }
      ],
      check: {
        global: {
          statements: 70,
          branches: 70,
          functions: 70,
          lines: 70
        }
      }
    }
  });
};
```

### View Coverage Report

```bash
cd slip-verification-web
open coverage/index.html
```

## Mobile Coverage (React Native)

### Running Tests with Coverage

```bash
cd slip-verification-mobile

# Run tests with coverage
npm test -- --coverage --watchAll=false

# Generate coverage report
npm test -- --coverage --coverageReporters=html --coverageReporters=text
```

### Jest Configuration

Update `jest.config.js`:

```javascript
module.exports = {
  collectCoverage: true,
  collectCoverageFrom: [
    'src/**/*.{ts,tsx}',
    '!src/**/*.test.{ts,tsx}',
    '!src/**/*.spec.{ts,tsx}',
    '!src/**/__tests__/**',
    '!src/**/node_modules/**',
    '!src/**/*.mock.{ts,tsx}'
  ],
  coverageThreshold: {
    global: {
      branches: 70,
      functions: 70,
      lines: 70,
      statements: 70
    }
  },
  coverageReporters: ['html', 'text', 'lcov', 'json']
};
```

### View Coverage Report

```bash
cd slip-verification-mobile
open coverage/lcov-report/index.html
```

## Python Coverage (OCR Service)

### Running Tests with Coverage

```bash
cd ocr-service

# Install coverage tool
pip install pytest-cov

# Run tests with coverage
pytest --cov=app --cov-report=html --cov-report=term

# Run with specific threshold
pytest --cov=app --cov-report=html --cov-fail-under=80
```

### Coverage Configuration

Create `.coveragerc`:

```ini
[run]
source = app
omit = 
    */tests/*
    */test_*.py
    */__pycache__/*
    */venv/*

[report]
exclude_lines =
    pragma: no cover
    def __repr__
    raise AssertionError
    raise NotImplementedError
    if __name__ == .__main__.:
    if TYPE_CHECKING:
    @abstractmethod

[html]
directory = htmlcov
```

### View Coverage Report

```bash
cd ocr-service
open htmlcov/index.html
```

## CI/CD Coverage Integration

### Codecov Integration

1. **Add Codecov to GitHub Actions:**

```yaml
- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ./coverage.xml
    flags: unittests
    name: codecov-umbrella
    fail_ci_if_error: true
```

2. **Create `codecov.yml`:**

```yaml
coverage:
  status:
    project:
      default:
        target: 80%
        threshold: 1%
    patch:
      default:
        target: 80%

ignore:
  - "**/tests/**"
  - "**/*.test.ts"
  - "**/*.spec.ts"
  - "**/Migrations/**"

comment:
  layout: "reach,diff,flags,tree"
  behavior: default
```

### SonarQube Integration

1. **Install SonarScanner:**

```bash
dotnet tool install --global dotnet-sonarscanner
```

2. **Run with Coverage:**

```bash
# Start analysis
dotnet sonarscanner begin \
  /k:"slip-verification-system" \
  /d:sonar.host.url="http://sonarqube:9000" \
  /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml"

# Build and test
dotnet build
dotnet test --collect:"XPlat Code Coverage"

# End analysis
dotnet sonarscanner end
```

## Coverage Targets

### Minimum Requirements

| Component | Target | Critical Paths |
|-----------|--------|----------------|
| Backend API | 80% | 100% |
| Frontend | 70% | 100% |
| Mobile | 70% | 100% |
| OCR Service | 75% | 100% |

### Critical Paths

Critical paths requiring 100% coverage:
- Authentication flows
- Payment slip verification
- Order processing
- File upload and storage
- Security validations

## Monitoring Coverage Trends

### GitHub Actions Badge

Add to README.md:

```markdown
[![codecov](https://codecov.io/gh/username/slip-verification-system/branch/main/graph/badge.svg)](https://codecov.io/gh/username/slip-verification-system)
```

### Coverage Reports in PR

Coverage reports automatically posted to PRs showing:
- Overall coverage change
- File-level coverage changes
- Uncovered lines

## Best Practices

1. **Run coverage locally before committing:**
   ```bash
   # Backend
   cd slip-verification-api && dotnet test --collect:"XPlat Code Coverage"
   
   # Frontend
   cd slip-verification-web && npm test -- --code-coverage --watch=false
   ```

2. **Review coverage reports regularly:**
   - Identify untested code paths
   - Add tests for edge cases
   - Maintain or improve coverage percentage

3. **Don't sacrifice quality for coverage:**
   - 100% coverage doesn't mean bug-free
   - Focus on meaningful tests
   - Test behavior, not implementation

4. **Exclude appropriate files:**
   - Generated code
   - Database migrations
   - Test utilities
   - Mock data

5. **Set realistic thresholds:**
   - Increase gradually
   - Focus on critical components first
   - Allow lower coverage for UI components

## Troubleshooting

### Backend Coverage Not Generating

```bash
# Install coverlet
dotnet add package coverlet.msbuild

# Run with explicit format
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Frontend Coverage Incomplete

```bash
# Clear cache and rebuild
cd slip-verification-web
rm -rf coverage/ .angular/
ng test --code-coverage --watch=false
```

### Python Coverage Missing Branches

```bash
# Run with branch coverage
pytest --cov=app --cov-branch --cov-report=html
```

## Resources

- [Codecov Documentation](https://docs.codecov.com/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [Istanbul (Jest) Coverage](https://istanbul.js.org/)
- [pytest-cov Documentation](https://pytest-cov.readthedocs.io/)
