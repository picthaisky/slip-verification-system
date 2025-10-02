# On-Call Playbooks

## Table of Contents

1. [General Guidelines](#general-guidelines)
2. [Alert Response Matrix](#alert-response-matrix)
3. [Incident Response Playbooks](#incident-response-playbooks)
4. [Common Issues](#common-issues)
5. [Escalation Procedures](#escalation-procedures)
6. [Post-Incident](#post-incident)

## General Guidelines

### On-Call Responsibilities

- **Response Time**: Critical alerts within 5 minutes, warnings within 30 minutes
- **Communication**: Update team on incident status
- **Documentation**: Log all actions taken during incident
- **Escalation**: Escalate if unable to resolve within 30 minutes

### Tools Access

- Grafana: http://localhost:3000
- Prometheus: http://localhost:9090
- Kibana: http://localhost:5601
- Jaeger: http://localhost:16686
- AlertManager: http://localhost:9093

### Initial Response Checklist

1. ☐ Acknowledge alert
2. ☐ Check monitoring dashboards
3. ☐ Review recent logs
4. ☐ Check system resources
5. ☐ Notify team if severity is critical
6. ☐ Document investigation steps

## Alert Response Matrix

| Alert | Severity | Response Time | Action Required |
|-------|----------|---------------|-----------------|
| HighErrorRate | Critical | 5 minutes | Follow High Error Rate playbook |
| SlowResponseTime | Warning | 30 minutes | Follow Slow Response playbook |
| ServiceDown | Critical | 5 minutes | Follow Service Down playbook |
| DatabaseConnectionFailed | Critical | 5 minutes | Follow Database Issues playbook |
| HighMemoryUsage | Warning | 30 minutes | Monitor and investigate |
| DiskSpaceLow | Warning | 30 minutes | Clean up disk space |
| HighSlipVerificationFailureRate | Warning | 30 minutes | Investigate business logic |

## Incident Response Playbooks

### 1. High Error Rate

**Alert**: `HighErrorRate`
**Severity**: Critical
**Threshold**: Error rate > 5% for 5 minutes

#### Investigation Steps

1. **Check Error Dashboard**
   ```bash
   # Open Grafana error panel
   http://localhost:3000/d/slip-verification/error-rate
   ```

2. **Identify Error Types**
   ```bash
   # Query Prometheus for error breakdown
   curl 'http://localhost:9090/api/v1/query?query=rate(errors_total[5m])'
   ```

3. **Check Recent Logs**
   ```bash
   # View errors in Kibana
   http://localhost:5601/app/discover
   # Filter: Level:Error AND @timestamp:[now-15m TO now]
   ```

4. **Review Recent Deployments**
   ```bash
   # Check if errors started after deployment
   git log --oneline -n 10
   ```

#### Common Causes

- Recent deployment with bugs
- Database connectivity issues
- External API failures
- Invalid input data

#### Resolution Steps

**If caused by recent deployment:**
```bash
# Rollback to previous version
git checkout <previous-commit>
docker-compose -f docker-compose.prod.yml up -d --build api
```

**If database connectivity:**
```bash
# Check database health
docker exec slip-postgres pg_isready
# Restart database if needed
docker restart slip-postgres
```

**If external API failure:**
- Check external service status
- Enable fallback/circuit breaker if available
- Notify team about dependency issue

#### Verification

```bash
# Confirm error rate is decreasing
curl 'http://localhost:9090/api/v1/query?query=rate(errors_total[5m])'

# Check recent requests are succeeding
curl http://localhost:5000/health
```

---

### 2. Slow Response Time

**Alert**: `SlowResponseTime`
**Severity**: Warning
**Threshold**: p95 response time > 1s for 5 minutes

#### Investigation Steps

1. **Check Response Time Dashboard**
   ```bash
   # View response time trends
   http://localhost:3000/d/slip-verification/response-time
   ```

2. **Identify Slow Endpoints**
   ```promql
   # Find slowest endpoints
   topk(10, histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])))
   ```

3. **Check Database Performance**
   ```bash
   # Check slow queries
   docker exec slip-postgres psql -U slipverification -d SlipVerificationDb -c "
   SELECT pid, now() - query_start as duration, query
   FROM pg_stat_activity
   WHERE state = 'active'
   ORDER BY duration DESC;"
   ```

4. **Review Traces in Jaeger**
   ```bash
   # Open Jaeger and search for slow traces
   http://localhost:16686
   # Filter by latency > 1s
   ```

#### Common Causes

- Database query performance
- High load/traffic spike
- Resource constraints (CPU, memory)
- External API slowness
- N+1 query problems

#### Resolution Steps

**If database slow queries:**
```bash
# Add missing indexes
# Check query plan
docker exec slip-postgres psql -U slipverification -d SlipVerificationDb -c "
EXPLAIN ANALYZE <slow-query>;"
```

**If high load:**
```bash
# Scale up API instances
docker-compose -f docker-compose.prod.yml up -d --scale api=3

# Or increase resources
docker update --cpus="2" --memory="2g" slip-api
```

**If resource constraints:**
```bash
# Check system resources
docker stats

# Check CPU and memory usage
curl 'http://localhost:9090/api/v1/query?query=node_memory_MemAvailable_bytes'
```

#### Verification

```bash
# Confirm response time is improving
curl 'http://localhost:9090/api/v1/query?query=histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))'
```

---

### 3. Service Down

**Alert**: `ServiceDown`
**Severity**: Critical
**Threshold**: Service down for 2 minutes

#### Investigation Steps

1. **Check Service Status**
   ```bash
   # Check running containers
   docker ps -a | grep slip-
   
   # Check specific service
   docker ps | grep slip-api
   ```

2. **Check Service Logs**
   ```bash
   # View container logs
   docker logs --tail 100 slip-api
   
   # Follow logs in real-time
   docker logs -f slip-api
   ```

3. **Check Health Endpoint**
   ```bash
   # Test health endpoint
   curl http://localhost:5000/health
   ```

4. **Check System Resources**
   ```bash
   # Check if OOM killed the service
   dmesg | grep -i oom
   
   # Check disk space
   df -h
   ```

#### Common Causes

- Container crashed
- Out of memory (OOM)
- Configuration error
- Port conflict
- Database unavailable

#### Resolution Steps

**If container stopped:**
```bash
# Restart the container
docker restart slip-api

# Or restart all services
docker-compose -f docker-compose.prod.yml restart
```

**If container won't start:**
```bash
# Check detailed logs
docker logs slip-api

# Try to start manually to see error
docker start slip-api

# Check configuration
docker inspect slip-api
```

**If OOM issue:**
```bash
# Increase memory limit
docker update --memory="2g" slip-api

# Or in docker-compose.yml
services:
  api:
    deploy:
      resources:
        limits:
          memory: 2G
```

**If configuration error:**
```bash
# Validate environment variables
docker exec slip-api env

# Check configuration files
docker exec slip-api cat /app/appsettings.json
```

#### Verification

```bash
# Confirm service is up
curl http://localhost:5000/health

# Check Prometheus target
curl 'http://localhost:9090/api/v1/targets' | jq '.data.activeTargets[] | select(.labels.job=="api")'
```

---

### 4. Database Connection Failed

**Alert**: `DatabaseConnectionFailed`
**Severity**: Critical
**Threshold**: DB unreachable for 1 minute

#### Investigation Steps

1. **Check Database Container**
   ```bash
   # Check if PostgreSQL is running
   docker ps | grep postgres
   
   # Check PostgreSQL logs
   docker logs --tail 50 slip-postgres
   ```

2. **Test Database Connection**
   ```bash
   # Connect to database
   docker exec slip-postgres pg_isready
   
   # Try psql connection
   docker exec -it slip-postgres psql -U slipverification -d SlipVerificationDb
   ```

3. **Check Database Connections**
   ```bash
   # View active connections
   docker exec slip-postgres psql -U slipverification -d SlipVerificationDb -c "
   SELECT count(*) FROM pg_stat_activity;"
   ```

4. **Check Connection Pool**
   ```bash
   # Check API connection pool metrics
   curl 'http://localhost:9090/api/v1/query?query=database_connections_active'
   ```

#### Common Causes

- Database container stopped
- Connection pool exhausted
- Too many connections
- Network issues
- Disk space full

#### Resolution Steps

**If database stopped:**
```bash
# Restart PostgreSQL
docker restart slip-postgres

# Wait for database to be ready
until docker exec slip-postgres pg_isready; do sleep 1; done
```

**If connection pool exhausted:**
```bash
# Kill idle connections
docker exec slip-postgres psql -U slipverification -d SlipVerificationDb -c "
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE state = 'idle'
AND state_change < now() - interval '5 minutes';"

# Restart API to reset connection pool
docker restart slip-api
```

**If too many connections:**
```bash
# Increase max connections (requires restart)
docker exec slip-postgres sed -i 's/max_connections = 100/max_connections = 200/' /var/lib/postgresql/data/postgresql.conf
docker restart slip-postgres
```

**If disk space full:**
```bash
# Check disk usage
df -h

# Clean up old logs
docker exec slip-postgres find /var/lib/postgresql/data/pg_log -type f -mtime +7 -delete

# Vacuum database
docker exec slip-postgres psql -U slipverification -d SlipVerificationDb -c "VACUUM FULL;"
```

#### Verification

```bash
# Confirm database is accessible
docker exec slip-postgres pg_isready

# Check API can connect
curl http://localhost:5000/health

# Verify metrics
curl 'http://localhost:9090/api/v1/query?query=pg_up'
```

---

### 5. High Slip Verification Failure Rate

**Alert**: `HighSlipVerificationFailureRate`
**Severity**: Warning
**Threshold**: > 20% failures for 5 minutes

#### Investigation Steps

1. **Check Failure Rate Dashboard**
   ```bash
   # View verification statistics
   http://localhost:3000/d/slip-verification/business-metrics
   ```

2. **Analyze Failed Verifications**
   ```bash
   # Query for failed verifications
   curl 'http://localhost:9200/slip-verification-*/_search' -H 'Content-Type: application/json' -d'{
     "query": {
       "bool": {
         "must": [
           {"match": {"Status": "Failed"}},
           {"range": {"@timestamp": {"gte": "now-15m"}}}
         ]
       }
     }
   }'
   ```

3. **Check OCR Service**
   ```bash
   # Check OCR service health
   curl http://localhost:8000/health
   
   # Check OCR service logs
   docker logs --tail 50 slip-ocr-service
   ```

4. **Review Common Failure Reasons**
   ```bash
   # Group failures by reason
   # Check Kibana visualizations
   ```

#### Common Causes

- OCR service issues
- Poor quality slip images
- Bank API changes
- Network issues with external services
- Invalid slip format

#### Resolution Steps

**If OCR service issue:**
```bash
# Restart OCR service
docker restart slip-ocr-service

# Check OCR service resources
docker stats slip-ocr-service
```

**If external bank API issue:**
- Check bank API status page
- Enable fallback verification method
- Notify team about bank API issue

**If pattern recognition failure:**
- Update OCR patterns/models
- Add manual review queue
- Notify team for pattern updates

#### Verification

```bash
# Monitor failure rate
curl 'http://localhost:9090/api/v1/query?query=rate(slip_verifications_total{status="Failed"}[5m])/rate(slip_verifications_total[5m])'

# Test with sample slip
curl -X POST http://localhost:5000/api/v1/slips/verify \
  -F "file=@test-slip.jpg" \
  -F "orderId=test-123"
```

---

### 6. High Memory Usage

**Alert**: `HighMemoryUsage`
**Severity**: Warning
**Threshold**: Memory > 85% for 5 minutes

#### Investigation Steps

1. **Check Memory Usage**
   ```bash
   # System memory
   free -h
   
   # Docker container memory
   docker stats --no-stream
   ```

2. **Identify Memory-Heavy Containers**
   ```bash
   # Sort by memory usage
   docker stats --no-stream --format "table {{.Name}}\t{{.MemUsage}}" | sort -k 2 -h
   ```

3. **Check for Memory Leaks**
   ```bash
   # Check memory trend
   curl 'http://localhost:9090/api/v1/query?query=process_resident_memory_bytes'
   
   # View memory over time
   http://localhost:3000/d/slip-verification/memory-usage
   ```

#### Resolution Steps

**If specific container using too much memory:**
```bash
# Restart container to free memory
docker restart <container-name>

# Set memory limit
docker update --memory="1g" <container-name>
```

**If memory leak suspected:**
- Review recent code changes
- Check for unclosed connections
- Deploy hotfix if leak identified

**If system memory low:**
```bash
# Clear system cache
sync; echo 3 > /proc/sys/vm/drop_caches

# Clean Docker resources
docker system prune -a
```

---

## Common Issues

### Issue: Metrics Not Showing in Grafana

**Symptoms**: Empty dashboards, no data in Prometheus

**Solution**:
```bash
# Check Prometheus targets
curl http://localhost:9090/api/v1/targets

# Verify API metrics endpoint
curl http://localhost:5000/metrics

# Check if Prometheus can reach API
docker exec slip-prometheus wget -O- http://api:5000/metrics
```

### Issue: Logs Not Appearing in Kibana

**Symptoms**: No logs in Kibana, empty indices

**Solution**:
```bash
# Check Fluentd logs
docker logs slip-fluentd

# Verify Elasticsearch indices
curl http://localhost:9200/_cat/indices

# Test Fluentd forwarding
docker exec slip-fluentd curl -X POST -d 'json={"message":"test"}' http://localhost:24224/test.log

# Check Elasticsearch health
curl http://localhost:9200/_cluster/health
```

### Issue: Alerts Not Firing

**Symptoms**: No alerts despite issues

**Solution**:
```bash
# Check AlertManager status
curl http://localhost:9093/api/v2/status

# Check alert rules
curl http://localhost:9090/api/v1/rules

# Test alert manually
curl -X POST http://localhost:9093/api/v2/alerts -d '[{
  "labels": {"alertname": "test"},
  "annotations": {"description": "test alert"}
}]'
```

## Escalation Procedures

### Level 1: On-Call Engineer
- **Responsibility**: Initial response and common issues
- **SLA**: 5 minutes for critical, 30 minutes for warning
- **Escalate if**: Unable to resolve within 30 minutes

### Level 2: Senior Engineer / Team Lead
- **Responsibility**: Complex issues requiring architectural knowledge
- **Contact**: Escalate via phone/Slack
- **Escalate if**: Requires code changes or architectural decisions

### Level 3: Engineering Manager / CTO
- **Responsibility**: Business-critical decisions
- **Contact**: Only for major outages or customer-impacting issues
- **Escalate if**: Requires business/executive decision

### Escalation Contacts

```
On-Call Engineer: (Primary) - See PagerDuty schedule
Senior Engineer: (Secondary) - [Contact Info]
Team Lead: (Tertiary) - [Contact Info]
Engineering Manager: (Emergency) - [Contact Info]
```

## Post-Incident

### Incident Report Template

```markdown
# Incident Report: [YYYY-MM-DD] [Brief Description]

## Summary
Brief description of the incident.

## Timeline
- HH:MM - Alert fired
- HH:MM - Investigation started
- HH:MM - Root cause identified
- HH:MM - Fix applied
- HH:MM - Incident resolved

## Impact
- Duration: X minutes
- Affected users: X
- Affected services: List
- Business impact: Description

## Root Cause
Detailed explanation of what caused the incident.

## Resolution
Steps taken to resolve the incident.

## Action Items
- [ ] Short-term fix
- [ ] Long-term prevention
- [ ] Monitoring improvements
- [ ] Documentation updates

## Lessons Learned
What we learned and how to prevent future incidents.
```

### Post-Incident Checklist

1. ☐ Write incident report
2. ☐ Update runbooks based on learnings
3. ☐ Create tickets for action items
4. ☐ Schedule post-mortem meeting
5. ☐ Update alert thresholds if needed
6. ☐ Communicate resolution to stakeholders

### Post-Mortem Meeting Agenda

1. Incident timeline review
2. Root cause analysis
3. What went well
4. What could be improved
5. Action items and owners
6. Update documentation and runbooks

## Additional Resources

### Quick Reference Commands

```bash
# Check all service health
curl http://localhost:5000/health
curl http://localhost:9090/-/healthy
curl http://localhost:9200/_cluster/health

# View all containers
docker ps -a

# View all logs
docker-compose logs -f

# Restart all services
docker-compose -f docker-compose.prod.yml restart

# Scale API
docker-compose -f docker-compose.prod.yml up -d --scale api=3
```

### Monitoring URLs

- API: http://localhost:5000
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000
- Kibana: http://localhost:5601
- Jaeger: http://localhost:16686
- AlertManager: http://localhost:9093

### Useful Prometheus Queries

```promql
# Request rate
rate(http_request_duration_seconds_count[5m])

# Error rate
rate(errors_total[5m])

# Response time p95
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Active connections
active_websocket_connections

# CPU usage
100 - (avg by (instance) (irate(node_cpu_seconds_total{mode="idle"}[5m])) * 100)
```
