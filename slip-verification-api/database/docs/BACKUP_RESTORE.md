# Database Backup & Restore Strategy

## Overview
This document outlines the backup and restore strategy for the Slip Verification System PostgreSQL database.

## Backup Strategy

### 1. Backup Types

#### Full Backup (Daily)
Complete database backup including all data, schema, and configurations.
- **Frequency**: Daily at 2:00 AM
- **Retention**: 30 days
- **Storage**: Off-site cloud storage (AWS S3, Azure Blob, or similar)
- **Compression**: Enabled (gzip)
- **Encryption**: AES-256

#### Incremental Backup (Hourly)
Backup of changes since last full backup.
- **Frequency**: Every hour
- **Retention**: 7 days
- **Storage**: Local storage + cloud replication
- **Method**: WAL (Write-Ahead Logging) archiving

#### Transaction Log Backup (Continuous)
Continuous backup of transaction logs.
- **Frequency**: Real-time
- **Retention**: 7 days
- **Storage**: Local + cloud
- **Method**: WAL streaming

### 2. Backup Tools

#### pg_dump (Logical Backup)
Best for: Full database dumps, schema-only backups

```bash
# Full database backup
pg_dump -h localhost -U postgres -d SlipVerificationDb \
  -F c -b -v -f "/backups/full_backup_$(date +%Y%m%d_%H%M%S).backup"

# Schema only backup
pg_dump -h localhost -U postgres -d SlipVerificationDb \
  --schema-only -F p -f "/backups/schema_$(date +%Y%m%d).sql"

# Data only backup
pg_dump -h localhost -U postgres -d SlipVerificationDb \
  --data-only -F c -f "/backups/data_$(date +%Y%m%d).backup"

# Specific table backup
pg_dump -h localhost -U postgres -d SlipVerificationDb \
  -t "Transactions" -F c -f "/backups/transactions_$(date +%Y%m%d).backup"
```

#### pg_basebackup (Physical Backup)
Best for: Point-in-time recovery, replication

```bash
# Base backup for PITR
pg_basebackup -h localhost -U postgres \
  -D /backups/base_backup_$(date +%Y%m%d) \
  -F tar -z -P -v
```

#### WAL Archiving
Continuous archiving for point-in-time recovery.

**Configure in postgresql.conf:**
```conf
wal_level = replica
archive_mode = on
archive_command = 'cp %p /backups/wal_archive/%f'
max_wal_senders = 3
wal_keep_size = 1GB
```

### 3. Backup Scripts

#### Daily Full Backup Script
```bash
#!/bin/bash
# File: /scripts/backup_daily.sh

# Configuration
DB_NAME="SlipVerificationDb"
DB_USER="postgres"
DB_HOST="localhost"
BACKUP_DIR="/backups/daily"
S3_BUCKET="s3://your-backup-bucket/database"
RETENTION_DAYS=30

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup filename with timestamp
BACKUP_FILE="$BACKUP_DIR/${DB_NAME}_full_$(date +%Y%m%d_%H%M%S).backup"

# Perform backup
echo "Starting full backup at $(date)"
pg_dump -h $DB_HOST -U $DB_USER -d $DB_NAME \
  -F c -b -v -f "$BACKUP_FILE"

# Compress backup
echo "Compressing backup..."
gzip "$BACKUP_FILE"
BACKUP_FILE="${BACKUP_FILE}.gz"

# Encrypt backup
echo "Encrypting backup..."
openssl enc -aes-256-cbc -salt -pbkdf2 \
  -in "$BACKUP_FILE" \
  -out "${BACKUP_FILE}.enc" \
  -pass file:/secrets/backup_password.txt

# Upload to cloud storage
echo "Uploading to cloud..."
aws s3 cp "${BACKUP_FILE}.enc" "$S3_BUCKET/"

# Clean up local old backups
echo "Cleaning up old backups..."
find $BACKUP_DIR -name "*.backup.gz.enc" -mtime +$RETENTION_DAYS -delete

# Verify backup
echo "Verifying backup integrity..."
pg_restore --list "${BACKUP_FILE}" > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "Backup completed successfully at $(date)"
else
    echo "ERROR: Backup verification failed!" >&2
    exit 1
fi
```

#### Hourly Incremental Backup Script
```bash
#!/bin/bash
# File: /scripts/backup_incremental.sh

# Configuration
BACKUP_DIR="/backups/incremental"
WAL_ARCHIVE="/backups/wal_archive"
S3_BUCKET="s3://your-backup-bucket/wal"

# Archive WAL files
rsync -av $WAL_ARCHIVE/ $BACKUP_DIR/wal_$(date +%Y%m%d_%H)/

# Upload to cloud
aws s3 sync $BACKUP_DIR $S3_BUCKET

# Log completion
echo "Incremental backup completed at $(date)" >> /var/log/postgres_backup.log
```

### 4. Automated Backup Schedule

#### Cron Configuration
```cron
# Edit crontab: crontab -e

# Daily full backup at 2:00 AM
0 2 * * * /scripts/backup_daily.sh >> /var/log/backup_daily.log 2>&1

# Hourly incremental backup
0 * * * * /scripts/backup_incremental.sh >> /var/log/backup_incremental.log 2>&1

# Weekly backup verification
0 3 * * 0 /scripts/verify_backups.sh >> /var/log/backup_verify.log 2>&1
```

#### Systemd Timer (Alternative)
```ini
# File: /etc/systemd/system/postgres-backup.timer
[Unit]
Description=PostgreSQL Daily Backup Timer

[Timer]
OnCalendar=daily
OnCalendar=02:00
Persistent=true

[Install]
WantedBy=timers.target
```

## Restore Procedures

### 1. Full Database Restore

#### From pg_dump Backup
```bash
# Stop application services
sudo systemctl stop slipverification-api

# Drop existing database (CAUTION!)
psql -h localhost -U postgres -c "DROP DATABASE IF EXISTS SlipVerificationDb;"

# Create new database
psql -h localhost -U postgres -c "CREATE DATABASE SlipVerificationDb;"

# Decrypt backup
openssl enc -aes-256-cbc -d -pbkdf2 \
  -in /backups/daily/backup.backup.gz.enc \
  -out /backups/daily/backup.backup.gz \
  -pass file:/secrets/backup_password.txt

# Decompress backup
gunzip /backups/daily/backup.backup.gz

# Restore from backup
pg_restore -h localhost -U postgres -d SlipVerificationDb \
  -v /backups/daily/backup.backup

# Verify restore
psql -h localhost -U postgres -d SlipVerificationDb \
  -c "SELECT COUNT(*) FROM \"Users\";"

# Restart application
sudo systemctl start slipverification-api
```

### 2. Point-in-Time Recovery (PITR)

#### Setup for PITR
```bash
# 1. Stop PostgreSQL
sudo systemctl stop postgresql

# 2. Backup current data directory
mv /var/lib/postgresql/16/main /var/lib/postgresql/16/main.old

# 3. Restore base backup
tar -xzf /backups/base_backup_20250101.tar.gz \
  -C /var/lib/postgresql/16/main

# 4. Create recovery configuration
cat > /var/lib/postgresql/16/main/recovery.signal << EOF
restore_command = 'cp /backups/wal_archive/%f %p'
recovery_target_time = '2025-01-01 14:30:00'
EOF

# 5. Start PostgreSQL (will enter recovery mode)
sudo systemctl start postgresql

# 6. Monitor recovery
tail -f /var/log/postgresql/postgresql-16-main.log

# 7. Verify recovery
psql -h localhost -U postgres -d SlipVerificationDb \
  -c "SELECT pg_is_in_recovery();"
```

### 3. Selective Table Restore

```bash
# Restore specific table only
pg_restore -h localhost -U postgres -d SlipVerificationDb \
  -t "Transactions" \
  -v /backups/full_backup.backup

# Or restore using SQL dump
psql -h localhost -U postgres -d SlipVerificationDb \
  < /backups/transactions_table.sql
```

### 4. Schema-Only Restore

```bash
# Restore schema only (no data)
pg_restore -h localhost -U postgres -d SlipVerificationDb \
  --schema-only -v /backups/full_backup.backup
```

## Disaster Recovery Plan

### RTO (Recovery Time Objective)
- **Target**: < 4 hours
- **Maximum acceptable downtime**: 8 hours

### RPO (Recovery Point Objective)
- **Target**: < 1 hour
- **Maximum data loss**: 4 hours

### Recovery Steps

#### 1. Assess Situation
```bash
# Check database status
systemctl status postgresql
psql -h localhost -U postgres -l

# Check last backup
ls -lh /backups/daily/ | tail -5
aws s3 ls $S3_BUCKET/ | tail -5

# Review logs
tail -100 /var/log/postgresql/postgresql-16-main.log
```

#### 2. Initiate Recovery
```bash
# Download latest backup from cloud
aws s3 cp $S3_BUCKET/latest_backup.backup.gz.enc /tmp/

# Follow restore procedure
/scripts/restore_database.sh /tmp/latest_backup.backup.gz.enc
```

#### 3. Validate Recovery
```bash
# Run validation queries
psql -h localhost -U postgres -d SlipVerificationDb << EOF
SELECT COUNT(*) FROM "Users";
SELECT COUNT(*) FROM "Orders";
SELECT COUNT(*) FROM "Transactions";
SELECT MAX("CreatedAt") FROM "Transactions";
EOF

# Check application connectivity
curl http://localhost:5000/health
```

#### 4. Resume Operations
```bash
# Start application services
sudo systemctl start slipverification-api

# Monitor for errors
journalctl -u slipverification-api -f
```

## Backup Verification

### Automated Verification Script
```bash
#!/bin/bash
# File: /scripts/verify_backups.sh

BACKUP_FILE="/backups/daily/latest_backup.backup"
TEST_DB="SlipVerificationDb_Test"

echo "Starting backup verification at $(date)"

# Create test database
psql -h localhost -U postgres -c "DROP DATABASE IF EXISTS $TEST_DB;"
psql -h localhost -U postgres -c "CREATE DATABASE $TEST_DB;"

# Restore to test database
pg_restore -h localhost -U postgres -d $TEST_DB \
  -v "$BACKUP_FILE" 2>&1 | tee /tmp/restore_test.log

# Verify data integrity
USERS_COUNT=$(psql -h localhost -U postgres -d $TEST_DB -t -c "SELECT COUNT(*) FROM \"Users\";")
ORDERS_COUNT=$(psql -h localhost -U postgres -d $TEST_DB -t -c "SELECT COUNT(*) FROM \"Orders\";")

echo "Users: $USERS_COUNT"
echo "Orders: $ORDERS_COUNT"

# Clean up test database
psql -h localhost -U postgres -c "DROP DATABASE $TEST_DB;"

# Alert if counts are suspiciously low
if [ "$USERS_COUNT" -lt 10 ]; then
    echo "WARNING: User count is suspiciously low!" >&2
    # Send alert email/notification
fi

echo "Backup verification completed at $(date)"
```

### Manual Verification Checklist
- [ ] Backup file size is reasonable (not too small/large)
- [ ] Backup file is not corrupted (pg_restore --list succeeds)
- [ ] All critical tables are present
- [ ] Row counts match expected values
- [ ] Latest records are included
- [ ] Foreign key relationships are intact
- [ ] Indexes are created correctly

## Cloud Storage Configuration

### AWS S3
```bash
# Configure AWS CLI
aws configure

# Create bucket
aws s3 mb s3://slipverification-backups --region us-west-2

# Enable versioning
aws s3api put-bucket-versioning \
  --bucket slipverification-backups \
  --versioning-configuration Status=Enabled

# Configure lifecycle policy
aws s3api put-bucket-lifecycle-configuration \
  --bucket slipverification-backups \
  --lifecycle-configuration file://s3-lifecycle.json
```

**s3-lifecycle.json:**
```json
{
  "Rules": [
    {
      "Id": "ArchiveOldBackups",
      "Status": "Enabled",
      "Transitions": [
        {
          "Days": 30,
          "StorageClass": "GLACIER"
        }
      ],
      "Expiration": {
        "Days": 365
      }
    }
  ]
}
```

### Azure Blob Storage
```bash
# Create storage account
az storage account create \
  --name slipverificationbackups \
  --resource-group BackupResourceGroup \
  --location westus \
  --sku Standard_LRS

# Create container
az storage container create \
  --name database-backups \
  --account-name slipverificationbackups
```

## Monitoring & Alerts

### Backup Monitoring Script
```bash
#!/bin/bash
# File: /scripts/monitor_backups.sh

# Configuration
MAX_AGE_HOURS=24
BACKUP_DIR="/backups/daily"
ALERT_EMAIL="admin@slipverification.com"

# Find latest backup
LATEST_BACKUP=$(ls -t $BACKUP_DIR/*.backup.gz.enc | head -1)
BACKUP_AGE=$(( ($(date +%s) - $(stat -c %Y "$LATEST_BACKUP")) / 3600 ))

# Check backup age
if [ $BACKUP_AGE -gt $MAX_AGE_HOURS ]; then
    echo "ALERT: Latest backup is $BACKUP_AGE hours old!" | \
    mail -s "Backup Alert: Old Backup" $ALERT_EMAIL
fi

# Check backup size
BACKUP_SIZE=$(stat -c %s "$LATEST_BACKUP")
MIN_SIZE=$((100 * 1024 * 1024))  # 100 MB

if [ $BACKUP_SIZE -lt $MIN_SIZE ]; then
    echo "ALERT: Latest backup size is only $BACKUP_SIZE bytes!" | \
    mail -s "Backup Alert: Small Backup" $ALERT_EMAIL
fi
```

### Metrics to Monitor
- Backup completion time
- Backup file size trends
- Failed backup attempts
- Storage space utilization
- Restore test success rate

## Security Considerations

### 1. Encryption
- All backups encrypted at rest
- Use strong encryption keys (AES-256)
- Rotate encryption keys annually
- Store keys securely (AWS KMS, Azure Key Vault)

### 2. Access Control
```bash
# Set proper permissions on backup files
chmod 600 /backups/daily/*.backup.gz.enc
chown postgres:postgres /backups/daily/*.backup.gz.enc

# Restrict directory access
chmod 700 /backups
```

### 3. Network Security
- Use VPN for backup transfers
- Enable SSL/TLS for PostgreSQL connections
- Restrict backup server access by IP

## Best Practices

### 1. Testing
- Test restore procedures monthly
- Maintain restore documentation
- Time restore operations
- Document any issues encountered

### 2. Documentation
- Keep backup logs
- Document configuration changes
- Maintain recovery runbooks
- Update contact information

### 3. Compliance
- Follow data retention policies
- Maintain audit trail
- Encrypt sensitive data
- Regular security audits

## Troubleshooting

### Common Issues

#### 1. Backup Too Large
```bash
# Compress more aggressively
pg_dump ... | gzip -9 > backup.sql.gz

# Exclude large tables
pg_dump ... --exclude-table="AuditLogs" > backup.sql
```

#### 2. Restore Fails
```bash
# Check PostgreSQL version compatibility
pg_restore --version

# Restore with verbose output
pg_restore -v -d dbname backup.backup 2>&1 | tee restore.log

# Restore in single transaction
pg_restore -1 -d dbname backup.backup
```

#### 3. Out of Disk Space
```bash
# Check disk usage
df -h /backups

# Clean old backups manually
find /backups -name "*.backup.gz" -mtime +30 -delete

# Compress existing backups
gzip /backups/*.backup
```

## Conclusion

This backup and restore strategy provides:
- Multiple backup layers (full, incremental, continuous)
- Quick recovery capabilities (< 4 hours RTO)
- Minimal data loss (< 1 hour RPO)
- Automated verification
- Cloud storage redundancy
- Security through encryption
- Comprehensive monitoring

Regular testing and updates to this strategy are essential for maintaining effective disaster recovery capabilities.
