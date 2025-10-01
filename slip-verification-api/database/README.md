# Database Documentation

## Overview
Complete database schema and documentation for the Slip Verification System, built on PostgreSQL 16 with Entity Framework Core 9.

## Directory Structure

```
database/
├── docs/
│   ├── ERD.md                    # Entity Relationship Diagram
│   ├── INDEX_STRATEGY.md         # Indexing strategy and optimization
│   ├── BACKUP_RESTORE.md         # Backup and disaster recovery procedures
│   └── PERFORMANCE_TUNING.md     # Performance optimization guide
└── scripts/
    ├── 01_schema.sql             # Complete database schema
    └── 02_seed_data.sql          # Sample seed data
```

## Quick Start

### 1. Create Database

Using Docker Compose (Recommended):
```bash
cd /path/to/slip-verification-api
docker-compose up -d postgres
```

Manual Installation:
```bash
# Install PostgreSQL 16
sudo apt-get install postgresql-16

# Create database
sudo -u postgres psql -c "CREATE DATABASE SlipVerificationDb;"
sudo -u postgres psql -c "CREATE USER app_user WITH PASSWORD 'your_password';"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE SlipVerificationDb TO app_user;"
```

### 2. Apply Schema

#### Option A: Using Entity Framework Core Migrations (Recommended)
```bash
cd /path/to/slip-verification-api

# Install EF Core tools
dotnet tool install --global dotnet-ef

# Apply migrations
dotnet ef database update \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

#### Option B: Using SQL Scripts
```bash
# Apply schema
psql -h localhost -U postgres -d SlipVerificationDb -f database/scripts/01_schema.sql

# Apply seed data (optional)
psql -h localhost -U postgres -d SlipVerificationDb -f database/scripts/02_seed_data.sql
```

### 3. Verify Installation
```bash
psql -h localhost -U postgres -d SlipVerificationDb -c "\dt"
psql -h localhost -U postgres -d SlipVerificationDb -c "SELECT COUNT(*) FROM \"Users\";"
```

## Database Schema

### Core Tables
- **Users**: System users with authentication
- **Orders**: Payment orders awaiting verification
- **SlipVerifications**: Uploaded payment slips with OCR data
- **Transactions**: Financial transaction records
- **Notifications**: Multi-channel user notifications
- **AuditLogs**: System audit trail

### Views
- **v_daily_transaction_summary**: Daily transaction statistics
- **v_user_statistics**: Per-user activity statistics

### Functions & Triggers
- **update_updated_at_column()**: Automatic timestamp updates
- **audit_log_function()**: Audit trail tracking

## Key Features

### 1. Normalized Design (3NF)
- Eliminates data redundancy
- Ensures data integrity
- Optimized for OLTP workloads

### 2. Soft Delete Pattern
- All tables support soft delete (IsDeleted, DeletedAt)
- Enables data recovery
- Maintains referential integrity

### 3. Audit Trail
- Complete change tracking in AuditLogs
- Captures CREATE, UPDATE, DELETE operations
- Stores old and new values as JSON

### 4. Comprehensive Indexing
- Primary indexes on all tables
- Secondary indexes for common queries
- Partial indexes for soft delete patterns
- GIN indexes for JSONB columns
- See [INDEX_STRATEGY.md](docs/INDEX_STRATEGY.md) for details

### 5. JSONB Support
- Flexible metadata storage in Transactions
- Notification data storage
- Audit log value storage

## Performance Characteristics

### Design Goals
- Support 1,000+ transactions per day
- Query response time < 200ms (95th percentile)
- Authentication time < 50ms
- High availability (99.9% uptime)

### Optimization Features
- Connection pooling ready
- Prepared for table partitioning
- Autovacuum configured
- Query performance monitored

## Documentation

### 📊 [Entity Relationship Diagram](docs/ERD.md)
Detailed entity relationships, cardinality, and foreign key constraints.

### 📈 [Index Strategy](docs/INDEX_STRATEGY.md)
Complete indexing strategy including:
- Index types and usage
- Performance impact analysis
- Monitoring queries
- Maintenance procedures

### 💾 [Backup & Restore](docs/BACKUP_RESTORE.md)
Comprehensive backup and disaster recovery:
- Backup strategies (full, incremental, continuous)
- Restore procedures
- Point-in-time recovery
- Disaster recovery plan

### ⚡ [Performance Tuning](docs/PERFORMANCE_TUNING.md)
Database performance optimization:
- PostgreSQL configuration
- Connection pooling setup
- Query optimization techniques
- Monitoring and diagnostics
- Load testing procedures

## Connection Strings

### Development
```
Host=localhost;Port=5432;Database=SlipVerificationDb;Username=postgres;Password=postgres
```

### Production (with Connection Pooling)
```
Host=localhost;Port=6432;Database=SlipVerificationDb;Username=app_user;Password=***;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100
```

### Docker Compose
```
Host=postgres;Port=5432;Database=SlipVerificationDb;Username=postgres;Password=postgres
```

## Migration Management

### Create New Migration
```bash
dotnet ef migrations add MigrationName \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### Apply Migration
```bash
dotnet ef database update \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### Generate SQL Script
```bash
dotnet ef migrations script \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --output migration.sql
```

## Common Tasks

### Add New User
```sql
INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "Role", "IsActive", "EmailVerified", "CreatedAt")
VALUES (uuid_generate_v4(), 'newuser', 'newuser@example.com', 'hashed_password', 'User', true, true, NOW());
```

### Check Database Size
```sql
SELECT pg_size_pretty(pg_database_size('SlipVerificationDb'));
```

### View Active Connections
```sql
SELECT count(*) FROM pg_stat_activity WHERE datname = 'SlipVerificationDb';
```

### Find Slow Queries
```sql
SELECT query, mean_exec_time 
FROM pg_stat_statements 
ORDER BY mean_exec_time DESC 
LIMIT 10;
```

### Vacuum Database
```sql
VACUUM ANALYZE;
```

## Monitoring

### Essential Queries

#### Table Sizes
```sql
SELECT 
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

#### Index Usage
```sql
SELECT 
    tablename,
    indexname,
    idx_scan,
    pg_size_pretty(pg_relation_size(indexrelid)) AS size
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;
```

#### Cache Hit Ratio
```sql
SELECT 
    sum(heap_blks_hit) / nullif(sum(heap_blks_hit + heap_blks_read), 0) * 100 AS cache_hit_ratio
FROM pg_statio_user_tables;
```

## Security

### Best Practices
1. Use strong passwords
2. Enable SSL/TLS connections
3. Restrict network access
4. Regular security updates
5. Encrypt backups
6. Monitor access logs
7. Use least privilege principle

### User Roles
```sql
-- Create read-only user
CREATE ROLE readonly WITH LOGIN PASSWORD 'password';
GRANT CONNECT ON DATABASE SlipVerificationDb TO readonly;
GRANT USAGE ON SCHEMA public TO readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO readonly;

-- Create application user
CREATE ROLE app_user WITH LOGIN PASSWORD 'password';
GRANT CONNECT ON DATABASE SlipVerificationDb TO app_user;
GRANT USAGE ON SCHEMA public TO app_user;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app_user;
```

## Troubleshooting

### Cannot Connect to Database
```bash
# Check if PostgreSQL is running
systemctl status postgresql

# Check port
netstat -an | grep 5432

# Check configuration
psql -h localhost -U postgres -l
```

### Migration Fails
```bash
# Check current migration status
dotnet ef migrations list

# Remove last migration
dotnet ef migrations remove

# View detailed error
dotnet ef database update --verbose
```

### Performance Issues
1. Check [PERFORMANCE_TUNING.md](docs/PERFORMANCE_TUNING.md)
2. Run EXPLAIN ANALYZE on slow queries
3. Check index usage
4. Monitor system resources
5. Review PostgreSQL logs

## Support

### Resources
- [PostgreSQL Documentation](https://www.postgresql.org/docs/16/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)

### Database Team Contacts
- Database Administrator: admin@slipverification.com
- Development Team: dev@slipverification.com

## License
MIT License - See [LICENSE](../LICENSE) file for details

## Version History

### v1.0.0 (2025-01-01)
- Initial database schema
- Core tables: Users, Orders, SlipVerifications, Transactions, Notifications, AuditLogs
- Complete indexing strategy
- Backup and restore procedures
- Performance tuning guidelines
- Comprehensive documentation
