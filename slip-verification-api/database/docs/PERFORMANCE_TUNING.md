# Database Performance Tuning Guide

## Overview
This guide provides PostgreSQL 16 performance tuning recommendations for the Slip Verification System, optimized for 1,000+ transactions per day.

## Hardware Recommendations

### Minimum Requirements (Development)
- **CPU**: 4 cores
- **RAM**: 8 GB
- **Storage**: 100 GB SSD
- **Network**: 1 Gbps

### Production Requirements (1,000+ TPS)
- **CPU**: 8-16 cores
- **RAM**: 32-64 GB
- **Storage**: 500 GB NVMe SSD (RAID 10)
- **Network**: 10 Gbps
- **Backup Storage**: 2 TB (separate disk)

## PostgreSQL Configuration

### postgresql.conf Tuning

#### Memory Settings
```conf
# Memory Configuration
# Rule of thumb: shared_buffers = 25% of RAM (for dedicated DB server)
shared_buffers = 8GB                  # 25% of 32GB RAM
effective_cache_size = 24GB           # 75% of 32GB RAM
maintenance_work_mem = 2GB            # For VACUUM, CREATE INDEX
work_mem = 64MB                       # Per sort/hash operation
wal_buffers = 16MB                    # Write-ahead log buffer
```

#### Connection Pooling
```conf
# Connection Settings
max_connections = 200                 # Application + background jobs
superuser_reserved_connections = 5    # Reserved for admin
```

#### Query Planner
```conf
# Query Planner Configuration
random_page_cost = 1.1                # For SSD storage (default is 4.0)
effective_io_concurrency = 200        # For SSD storage
default_statistics_target = 100       # For better query plans
```

#### WAL (Write-Ahead Logging)
```conf
# WAL Configuration
wal_level = replica                   # For replication support
wal_buffers = 16MB
min_wal_size = 1GB
max_wal_size = 4GB
checkpoint_completion_target = 0.9    # Spread checkpoints
checkpoint_timeout = 15min
```

#### Autovacuum
```conf
# Autovacuum Configuration
autovacuum = on
autovacuum_max_workers = 4
autovacuum_naptime = 30s
autovacuum_vacuum_threshold = 50
autovacuum_vacuum_scale_factor = 0.1  # More aggressive
autovacuum_analyze_threshold = 50
autovacuum_analyze_scale_factor = 0.05
```

#### Logging
```conf
# Logging Configuration
logging_collector = on
log_directory = 'log'
log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'
log_rotation_age = 1d
log_rotation_size = 100MB
log_min_duration_statement = 1000     # Log queries > 1 second
log_line_prefix = '%t [%p]: [%l-1] user=%u,db=%d,app=%a,client=%h '
log_checkpoints = on
log_connections = on
log_disconnections = on
log_lock_waits = on
log_temp_files = 0                    # Log all temp files
log_autovacuum_min_duration = 0       # Log all autovacuum
```

#### Parallel Query
```conf
# Parallel Query Configuration
max_parallel_workers_per_gather = 4
max_parallel_workers = 8
max_worker_processes = 8
parallel_leader_participation = on
```

### pg_hba.conf Security
```conf
# TYPE  DATABASE        USER            ADDRESS                 METHOD
local   all             postgres                                peer
host    all             all             127.0.0.1/32            scram-sha-256
host    all             all             ::1/128                 scram-sha-256
host    SlipVerificationDb app_user     10.0.0.0/8              scram-sha-256
```

## Connection Pooling

### PgBouncer Configuration

#### pgbouncer.ini
```ini
[databases]
SlipVerificationDb = host=localhost port=5432 dbname=SlipVerificationDb

[pgbouncer]
listen_addr = *
listen_port = 6432
auth_type = scram-sha-256
auth_file = /etc/pgbouncer/userlist.txt
pool_mode = transaction
max_client_conn = 1000
default_pool_size = 25
min_pool_size = 10
reserve_pool_size = 5
reserve_pool_timeout = 5
max_db_connections = 100
max_user_connections = 100
server_idle_timeout = 600
server_lifetime = 3600
server_connect_timeout = 15
query_timeout = 120
```

### Application-Level Pooling (.NET)
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=6432;Database=SlipVerificationDb;Username=app_user;Password=***;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;Connection Idle Lifetime=300;Connection Pruning Interval=10"
  }
}
```

## Index Optimization

### Monitor Index Usage
```sql
-- Find unused indexes
SELECT 
    schemaname,
    tablename,
    indexname,
    idx_scan,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
WHERE idx_scan = 0
  AND indexname NOT LIKE '%_pkey'
ORDER BY pg_relation_size(indexrelid) DESC;

-- Index hit ratio (should be > 95%)
SELECT 
    sum(idx_blks_hit) / nullif(sum(idx_blks_hit + idx_blks_read), 0) * 100 AS index_hit_ratio
FROM pg_statio_user_indexes;
```

### Rebuild Bloated Indexes
```sql
-- Check index bloat
SELECT 
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) AS index_size,
    pg_size_pretty(pg_total_relation_size(indexrelid) - pg_relation_size(indexrelid)) AS bloat_size
FROM pg_stat_user_indexes
ORDER BY pg_total_relation_size(indexrelid) - pg_relation_size(indexrelid) DESC;

-- Rebuild indexes (do during maintenance window)
REINDEX TABLE "Transactions";
REINDEX TABLE "Orders";
```

## Query Optimization

### Analyze Slow Queries
```sql
-- Enable pg_stat_statements extension
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Find slowest queries
SELECT 
    query,
    calls,
    mean_exec_time,
    total_exec_time,
    rows
FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 20;

-- Reset statistics
SELECT pg_stat_statements_reset();
```

### Query Optimization Techniques

#### 1. Use EXPLAIN ANALYZE
```sql
EXPLAIN ANALYZE
SELECT o.*, u."Username"
FROM "Orders" o
JOIN "Users" u ON o."UserId" = u."Id"
WHERE o."Status" = 'Pending'
  AND o."DeletedAt" IS NULL;
```

#### 2. Avoid SELECT *
```sql
-- Bad
SELECT * FROM "Orders" WHERE "UserId" = '...';

-- Good
SELECT "Id", "OrderNumber", "Amount", "Status" 
FROM "Orders" 
WHERE "UserId" = '...';
```

#### 3. Use EXISTS Instead of IN
```sql
-- Bad
SELECT * FROM "Users" 
WHERE "Id" IN (SELECT "UserId" FROM "Orders");

-- Good
SELECT * FROM "Users" u
WHERE EXISTS (SELECT 1 FROM "Orders" o WHERE o."UserId" = u."Id");
```

#### 4. Limit Result Sets
```sql
-- Always use pagination
SELECT * FROM "Transactions"
ORDER BY "CreatedAt" DESC
LIMIT 50 OFFSET 0;
```

## Table Partitioning

### Partition Transactions by Month
```sql
-- Create partitioned table
CREATE TABLE "Transactions_Partitioned" (
    LIKE "Transactions" INCLUDING ALL
) PARTITION BY RANGE ("CreatedAt");

-- Create partitions for each month
CREATE TABLE "Transactions_2025_01" PARTITION OF "Transactions_Partitioned"
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');

CREATE TABLE "Transactions_2025_02" PARTITION OF "Transactions_Partitioned"
    FOR VALUES FROM ('2025-02-01') TO ('2025-03-01');

-- Create index on each partition
CREATE INDEX ON "Transactions_2025_01"("UserId");
CREATE INDEX ON "Transactions_2025_02"("UserId");

-- Automatic partition creation (using pg_partman extension)
CREATE EXTENSION pg_partman;

SELECT partman.create_parent(
    p_parent_table := 'public."Transactions_Partitioned"',
    p_control := 'CreatedAt',
    p_type := 'native',
    p_interval := '1 month',
    p_premake := 3
);
```

## Vacuum and Analyze

### Manual Vacuum
```sql
-- Vacuum all tables
VACUUM ANALYZE;

-- Vacuum specific table
VACUUM ANALYZE "Transactions";

-- Full vacuum (locks table, use during maintenance)
VACUUM FULL "AuditLogs";
```

### Monitor Table Bloat
```sql
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size,
    n_dead_tup,
    n_live_tup,
    round(n_dead_tup * 100.0 / NULLIF(n_live_tup + n_dead_tup, 0), 2) AS dead_ratio
FROM pg_stat_user_tables
ORDER BY n_dead_tup DESC;
```

## Monitoring and Diagnostics

### Essential Monitoring Queries

#### 1. Database Size
```sql
SELECT 
    pg_database.datname,
    pg_size_pretty(pg_database_size(pg_database.datname)) AS size
FROM pg_database
ORDER BY pg_database_size(pg_database.datname) DESC;
```

#### 2. Table Sizes
```sql
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS total_size,
    pg_size_pretty(pg_relation_size(schemaname||'.'||tablename)) AS table_size,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename) - pg_relation_size(schemaname||'.'||tablename)) AS indexes_size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

#### 3. Active Connections
```sql
SELECT 
    datname,
    usename,
    application_name,
    client_addr,
    state,
    query_start,
    state_change,
    query
FROM pg_stat_activity
WHERE state != 'idle'
ORDER BY query_start;
```

#### 4. Long Running Queries
```sql
SELECT 
    pid,
    now() - query_start AS duration,
    query,
    state
FROM pg_stat_activity
WHERE state != 'idle'
  AND query NOT ILIKE '%pg_stat_activity%'
ORDER BY duration DESC;
```

#### 5. Blocking Queries
```sql
SELECT 
    blocked_locks.pid AS blocked_pid,
    blocked_activity.usename AS blocked_user,
    blocking_locks.pid AS blocking_pid,
    blocking_activity.usename AS blocking_user,
    blocked_activity.query AS blocked_statement,
    blocking_activity.query AS blocking_statement
FROM pg_catalog.pg_locks blocked_locks
JOIN pg_catalog.pg_stat_activity blocked_activity ON blocked_activity.pid = blocked_locks.pid
JOIN pg_catalog.pg_locks blocking_locks 
    ON blocking_locks.locktype = blocked_locks.locktype
    AND blocking_locks.database IS NOT DISTINCT FROM blocked_locks.database
    AND blocking_locks.relation IS NOT DISTINCT FROM blocked_locks.relation
    AND blocking_locks.page IS NOT DISTINCT FROM blocked_locks.page
    AND blocking_locks.tuple IS NOT DISTINCT FROM blocked_locks.tuple
    AND blocking_locks.virtualxid IS NOT DISTINCT FROM blocked_locks.virtualxid
    AND blocking_locks.transactionid IS NOT DISTINCT FROM blocked_locks.transactionid
    AND blocking_locks.classid IS NOT DISTINCT FROM blocked_locks.classid
    AND blocking_locks.objid IS NOT DISTINCT FROM blocked_locks.objid
    AND blocking_locks.objsubid IS NOT DISTINCT FROM blocked_locks.objsubid
    AND blocking_locks.pid != blocked_locks.pid
JOIN pg_catalog.pg_stat_activity blocking_activity ON blocking_activity.pid = blocking_locks.pid
WHERE NOT blocked_locks.granted;
```

#### 6. Cache Hit Ratio
```sql
-- Should be > 99% for optimal performance
SELECT 
    sum(heap_blks_read) as heap_read,
    sum(heap_blks_hit) as heap_hit,
    sum(heap_blks_hit) / (sum(heap_blks_hit) + sum(heap_blks_read)) * 100 AS cache_hit_ratio
FROM pg_statio_user_tables;
```

### Performance Monitoring Tools

#### 1. pg_stat_statements
```sql
-- Install extension
CREATE EXTENSION pg_stat_statements;

-- View query statistics
SELECT 
    substring(query, 1, 50) AS short_query,
    round(total_exec_time::numeric, 2) AS total_time,
    calls,
    round(mean_exec_time::numeric, 2) AS mean,
    round((100 * total_exec_time / sum(total_exec_time) OVER ())::numeric, 2) AS percentage
FROM pg_stat_statements
ORDER BY total_exec_time DESC
LIMIT 20;
```

#### 2. pgBadger (Log Analyzer)
```bash
# Install pgBadger
apt-get install pgbadger

# Generate report
pgbadger /var/log/postgresql/postgresql-16-main.log \
  -o /var/www/html/pgbadger_report.html

# Analyze last 7 days
pgbadger /var/log/postgresql/postgresql-*.log \
  --begin "2025-01-01 00:00:00" \
  --end "2025-01-07 23:59:59" \
  -o report.html
```

#### 3. pg_top (Real-time Monitoring)
```bash
# Install pg_top
apt-get install pgtop

# Run monitoring
pg_top -d SlipVerificationDb
```

## Caching Strategy

### Application-Level Caching (Redis)

#### Cache Configuration
```csharp
// Common cached queries
- User profile: TTL 5 minutes
- Order status: TTL 1 minute
- Static reference data: TTL 1 hour
- Verification results: TTL 30 seconds
```

#### Cache Invalidation
```csharp
public async Task UpdateOrderStatus(Guid orderId, string status)
{
    // Update database
    await _repository.UpdateAsync(orderId, status);
    
    // Invalidate cache
    await _cache.RemoveAsync($"order:{orderId}");
    await _cache.RemoveAsync($"user:orders:{userId}");
}
```

### Database Query Caching
```sql
-- Use prepared statements
PREPARE get_order_by_id (uuid) AS
    SELECT * FROM "Orders" WHERE "Id" = $1 AND "DeletedAt" IS NULL;

EXECUTE get_order_by_id('...');
```

## Load Testing

### pgbench (Built-in Load Testing)
```bash
# Initialize test database
pgbench -i -s 50 SlipVerificationDb

# Run benchmark (10 clients, 1000 transactions)
pgbench -c 10 -t 1000 SlipVerificationDb

# Custom benchmark script
cat > test_transactions.sql << 'EOF'
\set user_id random(1, 1000)
SELECT * FROM "Transactions" WHERE "UserId" = :user_id::text::uuid;
EOF

pgbench -c 50 -T 300 -f test_transactions.sql SlipVerificationDb
```

### K6 (Application Load Testing)
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
};

export default function () {
  let response = http.get('http://localhost:5000/api/v1/orders');
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
  sleep(1);
}
```

## Optimization Checklist

### Daily
- [ ] Monitor slow query log
- [ ] Check active connections
- [ ] Review cache hit ratios
- [ ] Monitor disk space

### Weekly
- [ ] Analyze table statistics
- [ ] Review index usage
- [ ] Check for table bloat
- [ ] Verify backup performance

### Monthly
- [ ] Full database analysis
- [ ] Index rebuild on bloated indexes
- [ ] Review and optimize slow queries
- [ ] Capacity planning review
- [ ] Performance test under load

### Quarterly
- [ ] PostgreSQL version upgrade planning
- [ ] Hardware capacity assessment
- [ ] Partitioning strategy review
- [ ] Archive old data
- [ ] Security audit

## Common Performance Issues

### Issue 1: Slow Queries
**Symptoms**: High query execution time
**Solutions**:
- Add missing indexes
- Rewrite query to use indexes
- Increase work_mem
- Use materialized views

### Issue 2: High Connection Count
**Symptoms**: Max connections reached
**Solutions**:
- Implement connection pooling (PgBouncer)
- Reduce max_connections
- Fix connection leaks in application
- Close idle connections

### Issue 3: Table Bloat
**Symptoms**: Large table size, slow queries
**Solutions**:
- Run VACUUM FULL during maintenance
- Tune autovacuum more aggressively
- Use pg_repack
- Implement table partitioning

### Issue 4: Lock Contention
**Symptoms**: Queries waiting for locks
**Solutions**:
- Use shorter transactions
- Avoid explicit table locks
- Use row-level locking
- Review query patterns

### Issue 5: I/O Bottleneck
**Symptoms**: High disk wait time
**Solutions**:
- Upgrade to faster storage (NVMe)
- Increase shared_buffers
- Optimize autovacuum settings
- Use table partitioning

## Conclusion

This performance tuning guide provides:
- Optimized PostgreSQL configuration for 1,000+ TPS
- Connection pooling setup
- Comprehensive monitoring queries
- Query optimization techniques
- Caching strategies
- Load testing procedures
- Troubleshooting guide

Regular monitoring and proactive optimization are key to maintaining high performance as the system scales.
