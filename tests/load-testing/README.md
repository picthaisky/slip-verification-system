# Load Testing Guide

This directory contains k6 load testing scripts for the Slip Verification System.

## Prerequisites

1. Install k6: https://k6.io/docs/getting-started/installation/
2. Ensure the API is running
3. Create a test user account

## Running Load Tests

### Basic Load Test

```bash
k6 run load-test.js
```

### Custom Configuration

```bash
# Custom base URL
k6 run --env BASE_URL=http://your-api-url:5000 load-test.js

# Custom test user
k6 run --env TEST_USER_EMAIL=test@example.com --env TEST_USER_PASSWORD=YourPassword load-test.js

# Run with HTML report
k6 run --out json=test-results.json load-test.js
```

### Quick Smoke Test (5 minutes)

```bash
k6 run --duration 5m --vus 50 load-test.js
```

### Stress Test (1000 users)

```bash
k6 run --stage "5m:1000,10m:1000,2m:0" load-test.js
```

## Performance Thresholds

The load test enforces the following performance thresholds:

- **API Response Time (p95)**: < 200ms
- **API Response Time (p99)**: < 500ms
- **Error Rate**: < 0.1%
- **Success Rate**: > 99.9%

## Test Scenarios

The load test simulates realistic user behavior:

1. **Authentication**: User login
2. **List Orders**: Fetch orders list
3. **Create Order**: Create a new order
4. **View Details**: Get order details
5. **List Slips**: Fetch slip verifications
6. **Profile**: Get user profile

## Load Test Stages

| Stage | Duration | Users | Description |
|-------|----------|-------|-------------|
| 1 | 2 min | 0 → 100 | Ramp up to 100 users |
| 2 | 5 min | 100 | Stay at 100 users |
| 3 | 2 min | 100 → 200 | Ramp up to 200 users |
| 4 | 5 min | 200 | Stay at 200 users |
| 5 | 2 min | 200 → 500 | Ramp up to 500 users |
| 6 | 5 min | 500 | Stay at 500 users |
| 7 | 3 min | 500 → 1000 | Ramp up to 1000 users |
| 8 | 10 min | 1000 | Stay at 1000 users |
| 9 | 2 min | 1000 → 0 | Ramp down |

Total duration: ~36 minutes

## Metrics

The test collects the following custom metrics:

- `order_creation_errors`: Count of failed order creations
- `slip_upload_errors`: Count of failed slip uploads
- `auth_failures`: Count of authentication failures
- `success_rate`: Overall success rate
- `order_creation_duration`: Time to create orders
- `slip_upload_duration`: Time to upload slips

## Analyzing Results

### Console Output

k6 provides real-time metrics in the console:

```
✓ login successful
✓ orders retrieved
✓ order created
...

checks.........................: 99.98% ✓ 45832    ✗ 9
data_received..................: 125 MB 3.5 MB/s
data_sent......................: 42 MB  1.2 MB/s
http_req_duration..............: avg=145ms  min=12ms  med=132ms  max=1.2s   p(90)=178ms  p(95)=195ms
http_req_failed................: 0.09%  ✓ 45       ✗ 51287
http_reqs......................: 51332  1427/s
```

### HTML Report

Generate an HTML report:

```bash
# Generate JSON output
k6 run --out json=results.json load-test.js

# Convert to HTML (requires k6-reporter)
k6-reporter results.json
```

### Grafana Dashboard

For continuous monitoring:

1. Run k6 with InfluxDB output:
```bash
k6 run --out influxdb=http://localhost:8086/k6 load-test.js
```

2. View results in Grafana dashboard

## Troubleshooting

### High Error Rate

- Check API logs for errors
- Verify database connection pool size
- Check Redis connectivity
- Monitor server resources (CPU, memory)

### Slow Response Times

- Review database query performance
- Check cache hit ratio
- Monitor network latency
- Verify connection pooling settings

### Connection Timeouts

- Increase connection timeout in k6
- Check server max connections
- Verify load balancer configuration
- Review rate limiting settings

## Best Practices

1. **Start Small**: Begin with low user counts and gradually increase
2. **Monitor Resources**: Watch CPU, memory, and database metrics during tests
3. **Realistic Data**: Use production-like test data
4. **Clean Up**: Remove test data after load testing
5. **Schedule Tests**: Run during off-peak hours
6. **Baseline**: Establish baseline metrics before optimization

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `BASE_URL` | `http://localhost:5000` | API base URL |
| `TEST_USER_EMAIL` | `loadtest@example.com` | Test user email |
| `TEST_USER_PASSWORD` | `Test@123` | Test user password |

## Example: Create Test User

```bash
# Using the API
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "loadtest@example.com",
    "password": "Test@123",
    "firstName": "Load",
    "lastName": "Test"
  }'
```

## CI/CD Integration

Add to your CI pipeline:

```yaml
# GitHub Actions example
- name: Run Load Test
  run: |
    k6 run --out json=results.json tests/load-testing/load-test.js
    
- name: Upload Results
  uses: actions/upload-artifact@v3
  with:
    name: load-test-results
    path: results.json
```

## References

- [k6 Documentation](https://k6.io/docs/)
- [k6 Best Practices](https://k6.io/docs/testing-guides/test-types/)
- [Performance Testing Guide](https://k6.io/docs/testing-guides/api-load-testing/)
