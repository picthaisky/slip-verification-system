# Performance Optimization Quick Start

This guide helps you quickly implement and verify performance optimizations in the Slip Verification System.

## 🚀 Quick Setup (5 minutes)

### 1. Verify Configuration

Check that performance settings are enabled in `appsettings.json`:

```bash
cd slip-verification-api/src/SlipVerification.API
cat appsettings.json | grep -A 2 "ConnectionStrings"
```

Expected output should show connection pooling:
```json
"DefaultConnection": "...;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;..."
```

### 2. Start Redis (Required for Caching)

```bash
# Using Docker
docker run -d -p 6379:6379 --name redis redis:7-alpine

# Or using docker-compose
docker-compose up -d redis
```

### 3. Build and Run

```bash
cd slip-verification-api
dotnet run --project src/SlipVerification.API
```

Watch for cache warmup logs:
```
[INFO] Starting cache warmup...
[INFO] Cache warmup completed successfully. Cached 42 pending orders
```

## ✅ Verify Performance Features

### 1. Check Response Compression

```bash
# Request with compression
curl -H "Accept-Encoding: gzip, br" -I http://localhost:5000/api/v1/orders

# Look for this header:
# Content-Encoding: br  (or gzip)
```

### 2. Verify Cache Hit

```bash
# First request (cache miss)
time curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/v1/orders

# Second request (cache hit - should be faster)
time curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/v1/orders
```

### 3. Check Database Connection Pool

```bash
# In PostgreSQL
psql -U postgres -d SlipVerificationDb -c "SELECT count(*) FROM pg_stat_activity WHERE datname = 'SlipVerificationDb';"
```

### 4. View Prometheus Metrics

```bash
curl http://localhost:5000/metrics | grep http_request_duration
```

## 📊 Run Load Test

### Install k6

```bash
# macOS
brew install k6

# Linux
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6

# Windows
choco install k6
```

### Create Test User

```bash
curl -X POST http://localhost:5000/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "loadtest@example.com",
    "password": "Test@123",
    "firstName": "Load",
    "lastName": "Test"
  }'
```

### Run Quick Test (5 minutes)

```bash
cd tests/load-testing
k6 run --duration 5m --vus 50 load-test.js
```

### Expected Results

```
✓ login successful
✓ orders retrieved
✓ order created

checks.........................: 99.98% ✓ 12458    ✗ 2
http_req_duration..............: avg=145ms  min=12ms  med=132ms  max=890ms  p(90)=178ms  p(95)=195ms
http_req_failed................: 0.01%  ✓ 2        ✗ 12456
success_rate...................: 99.99% ✓ 12458    ✗ 2
```

## 🎯 Performance Checklist

Quick verification checklist:

- [ ] Redis is running (`docker ps | grep redis`)
- [ ] API starts without errors
- [ ] Cache warmup logs appear on startup
- [ ] Response compression headers present
- [ ] Prometheus metrics endpoint accessible (`/metrics`)
- [ ] Load test passes with <200ms p95

## 🔍 Troubleshooting

### Issue: Cache Not Working

**Solution:**
```bash
# Check Redis connection
redis-cli ping
# Should return: PONG

# Check API logs for Redis errors
docker logs slip-verification-api | grep -i redis
```

### Issue: Slow Queries

**Solution:**
```bash
# Enable query logging in PostgreSQL
psql -U postgres -d SlipVerificationDb -c "ALTER SYSTEM SET log_min_duration_statement = 100;"
psql -U postgres -d SlipVerificationDb -c "SELECT pg_reload_conf();"

# Check slow queries
tail -f /var/lib/postgresql/data/pg_log/postgresql-*.log | grep "duration"
```

### Issue: High Memory Usage

**Solution:**
```bash
# Check DbContext configuration
grep -A 5 "UseQueryTrackingBehavior" src/SlipVerification.API/Program.cs

# Should see: QueryTrackingBehavior.NoTracking
```

## 📈 Monitor Performance

### View Grafana Dashboard

```bash
# Start monitoring stack
docker-compose -f docker-compose.monitoring.yml up -d

# Open Grafana
open http://localhost:3000
# Default: admin/admin
```

### Key Metrics to Watch

1. **API Response Time (p95)**: Should be < 200ms
2. **Cache Hit Ratio**: Should be > 80%
3. **Database Connections**: Should stay within pool limits (5-100)
4. **Error Rate**: Should be < 0.1%

### Prometheus Queries

```promql
# Response time p95
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Cache hit ratio
rate(cache_hits_total[5m]) / (rate(cache_hits_total[5m]) + rate(cache_misses_total[5m]))

# Error rate
rate(errors_total[5m]) / rate(http_request_duration_seconds_count[5m])
```

## 🎓 Next Steps

1. **Read Full Guide**: See [PERFORMANCE_OPTIMIZATION.md](PERFORMANCE_OPTIMIZATION.md)
2. **Run Full Load Test**: See [Load Testing Guide](../tests/load-testing/README.md)
3. **Tune Settings**: Adjust connection pool and cache TTLs based on your workload
4. **Add Custom Metrics**: Implement business-specific performance tracking
5. **Set Up Alerts**: Configure alerts for performance degradation

## 📚 Quick Reference

### Connection Pool Settings

| Setting | Default | Recommended |
|---------|---------|-------------|
| Min Pool Size | 5 | 5-10 |
| Max Pool Size | 100 | 50-200 |
| Connection Lifetime | 300s | 300-600s |

### Cache TTL Settings

| Data Type | TTL | When to Use |
|-----------|-----|-------------|
| User Profile | 5 min | Rarely changes |
| Order Status | 1 min | Frequent updates |
| Static Data | 1 hour | Config/lookup data |

### Performance Targets

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| API Response (p95) | <200ms | <500ms |
| Database Query (p95) | <50ms | <100ms |
| Cache Hit Ratio | >80% | >70% |
| Error Rate | <0.1% | <1% |

## 💡 Pro Tips

1. **Use projections**: Always use `.Select()` for list views
2. **Disable tracking**: Use `.AsNoTracking()` for read-only queries
3. **Batch operations**: Use `ProcessSlipsBatchAsync` for bulk processing
4. **Cache strategically**: Cache frequently read, rarely updated data
5. **Monitor always**: Keep an eye on Grafana dashboards

## 🆘 Need Help?

- Documentation: [PERFORMANCE_OPTIMIZATION.md](PERFORMANCE_OPTIMIZATION.md)
- Load Testing: [tests/load-testing/README.md](../tests/load-testing/README.md)
- Monitoring: [MONITORING_GUIDE.md](../infrastructure/monitoring/MONITORING_GUIDE.md)
- Issues: Check GitHub issues or create a new one

---

**Time to Performance**: ~5 minutes with this guide! 🚀
