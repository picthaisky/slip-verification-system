# Nginx Monitoring Configuration

This directory contains monitoring and logging configuration for Nginx.

## 📊 Prometheus Metrics

### Option 1: nginx-prometheus-exporter

Add to `docker-compose.monitoring.yml`:

```yaml
services:
  nginx-exporter:
    image: nginx/nginx-prometheus-exporter:latest
    container_name: nginx-exporter
    command:
      - '-nginx.scrape-uri=http://nginx:80/stub_status'
    ports:
      - "9113:9113"
    depends_on:
      - nginx
    networks:
      - slip-network
```

Add to nginx configuration (`conf.d/monitoring.conf`):

```nginx
server {
    listen 80;
    server_name _;
    
    location /stub_status {
        stub_status;
        access_log off;
        allow 172.16.0.0/12;  # Docker network
        deny all;
    }
}
```

### Option 2: VTS Module (More Detailed Metrics)

Use `nginx-module-vts` for more detailed metrics:

```dockerfile
FROM nginx:alpine

# Install VTS module
RUN apk add --no-cache nginx-module-vts
```

Configuration:

```nginx
http {
    vhost_traffic_status_zone;
    
    server {
        location /status {
            vhost_traffic_status_display;
            vhost_traffic_status_display_format html;
            access_log off;
        }
    }
}
```

## 📝 Logging

### Access Log Format

Standard format (already configured):
```nginx
log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                '$status $body_bytes_sent "$http_referer" '
                '"$http_user_agent" "$http_x_forwarded_for"';
```

JSON format (for log aggregation):
```nginx
log_format json_combined escape=json
'{'
  '"time_local":"$time_local",'
  '"remote_addr":"$remote_addr",'
  '"remote_user":"$remote_user",'
  '"request":"$request",'
  '"status": "$status",'
  '"body_bytes_sent":"$body_bytes_sent",'
  '"request_time":"$request_time",'
  '"http_referrer":"$http_referer",'
  '"http_user_agent":"$http_user_agent",'
  '"upstream_addr":"$upstream_addr",'
  '"upstream_status":"$upstream_status",'
  '"upstream_response_time":"$upstream_response_time"'
'}';

access_log /var/log/nginx/access.log json_combined;
```

### Log Rotation

Create `/etc/logrotate.d/nginx`:

```
/var/log/nginx/*.log {
    daily
    missingok
    rotate 14
    compress
    delaycompress
    notifempty
    create 0640 nginx adm
    sharedscripts
    postrotate
        if [ -f /var/run/nginx.pid ]; then
            kill -USR1 `cat /var/run/nginx.pid`
        fi
    endscript
}
```

Or use Docker log driver:

```yaml
services:
  nginx:
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

## 🔍 ELK Stack Integration

### Filebeat Configuration

`filebeat.yml`:

```yaml
filebeat.inputs:
  - type: log
    enabled: true
    paths:
      - /var/log/nginx/access.log
    fields:
      type: nginx-access
      
  - type: log
    enabled: true
    paths:
      - /var/log/nginx/error.log
    fields:
      type: nginx-error

output.elasticsearch:
  hosts: ["elasticsearch:9200"]
  
setup.kibana:
  host: "kibana:5601"
```

Add to `docker-compose.monitoring.yml`:

```yaml
filebeat:
  image: docker.elastic.co/beats/filebeat:8.10.0
  user: root
  volumes:
    - ./monitoring/filebeat.yml:/usr/share/filebeat/filebeat.yml:ro
    - nginx_logs:/var/log/nginx:ro
  depends_on:
    - elasticsearch
```

## 📈 Grafana Dashboards

### Import Dashboard

Use Nginx Dashboard ID: `12708` or `9614`

```bash
# Using Grafana API
curl -X POST \
  http://admin:admin@localhost:3000/api/dashboards/import \
  -H 'Content-Type: application/json' \
  -d '{
    "dashboard": { "id": 12708 },
    "overwrite": true,
    "inputs": [{
      "name": "DS_PROMETHEUS",
      "type": "datasource",
      "pluginId": "prometheus",
      "value": "Prometheus"
    }]
  }'
```

### Key Metrics to Monitor

1. **Request Rate**
   - Total requests per second
   - Requests by status code
   - Requests by location

2. **Response Time**
   - Average response time
   - P95/P99 response time
   - Upstream response time

3. **Error Rate**
   - 4xx errors
   - 5xx errors
   - Upstream errors

4. **Throughput**
   - Bytes sent/received
   - Bandwidth usage

5. **Connections**
   - Active connections
   - Connection rate
   - Keepalive connections

6. **Upstream Health**
   - Backend availability
   - Failed requests
   - Health check status

## 🚨 Alerting

### Prometheus Alerts

`alerts.yml`:

```yaml
groups:
  - name: nginx
    interval: 30s
    rules:
      - alert: NginxHighErrorRate
        expr: rate(nginx_http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High error rate on Nginx"
          
      - alert: NginxDown
        expr: up{job="nginx"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "Nginx is down"
          
      - alert: NginxHighResponseTime
        expr: nginx_http_request_duration_seconds{quantile="0.95"} > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Nginx response time is high"
```

## 📊 Real-Time Monitoring

### GoAccess (Terminal-based)

```bash
# Install GoAccess
apt-get install goaccess

# Real-time dashboard
docker exec -it slip-nginx-dev tail -f /var/log/nginx/access.log | \
  goaccess --log-format=COMBINED -
```

### Web-based Dashboard

```bash
# Generate HTML report
docker exec slip-nginx-dev goaccess /var/log/nginx/access.log \
  --log-format=COMBINED \
  -o /var/log/nginx/report.html \
  --real-time-html
```

## 🔧 Health Checks

### Nginx Health Check

Already configured in `default.conf`:

```nginx
location /health {
    access_log off;
    return 200 "healthy\n";
}
```

### Deep Health Check

Add more detailed health check:

```nginx
location /health/detailed {
    access_log off;
    add_header Content-Type application/json;
    return 200 '{"status":"healthy","timestamp":"$date_gmt","uptime":"$nginx_version"}';
}
```

### External Monitoring

Use services like:
- UptimeRobot
- Pingdom
- StatusCake
- AWS CloudWatch
- Google Cloud Monitoring

Configure to check:
- `https://yourdomain.com/health`
- Expected: `200 OK` with body `healthy`

## 📦 Log Aggregation

### Loki (Recommended)

Add to `docker-compose.monitoring.yml`:

```yaml
loki:
  image: grafana/loki:latest
  ports:
    - "3100:3100"
  volumes:
    - ./monitoring/loki-config.yml:/etc/loki/local-config.yaml
    
promtail:
  image: grafana/promtail:latest
  volumes:
    - nginx_logs:/var/log/nginx:ro
    - ./monitoring/promtail-config.yml:/etc/promtail/config.yml
```

## 🎯 Best Practices

1. **Sampling**: Use sampling for high-traffic sites
2. **Retention**: Keep logs for 30-90 days
3. **Alerts**: Set up alerts for critical metrics
4. **Dashboards**: Create custom dashboards for your KPIs
5. **Regular Review**: Review metrics weekly
6. **Capacity Planning**: Monitor trends for capacity planning

## 📚 Resources

- [Nginx Metrics](https://nginx.org/en/docs/http/ngx_http_stub_status_module.html)
- [Prometheus Best Practices](https://prometheus.io/docs/practices/)
- [Grafana Dashboards](https://grafana.com/grafana/dashboards/)
- [ELK Stack Documentation](https://www.elastic.co/guide/index.html)
