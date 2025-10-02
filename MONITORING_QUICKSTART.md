# Complete Monitoring & Observability Stack - Quick Start

This guide provides a quick start for deploying and using the complete monitoring and observability stack for the Slip Verification System.

## Stack Overview

- **Prometheus**: Metrics collection and storage
- **Grafana**: Visualization and dashboards
- **Elasticsearch + Fluentd + Kibana (EFK)**: Centralized logging
- **Jaeger**: Distributed tracing
- **AlertManager**: Alert routing and notification

## Quick Start

### Prerequisites

- Docker and Docker Compose installed
- At least 8GB RAM available
- Ports available: 3000, 5000, 5601, 9090, 9093, 9200, 16686, 24224

### 1. Start Monitoring Stack

```bash
# Start Prometheus, Grafana, AlertManager, and exporters
docker-compose -f docker-compose.monitoring.yml up -d
```

**Services started:**
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (admin/admin)
- AlertManager: http://localhost:9093
- Node Exporter: http://localhost:9100/metrics
- PostgreSQL Exporter: http://localhost:9187/metrics
- Redis Exporter: http://localhost:9121/metrics

### 2. Start Logging Stack

```bash
# Start Elasticsearch, Kibana, and Fluentd
docker-compose -f docker-compose.logging.yml up -d
```

**Services started:**
- Elasticsearch: http://localhost:9200
- Kibana: http://localhost:5601
- Fluentd: localhost:24224

### 3. Start Tracing Stack

```bash
# Start Jaeger
docker-compose -f docker-compose.tracing.yml up -d
```

**Services started:**
- Jaeger UI: http://localhost:16686
- Jaeger Collector: http://localhost:14268

### 4. Start Application

```bash
# Start the complete application stack
docker-compose -f docker-compose.dev.yml up -d
```

**Services started:**
- API: http://localhost:5000
- API Metrics: http://localhost:5000/metrics
- API Health: http://localhost:5000/health

### 5. Verify Everything is Running

```bash
# Check all containers
docker ps

# Check Prometheus targets
curl http://localhost:9090/api/v1/targets | jq '.data.activeTargets[] | {job: .labels.job, health: .health}'

# Check API metrics
curl http://localhost:5000/metrics

# Check Elasticsearch
curl http://localhost:9200/_cluster/health

# Check Jaeger
curl http://localhost:14269/
```

## Complete Stack Deployment

Start everything at once:

```bash
docker-compose \
  -f docker-compose.dev.yml \
  -f docker-compose.monitoring.yml \
  -f docker-compose.logging.yml \
  -f docker-compose.tracing.yml \
  up -d
```

## Access Web Interfaces

| Service | URL | Credentials |
|---------|-----|-------------|
| Grafana | http://localhost:3000 | admin/admin |
| Prometheus | http://localhost:9090 | - |
| Kibana | http://localhost:5601 | - |
| Jaeger | http://localhost:16686 | - |
| AlertManager | http://localhost:9093 | - |
| API | http://localhost:5000 | JWT required |
| API Swagger | http://localhost:5000/swagger | JWT required |

## Initial Setup

### Grafana

1. Open http://localhost:3000
2. Login with admin/admin
3. Change password when prompted
4. Navigate to Dashboards
5. The "Slip Verification System" dashboard should be auto-provisioned

### Kibana

1. Open http://localhost:5601
2. Wait for Kibana to initialize (may take 2-3 minutes)
3. Go to Management → Index Patterns
4. Create index pattern: `slip-verification-*`
5. Select `@timestamp` as the time field
6. Go to Discover to view logs

### AlertManager

1. Open http://localhost:9093
2. Configure alert receivers (Slack webhook in `infrastructure/monitoring/alertmanager/alertmanager.yml`)

## Key Metrics Endpoints

### Application Metrics

```bash
# View all API metrics
curl http://localhost:5000/metrics

# Specific metrics
curl http://localhost:5000/metrics | grep slip_verifications_total
curl http://localhost:5000/metrics | grep http_request_duration_seconds
curl http://localhost:5000/metrics | grep active_websocket_connections
```

### Prometheus Queries

Open http://localhost:9090 and try these queries:

```promql
# Request rate
rate(http_request_duration_seconds_count[5m])

# Response time p95
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Error rate
rate(errors_total[5m])

# Slip verifications by status
rate(slip_verifications_total[5m])

# Active connections
active_websocket_connections

# CPU usage
100 - (avg by (instance) (irate(node_cpu_seconds_total{mode="idle"}[5m])) * 100)
```

## Testing the Monitoring Stack

### Generate Test Traffic

```bash
# Install Apache Bench (if not already installed)
# Ubuntu/Debian
sudo apt-get install apache2-utils

# macOS
brew install httpd

# Generate load
ab -n 1000 -c 10 http://localhost:5000/health
```

### View Metrics in Grafana

1. Open http://localhost:3000
2. Go to Dashboards → Slip Verification System
3. You should see:
   - Request rate increasing
   - Response times
   - Active connections
   - System metrics

### View Logs in Kibana

1. Open http://localhost:5601
2. Go to Discover
3. Select `slip-verification-*` index pattern
4. Set time range to "Last 15 minutes"
5. You should see application logs

### Trigger Alerts

```bash
# Simulate high error rate (requires authentication)
for i in {1..100}; do
  curl -X GET http://localhost:5000/api/v1/slips/non-existent-id
done

# Check AlertManager for fired alerts
curl http://localhost:9093/api/v2/alerts
```

## Business Metrics

The following business metrics are automatically tracked:

| Metric | Description | Labels |
|--------|-------------|--------|
| `slip_verifications_total` | Total slip verifications | status, bank |
| `slip_processing_duration_seconds` | Time to process slips | - |
| `active_websocket_connections` | Active WebSocket connections | - |
| `errors_total` | Total errors | type, endpoint |
| `http_request_duration_seconds` | HTTP request duration | method, path, status_code |

### Example: Query Slip Verification Rate

```promql
# Verifications per second
rate(slip_verifications_total[5m])

# Success rate
rate(slip_verifications_total{status="Verified"}[5m]) / rate(slip_verifications_total[5m])

# Failure rate by bank
rate(slip_verifications_total{status="Failed"}[5m]) by (bank)
```

## Troubleshooting

### Prometheus Not Scraping Metrics

```bash
# Check if API is accessible from Prometheus container
docker exec slip-prometheus wget -O- http://api:5000/metrics

# Check Prometheus targets
curl http://localhost:9090/api/v1/targets
```

### No Logs in Kibana

```bash
# Check Fluentd logs
docker logs slip-fluentd

# Check if Elasticsearch is receiving data
curl http://localhost:9200/_cat/indices

# Test Fluentd forwarding
echo '{"message":"test"}' | docker exec -i slip-fluentd fluent-cat test
```

### Grafana Shows No Data

1. Check Prometheus datasource configuration
2. Verify time range in dashboard (default: Last 6 hours)
3. Ensure application is generating traffic
4. Check browser console for errors

### High Resource Usage

```bash
# Check resource usage
docker stats

# Reduce Elasticsearch memory (in docker-compose.logging.yml)
# Change ES_JAVA_OPTS=-Xms512m -Xmx512m to ES_JAVA_OPTS=-Xms256m -Xmx256m
```

## Stopping Services

```bash
# Stop all monitoring services
docker-compose -f docker-compose.monitoring.yml down

# Stop logging stack
docker-compose -f docker-compose.logging.yml down

# Stop tracing stack
docker-compose -f docker-compose.tracing.yml down

# Stop everything including volumes (WARNING: deletes all data)
docker-compose \
  -f docker-compose.dev.yml \
  -f docker-compose.monitoring.yml \
  -f docker-compose.logging.yml \
  -f docker-compose.tracing.yml \
  down -v
```

## Production Deployment

For production deployment:

1. **Update AlertManager Configuration**
   - Add Slack webhook URL
   - Configure PagerDuty integration
   - Set up email notifications

2. **Secure Grafana**
   - Change default admin password
   - Enable HTTPS
   - Configure SSO if available

3. **Configure Log Retention**
   - Set Elasticsearch index lifecycle policies
   - Configure backup strategy

4. **Set Up Monitoring Alerts**
   - Review and adjust alert thresholds
   - Test alert routing
   - Create on-call schedule

5. **Enable Authentication**
   - Protect Prometheus with authentication
   - Enable Kibana security features

## Additional Resources

- [Complete Monitoring Guide](./infrastructure/monitoring/MONITORING_GUIDE.md)
- [On-Call Playbooks](./infrastructure/monitoring/ON_CALL_PLAYBOOKS.md)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [Grafana Documentation](https://grafana.com/docs/)
- [Elasticsearch Documentation](https://www.elastic.co/guide/)

## Support

For issues or questions:
1. Check the monitoring guide and playbooks
2. Review container logs: `docker logs <container-name>`
3. Check service health endpoints
4. Contact the development team

## Next Steps

1. Explore pre-built Grafana dashboards
2. Create custom metrics for your use cases
3. Set up alert notifications
4. Configure log retention policies
5. Review and tune alert thresholds based on actual usage
