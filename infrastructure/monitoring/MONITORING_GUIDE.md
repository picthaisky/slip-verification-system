# Complete Monitoring & Observability Stack

This document provides comprehensive documentation for the complete monitoring and observability solution implemented for the Slip Verification System.

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Components](#components)
4. [Metrics Collection](#metrics-collection)
5. [Visualization](#visualization)
6. [Centralized Logging](#centralized-logging)
7. [Distributed Tracing](#distributed-tracing)
8. [Alerting](#alerting)
9. [Deployment](#deployment)
10. [Key Metrics](#key-metrics)
11. [Troubleshooting](#troubleshooting)

## Overview

The monitoring stack consists of:
- **Prometheus**: Metrics collection and storage
- **Grafana**: Visualization and dashboards
- **Elasticsearch + Fluentd + Kibana (EFK)**: Centralized logging
- **Jaeger**: Distributed tracing
- **AlertManager**: Alert routing and notification

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Monitoring Architecture                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────┐     ┌──────────────┐     ┌────────────────┐  │
│  │   API       │────▶│  Prometheus  │────▶│    Grafana     │  │
│  │   (Metrics) │     │  (Storage)   │     │ (Visualization)│  │
│  └─────────────┘     └──────────────┘     └────────────────┘  │
│         │                     │                                  │
│         │                     ▼                                  │
│         │            ┌──────────────┐                           │
│         │            │ AlertManager │                           │
│         │            │  (Alerting)  │                           │
│         │            └──────────────┘                           │
│         │                                                        │
│         │            ┌──────────────┐     ┌────────────────┐  │
│         └───────────▶│   Fluentd    │────▶│ Elasticsearch  │  │
│                      │ (Log Forward)│     │  (Log Storage) │  │
│                      └──────────────┘     └────────────────┘  │
│                                                   │             │
│                                                   ▼             │
│                                            ┌────────────────┐  │
│                                            │     Kibana     │  │
│                                            │(Log Visualization)│
│                                            └────────────────┘  │
│                                                                 │
│  ┌─────────────┐                          ┌────────────────┐  │
│  │   API       │─────────────────────────▶│     Jaeger     │  │
│  │  (Traces)   │                          │   (Tracing)    │  │
│  └─────────────┘                          └────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Components

### Prometheus (Metrics)
- **Port**: 9090
- **Function**: Time-series metrics collection and storage
- **Configuration**: `infrastructure/monitoring/prometheus/prometheus.yml`

### Grafana (Visualization)
- **Port**: 3000
- **Function**: Metrics visualization and dashboards
- **Default Credentials**: admin/admin
- **Dashboards**: `infrastructure/monitoring/grafana/dashboards/`

### Elasticsearch (Log Storage)
- **Port**: 9200 (HTTP), 9300 (Transport)
- **Function**: Store and index logs
- **Index Pattern**: `slip-verification-YYYY.MM.DD`

### Kibana (Log Visualization)
- **Port**: 5601
- **Function**: Log search and visualization
- **Access**: http://localhost:5601

### Fluentd (Log Forwarding)
- **Port**: 24224
- **Function**: Collect and forward logs to Elasticsearch
- **Configuration**: `infrastructure/logging/fluentd/fluent.conf`

### Jaeger (Distributed Tracing)
- **UI Port**: 16686
- **Collector Port**: 14268
- **Function**: Track distributed requests across services
- **Access**: http://localhost:16686

### AlertManager (Alerting)
- **Port**: 9093
- **Function**: Alert routing and notification management
- **Configuration**: `infrastructure/monitoring/alertmanager/alertmanager.yml`

## Metrics Collection

### .NET API Metrics

The API exposes metrics at `/metrics` endpoint using prometheus-net:

#### Custom Business Metrics

```csharp
// Slip verification counter
slip_verifications_total{status="Verified", bank="Bangkok Bank"}

// Slip processing duration histogram
slip_processing_duration_seconds_bucket{le="0.1"}
slip_processing_duration_seconds_bucket{le="0.2"}

// Active WebSocket connections
active_websocket_connections

// Error counter
errors_total{type="ValidationException", endpoint="/api/v1/slips"}
```

#### Standard HTTP Metrics

```
http_request_duration_seconds_bucket{method="GET", path="/api/v1/slips", status_code="200"}
http_request_duration_seconds_count
http_request_duration_seconds_sum
```

### Infrastructure Metrics

- **Node Exporter**: System metrics (CPU, memory, disk)
- **PostgreSQL Exporter**: Database metrics
- **Redis Exporter**: Cache metrics

## Visualization

### Grafana Dashboards

Access Grafana at http://localhost:3000

#### Main Dashboard: Slip Verification System

Includes panels for:
1. **Request Rate**: Requests per second by endpoint
2. **Response Time**: p50, p95, p99 percentiles
3. **Error Rate**: Errors per second by type
4. **Slip Verifications**: Verifications by status and bank
5. **Slip Processing Duration**: Processing time percentiles
6. **Active WebSocket Connections**: Real-time connections
7. **Total Requests**: Request volume
8. **Database Connections**: PostgreSQL connections
9. **Redis Memory**: Cache memory usage
10. **CPU Usage**: System CPU utilization
11. **Memory Usage**: System memory utilization

### Creating Custom Dashboards

```json
{
  "dashboard": {
    "title": "Custom Dashboard",
    "panels": [
      {
        "title": "My Metric",
        "targets": [
          {
            "expr": "rate(my_metric[5m])",
            "legendFormat": "{{label}}"
          }
        ]
      }
    ]
  }
}
```

## Centralized Logging

### Log Flow

```
Application (Serilog) → Fluentd → Elasticsearch → Kibana
```

### Serilog Configuration

Logs are automatically sent to Elasticsearch with the following enrichers:
- Machine name
- Environment name
- Application name
- Log context

### Kibana Index Pattern

1. Navigate to Kibana at http://localhost:5601
2. Go to Management → Index Patterns
3. Create pattern: `slip-verification-*`
4. Select timestamp field: `@timestamp`

### Common Log Queries

```
# Find errors in last 1 hour
Level:Error AND @timestamp:[now-1h TO now]

# Search by request ID
RequestId:"abc123"

# Find slow requests
@fields.Duration:>1000
```

## Distributed Tracing

### Jaeger Setup

Access Jaeger UI at http://localhost:16686

### Viewing Traces

1. Select service: "SlipVerification.API"
2. Set time range
3. Click "Find Traces"
4. Select a trace to view details

### Trace Information

Each trace includes:
- Request span with full path
- Database query spans
- External API call spans
- Processing time per operation

## Alerting

### Alert Rules

Located in `infrastructure/monitoring/prometheus/alerts.yml`

#### API Alerts

- **HighErrorRate**: Error rate > 5% for 5 minutes (Critical)
- **SlowResponseTime**: p95 response time > 1s for 5 minutes (Warning)
- **HighMemoryUsage**: Memory > 1GB for 5 minutes (Warning)
- **DatabasePoolExhausted**: DB connections > 90% of max for 2 minutes (Critical)

#### Business Alerts

- **HighSlipVerificationFailureRate**: >20% failures for 5 minutes (Warning)
- **SlowSlipProcessing**: p95 processing time > 5s for 5 minutes (Warning)
- **NoSlipVerificationsReceived**: No verifications for 10 minutes (Info)

#### Infrastructure Alerts

- **HighCPUUsage**: CPU > 80% for 5 minutes (Warning)
- **HighMemoryUsage**: Memory > 85% for 5 minutes (Warning)
- **ServiceDown**: Service down for 2 minutes (Critical)
- **DatabaseConnectionFailed**: DB unreachable for 1 minute (Critical)
- **RedisDown**: Redis unreachable for 1 minute (Critical)
- **DiskSpaceLow**: Disk < 15% free for 5 minutes (Warning)

### Alert Routing

Configured in `infrastructure/monitoring/alertmanager/alertmanager.yml`:

- **Critical alerts**: Slack + PagerDuty (5-minute repeat)
- **Warning alerts**: Slack only (1-hour repeat)
- **Info alerts**: Log only

### Setting Up Slack Alerts

1. Create a Slack webhook URL
2. Update `alertmanager.yml`:
   ```yaml
   global:
     slack_api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
   ```

## Deployment

### Start All Monitoring Services

```bash
# Start monitoring stack (Prometheus, Grafana, AlertManager)
docker-compose -f docker-compose.monitoring.yml up -d

# Start logging stack (Elasticsearch, Kibana, Fluentd)
docker-compose -f docker-compose.logging.yml up -d

# Start tracing stack (Jaeger)
docker-compose -f docker-compose.tracing.yml up -d
```

### Start Complete Stack

```bash
# Start all services at once
docker-compose \
  -f docker-compose.dev.yml \
  -f docker-compose.monitoring.yml \
  -f docker-compose.logging.yml \
  -f docker-compose.tracing.yml \
  up -d
```

### Verify Deployment

```bash
# Check Prometheus targets
curl http://localhost:9090/api/v1/targets

# Check API metrics endpoint
curl http://localhost:5000/metrics

# Check Elasticsearch health
curl http://localhost:9200/_cluster/health

# Check Jaeger health
curl http://localhost:14269/
```

### Access UIs

- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (admin/admin)
- Kibana: http://localhost:5601
- Jaeger: http://localhost:16686
- AlertManager: http://localhost:9093

## Key Metrics

### Technical Metrics

| Metric | Description | Target | Alert Threshold |
|--------|-------------|--------|-----------------|
| Request Rate | Requests per second | N/A | N/A |
| Response Time (p50) | Median response time | < 200ms | N/A |
| Response Time (p95) | 95th percentile | < 500ms | > 1s |
| Response Time (p99) | 99th percentile | < 1s | > 2s |
| Error Rate | % of failed requests | < 1% | > 5% |
| Database Query Time | DB query duration | < 50ms | > 100ms |
| Cache Hit Ratio | Redis cache hits | > 90% | < 70% |
| Active Connections | WebSocket connections | N/A | N/A |

### Business Metrics

| Metric | Description | Target | Alert Threshold |
|--------|-------------|--------|-----------------|
| Slips Verified/Hour | Verification throughput | N/A | 0 for 10 min |
| Success Rate | % successful verifications | > 95% | < 80% |
| Average Processing Time | Mean slip processing | < 3s | > 5s |
| Manual Review Rate | % requiring manual review | < 5% | > 20% |

### SLA Metrics

- **Availability**: 99.9% uptime (< 43 minutes downtime/month)
- **Response Time**: 95% of requests < 500ms
- **Error Rate**: < 1% of all requests

## Troubleshooting

### High Error Rate

1. Check Grafana error panel
2. View logs in Kibana filtered by error level
3. Check alert details in AlertManager
4. Review error traces in Jaeger

```bash
# Check recent errors
curl 'http://localhost:9200/slip-verification-*/_search?q=Level:Error'
```

### Slow Response Time

1. Check Grafana response time panel
2. Identify slow endpoints in traces (Jaeger)
3. Review database query performance
4. Check system resources (CPU, memory)

```promql
# Find slowest endpoints
topk(5, histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])))
```

### Missing Metrics

```bash
# Check Prometheus targets
curl http://localhost:9090/api/v1/targets | jq '.data.activeTargets[] | select(.health != "up")'

# Check API /metrics endpoint
curl http://localhost:5000/metrics

# Verify scrape configuration
docker exec slip-prometheus cat /etc/prometheus/prometheus.yml
```

### Log Collection Issues

```bash
# Check Fluentd logs
docker logs slip-fluentd

# Verify Elasticsearch indices
curl http://localhost:9200/_cat/indices

# Test log forwarding
docker exec slip-fluentd curl -X POST -d 'json={"message":"test"}' http://localhost:24224/test.log
```

### Jaeger Not Showing Traces

1. Verify Jaeger is running:
   ```bash
   docker ps | grep jaeger
   ```

2. Check Jaeger health:
   ```bash
   curl http://localhost:14269/
   ```

3. Ensure application is configured to send traces to Jaeger

## Best Practices

### Monitoring

1. **Set appropriate alert thresholds**: Balance between noise and coverage
2. **Use meaningful metric labels**: Make queries easier
3. **Monitor business metrics**: Not just technical metrics
4. **Regular dashboard reviews**: Ensure relevance
5. **Document runbooks**: For common alerts

### Logging

1. **Structured logging**: Use JSON format
2. **Include context**: Request ID, user ID, etc.
3. **Appropriate log levels**: INFO for normal operations, ERROR for failures
4. **Sensitive data**: Never log passwords, tokens, or PII
5. **Log retention**: Set appropriate retention policies

### Tracing

1. **Trace key operations**: Focus on user-facing features
2. **Add custom spans**: For important business logic
3. **Include metadata**: Request parameters, user info
4. **Sample traces**: For high-volume systems

### Alerting

1. **Critical alerts**: Page on-call engineer (< 5 min response)
2. **Warning alerts**: Slack notification (monitor actively)
3. **Info alerts**: Log only (no immediate action)
4. **Alert fatigue**: Tune thresholds to reduce noise
5. **Runbooks**: Document response procedures

## Integration with CI/CD

### Health Check in Pipeline

```yaml
- name: Health Check
  run: |
    curl -f http://localhost:5000/health || exit 1
    curl -f http://localhost:9090/-/healthy || exit 1
```

### Load Testing with Metrics

```bash
# Run k6 load test and observe metrics
k6 run tests/performance/load-test.js

# Check metrics during test
curl http://localhost:9090/api/v1/query?query=rate(http_request_duration_seconds_count[1m])
```

## Support and Resources

### Documentation
- Prometheus: https://prometheus.io/docs/
- Grafana: https://grafana.com/docs/
- Elasticsearch: https://www.elastic.co/guide/
- Jaeger: https://www.jaegertracing.io/docs/

### Dashboards
- Grafana Dashboard Library: https://grafana.com/grafana/dashboards/
- Prometheus Exporters: https://prometheus.io/docs/instrumenting/exporters/

### Community
- Prometheus Mailing List
- CNCF Slack
- Grafana Community Forums

## Next Steps

1. Configure production alert channels (Slack, PagerDuty)
2. Set up log retention policies
3. Create custom dashboards for specific use cases
4. Implement distributed tracing in application code
5. Set up automated metric-based scaling
6. Create on-call playbooks for common scenarios
7. Schedule regular monitoring review sessions
