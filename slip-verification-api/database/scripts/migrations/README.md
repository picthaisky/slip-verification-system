# Database Migration Scripts

This directory contains scripts and utilities for managing database migrations in the Slip Verification System.

## Overview

The Slip Verification System uses **Entity Framework Core 9** for database migrations with PostgreSQL 16. This directory provides production-ready scripts for applying, testing, and rolling back migrations safely.

## Directory Structure

```
migrations/
├── production-migration.sh      # Production migration script with backup/rollback
├── test-migrations.sh           # Test migrations in safe environment
├── rollback-migration.sh        # Rollback migrations or restore from backup
├── generate-sql-script.sh       # Generate SQL scripts from migrations
├── examples/                    # Example migration classes
│   ├── SeedInitialData.cs      # Seed data migration example
│   ├── AddFullTextSearch.cs    # Full-text search implementation
│   ├── AddAuditTriggers.cs     # Audit trail with triggers
│   └── MigrationTests.cs       # Migration testing examples
└── README.md                    # This file
```

## Quick Start

### 1. Prerequisites

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

### 2. Test Migrations Locally

```bash
# Test all migrations in a safe test environment
./test-migrations.sh

# This will:
# - Create a temporary test database
# - Apply all migrations
# - Verify schema
# - Test rollback
# - Reapply migrations
# - Clean up test database
```

### 3. Generate SQL Script (Review First)

```bash
# Generate SQL script for all migrations
./generate-sql-script.sh

# Generate incremental script
./generate-sql-script.sh --from InitialCreate --to AddSecurityEnhancements

# Generate idempotent script (safe to run multiple times)
./generate-sql-script.sh --idempotent --output production-migration.sql
```

### 4. Apply to Production

```bash
# Run production migration with automatic backup
./production-migration.sh \
  --host prod-db.example.com \
  --database SlipVerificationDb \
  --user app_user

# Dry run (generate script without applying)
./production-migration.sh --dry-run
```

## Scripts Documentation

### production-migration.sh

**Purpose**: Safely apply migrations to production database with backup and rollback capabilities.

**Features**:
- Automatic database backup before migration
- Connection testing
- Migration validation
- Automatic rollback on failure
- Detailed logging

**Usage**:
```bash
./production-migration.sh [OPTIONS]

Options:
  -h, --host         Database host (default: localhost)
  -p, --port         Database port (default: 5432)
  -d, --database     Database name
  -u, --user         Database user
  -w, --password     Database password
  -b, --backup-dir   Backup directory (default: /backups)
  -s, --skip-backup  Skip backup (NOT RECOMMENDED)
  --dry-run          Generate SQL without applying
```

**Example**:
```bash
# Production migration with backup
./production-migration.sh \
  -h prod-db.example.com \
  -d SlipVerificationDb \
  -u postgres \
  -b /var/backups/postgres

# Dry run to review changes
./production-migration.sh --dry-run
```

### test-migrations.sh

**Purpose**: Test migrations in a safe isolated environment.

**What it does**:
1. Creates temporary test database
2. Applies all migrations (UP)
3. Verifies database schema
4. Tests rollback (DOWN)
5. Reapplies migrations
6. Cleans up test database

**Usage**:
```bash
./test-migrations.sh

# With custom database settings
DB_HOST=localhost DB_USER=postgres DB_PASSWORD=postgres ./test-migrations.sh
```

### rollback-migration.sh

**Purpose**: Rollback migrations or restore from backup.

**Usage**:
```bash
# Rollback to specific migration
./rollback-migration.sh InitialCreate

# Rollback with database details
./rollback-migration.sh \
  -h prod-db.example.com \
  -d SlipVerificationDb \
  -u postgres \
  AddSecurityEnhancements

# Restore from backup file
./rollback-migration.sh --backup /backups/backup_20240101_120000.backup.gz
```

### generate-sql-script.sh

**Purpose**: Generate SQL scripts from EF Core migrations for review or manual execution.

**Usage**:
```bash
# Generate script for all migrations
./generate-sql-script.sh

# Generate incremental script
./generate-sql-script.sh \
  --from InitialCreate \
  --to AddSecurityEnhancements

# Generate idempotent script
./generate-sql-script.sh \
  --idempotent \
  --output production.sql

# Custom output file
./generate-sql-script.sh \
  --output /tmp/my-migration.sql
```

## Migration Workflow

### Development Environment

```bash
# 1. Make changes to entity models
# 2. Create new migration
cd /path/to/slip-verification-api
dotnet ef migrations add MyNewMigration \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# 3. Review generated migration
# 4. Test migration
./database/scripts/migrations/test-migrations.sh

# 5. Apply to development database
dotnet ef database update \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### Staging Environment

```bash
# 1. Generate SQL script
./generate-sql-script.sh --idempotent --output staging-migration.sql

# 2. Review the script
cat staging-migration.sql

# 3. Apply to staging
./production-migration.sh \
  -h staging-db.example.com \
  -d SlipVerificationDb \
  -u postgres

# 4. Test application thoroughly
# 5. Verify data integrity
```

### Production Environment

```bash
# 1. Schedule maintenance window
# 2. Notify stakeholders
# 3. Create backup manually (extra safety)
pg_dump -h prod-db -U postgres -d SlipVerificationDb -F c -f manual_backup.dump

# 4. Apply migration (includes automatic backup)
./production-migration.sh \
  -h prod-db.example.com \
  -d SlipVerificationDb \
  -u app_user

# 5. Verify migration
psql -h prod-db -U postgres -d SlipVerificationDb \
  -c "SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 5;"

# 6. Monitor application
# 7. Verify critical functionality
```

## Rollback Procedures

### Scenario 1: Migration Failed (Automatic Rollback)

The production migration script automatically rolls back if migration fails and backup exists.

### Scenario 2: Manual Rollback to Previous Migration

```bash
# List current migrations
dotnet ef migrations list

# Rollback to previous migration
./rollback-migration.sh PreviousMigrationName
```

### Scenario 3: Restore from Backup

```bash
# If migration succeeded but application has issues
./rollback-migration.sh --backup /backups/backup_before_migration.backup.gz
```

## Best Practices

### Before Migration

- [ ] Test in development environment
- [ ] Test in staging environment  
- [ ] Review generated SQL scripts
- [ ] Create database backup
- [ ] Schedule maintenance window
- [ ] Notify stakeholders
- [ ] Document expected changes
- [ ] Prepare rollback plan

### During Migration

- [ ] Put application in maintenance mode
- [ ] Monitor migration progress
- [ ] Watch for errors or warnings
- [ ] Check disk space
- [ ] Monitor database connections
- [ ] Keep rollback scripts ready

### After Migration

- [ ] Verify migration success
- [ ] Check migration history
- [ ] Test critical functionality
- [ ] Verify data integrity
- [ ] Monitor performance
- [ ] Update documentation
- [ ] Take application out of maintenance mode
- [ ] Monitor for issues

## Common Issues and Solutions

### Issue: Migration takes too long

**Solution**:
1. Review migration script for expensive operations
2. Consider breaking into smaller migrations
3. Create indexes after data load
4. Use batch operations for data changes

### Issue: Cannot connect to database

**Solution**:
```bash
# Check if PostgreSQL is running
systemctl status postgresql

# Test connection
psql -h localhost -U postgres -d SlipVerificationDb -c "SELECT 1;"

# Check firewall rules
```

### Issue: Migration fails midway

**Solution**:
The production script automatically rolls back. If manual intervention needed:
```bash
# Check current state
dotnet ef migrations list

# Restore from backup
./rollback-migration.sh --backup /backups/latest.backup.gz
```

### Issue: EF Core tools not found

**Solution**:
```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Add to PATH if needed
export PATH="$PATH:$HOME/.dotnet/tools"
```

## Example Migrations

The `examples/` directory contains reference implementations:

### SeedInitialData.cs
- Seeds default users (admin, manager, user)
- Seeds notification templates
- Demonstrates data seeding with migrations

### AddFullTextSearch.cs
- Adds full-text search to Orders and SlipVerifications
- Creates PostgreSQL functions and triggers
- Implements GIN indexes
- Includes usage examples

### AddAuditTriggers.cs
- Implements automatic audit logging
- Creates audit functions and triggers
- Tracks INSERT, UPDATE, DELETE operations
- Includes helper functions and views

### MigrationTests.cs
- Unit tests for migrations
- Tests schema creation
- Tests rollback functionality
- Verifies indexes and constraints

## Environment Variables

The scripts support environment variables for configuration:

```bash
# Database connection
export DB_HOST="localhost"
export DB_PORT="5432"
export DB_NAME="SlipVerificationDb"
export DB_USER="postgres"
export DB_PASSWORD="your_password"
export BACKUP_DIR="/backups"

# Then run scripts without parameters
./production-migration.sh
```

## Logging

All scripts create log files:
- Location: `$BACKUP_DIR/migration_<timestamp>.log`
- Contains: All output, errors, and execution details
- Retention: Keep for audit purposes

## Security Considerations

1. **Never commit passwords**: Use environment variables or prompt for passwords
2. **Secure backup files**: Restrict access to backup directory
3. **Encrypt backups**: Use encryption for sensitive data
4. **Audit trail**: Keep logs of all migration activities
5. **Access control**: Limit who can run production migrations

## Support

### Documentation
- [Main Migration Strategy Guide](../docs/MIGRATION_STRATEGY.md)
- [Database README](../README.md)
- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

### Contacts
- Database Team: admin@slipverification.com
- DevOps Team: devops@slipverification.com

## Version History

### v1.0.0 (2024-01-01)
- Initial migration scripts
- Production migration with backup
- Test migration script
- Rollback script
- SQL generation script
- Example migrations
- Comprehensive documentation
