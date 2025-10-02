#!/bin/bash
#
# Database Restore Script for Slip Verification System
# Restores PostgreSQL database from backup
#

set -e

# Configuration
BACKUP_FILE="$1"
DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-SlipVerificationDb}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD}"
ENCRYPTION_KEY="${ENCRYPTION_KEY}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1"
}

warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

# Validate input
if [ -z "$BACKUP_FILE" ]; then
    error "Usage: $0 <backup-file>"
    error "Example: $0 /backups/SlipVerificationDb_20240101_120000.backup.gz"
    exit 1
fi

if [ ! -f "$BACKUP_FILE" ]; then
    error "Backup file not found: $BACKUP_FILE"
    exit 1
fi

log "═══════════════════════════════════════"
log "Database Restore"
log "═══════════════════════════════════════"
log "Backup file: $BACKUP_FILE"
log "Database: ${DB_NAME}"
log "Host: ${DB_HOST}:${DB_PORT}"
log "═══════════════════════════════════════"

# Confirm restore
read -p "⚠️  This will REPLACE the current database. Are you sure? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
    log "Restore cancelled."
    exit 0
fi

# Temporary directory for restore
TEMP_DIR=$(mktemp -d)
RESTORE_FILE="$TEMP_DIR/restore.backup"

# Decrypt if needed
if [[ "$BACKUP_FILE" == *.enc ]]; then
    if [ -z "$ENCRYPTION_KEY" ]; then
        error "Backup is encrypted but ENCRYPTION_KEY not provided"
        rm -rf "$TEMP_DIR"
        exit 1
    fi
    
    log "Decrypting backup..."
    if openssl enc -aes-256-cbc -d -pbkdf2 -in "$BACKUP_FILE" -out "$RESTORE_FILE.gz" -k "$ENCRYPTION_KEY"; then
        log "✓ Backup decrypted"
        BACKUP_FILE="$RESTORE_FILE.gz"
    else
        error "Decryption failed"
        rm -rf "$TEMP_DIR"
        exit 1
    fi
fi

# Decompress if needed
if [[ "$BACKUP_FILE" == *.gz ]]; then
    log "Decompressing backup..."
    if gunzip -c "$BACKUP_FILE" > "$RESTORE_FILE"; then
        log "✓ Backup decompressed"
    else
        error "Decompression failed"
        rm -rf "$TEMP_DIR"
        exit 1
    fi
else
    cp "$BACKUP_FILE" "$RESTORE_FILE"
fi

# Set database password
export PGPASSWORD="$DB_PASSWORD"

# Test database connection
log "Testing database connection..."
if pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" > /dev/null 2>&1; then
    log "✓ Database connection successful"
else
    error "Cannot connect to database"
    rm -rf "$TEMP_DIR"
    exit 1
fi

# Create backup of current database
CURRENT_BACKUP="$TEMP_DIR/current_backup_$(date +%Y%m%d_%H%M%S).backup"
log "Creating safety backup of current database..."
if pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -F c -f "$CURRENT_BACKUP" 2>/dev/null; then
    log "✓ Safety backup created: $CURRENT_BACKUP"
else
    warning "Could not create safety backup (database may not exist)"
fi

# Terminate active connections
log "Terminating active connections..."
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c \
    "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '${DB_NAME}' AND pid <> pg_backend_pid();" \
    > /dev/null 2>&1 || true

# Drop and recreate database
log "Dropping existing database..."
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS ${DB_NAME};" > /dev/null 2>&1

log "Creating new database..."
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "CREATE DATABASE ${DB_NAME};" > /dev/null 2>&1

# Restore database
log "Restoring database from backup..."
if pg_restore -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
    -v "$RESTORE_FILE" 2>&1 | grep -i "processing\|creating\|restoring" | head -20; then
    log "✓ Database restored successfully"
else
    error "Restore failed!"
    
    # Attempt to restore safety backup
    if [ -f "$CURRENT_BACKUP" ]; then
        warning "Attempting to restore from safety backup..."
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "DROP DATABASE IF EXISTS ${DB_NAME};" > /dev/null 2>&1
        psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -c "CREATE DATABASE ${DB_NAME};" > /dev/null 2>&1
        pg_restore -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" "$CURRENT_BACKUP" > /dev/null 2>&1
        log "✓ Restored from safety backup"
    fi
    
    rm -rf "$TEMP_DIR"
    exit 1
fi

# Verify restore
log "Verifying restore..."
TABLE_COUNT=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -t -c \
    "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';" | tr -d ' ')

log "Number of tables restored: $TABLE_COUNT"

if [ "$TABLE_COUNT" -gt 0 ]; then
    log "✓ Restore verification successful"
else
    warning "No tables found in restored database"
fi

# Cleanup
log "Cleaning up temporary files..."
rm -rf "$TEMP_DIR"

log "═══════════════════════════════════════"
log "✓ Database restore completed successfully!"
log "═══════════════════════════════════════"

exit 0
