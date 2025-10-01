#!/bin/bash
#
# Database Backup Script for Slip Verification System
# Performs automated PostgreSQL backup with encryption and cloud upload
#

set -e

# Configuration
BACKUP_DIR="${BACKUP_DIR:-/backups}"
DB_HOST="${DB_HOST:-postgres}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-SlipVerificationDb}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD}"
RETENTION_DAYS="${RETENTION_DAYS:-30}"
S3_BUCKET="${S3_BUCKET}"
AZURE_STORAGE_ACCOUNT="${AZURE_STORAGE_ACCOUNT}"
AZURE_CONTAINER="${AZURE_CONTAINER:-backups}"
ENCRYPTION_KEY="${ENCRYPTION_KEY}"
SLACK_WEBHOOK="${SLACK_WEBHOOK}"

# Timestamp
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/${DB_NAME}_${TIMESTAMP}.backup"
LOG_FILE="${BACKUP_DIR}/backup_${TIMESTAMP}.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Functions
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1" | tee -a "$LOG_FILE"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" | tee -a "$LOG_FILE"
}

warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1" | tee -a "$LOG_FILE"
}

send_notification() {
    local status=$1
    local message=$2
    
    if [ -n "$SLACK_WEBHOOK" ]; then
        curl -X POST "$SLACK_WEBHOOK" \
            -H 'Content-Type: application/json' \
            -d "{\"text\":\"Database Backup ${status}: ${message}\"}" \
            2>/dev/null || true
    fi
}

# Create backup directory
mkdir -p "$BACKUP_DIR"

log "Starting database backup..."
log "Database: ${DB_NAME}"
log "Host: ${DB_HOST}:${DB_PORT}"

# Perform backup
log "Creating backup..."
export PGPASSWORD="$DB_PASSWORD"

if pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
    -F c -b -v -f "$BACKUP_FILE" 2>> "$LOG_FILE"; then
    log "✓ Backup created successfully: $BACKUP_FILE"
    
    # Get backup size
    BACKUP_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    log "Backup size: $BACKUP_SIZE"
else
    error "Backup failed!"
    send_notification "FAILED" "Database backup failed for ${DB_NAME}"
    exit 1
fi

# Compress backup
log "Compressing backup..."
if gzip "$BACKUP_FILE"; then
    BACKUP_FILE="${BACKUP_FILE}.gz"
    COMPRESSED_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
    log "✓ Backup compressed: $COMPRESSED_SIZE"
else
    warning "Compression failed, continuing with uncompressed backup"
fi

# Encrypt backup if encryption key is provided
if [ -n "$ENCRYPTION_KEY" ]; then
    log "Encrypting backup..."
    if openssl enc -aes-256-cbc -salt -pbkdf2 -in "$BACKUP_FILE" \
        -out "${BACKUP_FILE}.enc" -k "$ENCRYPTION_KEY"; then
        rm "$BACKUP_FILE"
        BACKUP_FILE="${BACKUP_FILE}.enc"
        log "✓ Backup encrypted"
    else
        warning "Encryption failed, keeping unencrypted backup"
    fi
fi

# Upload to S3 (AWS)
if [ -n "$S3_BUCKET" ]; then
    log "Uploading to S3..."
    if command -v aws &> /dev/null; then
        if aws s3 cp "$BACKUP_FILE" "s3://${S3_BUCKET}/database/" --storage-class STANDARD_IA; then
            log "✓ Backup uploaded to S3"
        else
            error "S3 upload failed"
        fi
    else
        warning "AWS CLI not found, skipping S3 upload"
    fi
fi

# Upload to Azure Blob Storage
if [ -n "$AZURE_STORAGE_ACCOUNT" ] && [ -n "$AZURE_CONTAINER" ]; then
    log "Uploading to Azure Blob Storage..."
    if command -v az &> /dev/null; then
        if az storage blob upload \
            --account-name "$AZURE_STORAGE_ACCOUNT" \
            --container-name "$AZURE_CONTAINER" \
            --name "database/$(basename $BACKUP_FILE)" \
            --file "$BACKUP_FILE" \
            --tier Cool; then
            log "✓ Backup uploaded to Azure"
        else
            error "Azure upload failed"
        fi
    else
        warning "Azure CLI not found, skipping Azure upload"
    fi
fi

# Clean up old backups
log "Cleaning up old backups (retention: ${RETENTION_DAYS} days)..."
find "$BACKUP_DIR" -name "${DB_NAME}_*.backup*" -type f -mtime +${RETENTION_DAYS} -delete
log "✓ Old backups cleaned up"

# Verify backup
log "Verifying backup integrity..."
if [ -f "$BACKUP_FILE" ]; then
    if gzip -t "$BACKUP_FILE" 2>/dev/null || file "$BACKUP_FILE" | grep -q "gzip"; then
        log "✓ Backup verification successful"
        FINAL_STATUS="SUCCESS"
    else
        error "Backup verification failed!"
        FINAL_STATUS="FAILED"
    fi
else
    error "Backup file not found!"
    FINAL_STATUS="FAILED"
fi

# Summary
log "═══════════════════════════════════════"
log "Backup Summary:"
log "  Database: ${DB_NAME}"
log "  Timestamp: ${TIMESTAMP}"
log "  File: $(basename $BACKUP_FILE)"
log "  Size: ${COMPRESSED_SIZE:-$BACKUP_SIZE}"
log "  Status: ${FINAL_STATUS}"
log "═══════════════════════════════════════"

# Send notification
send_notification "$FINAL_STATUS" "Database: ${DB_NAME}, Size: ${COMPRESSED_SIZE:-$BACKUP_SIZE}"

# Exit with appropriate code
if [ "$FINAL_STATUS" = "SUCCESS" ]; then
    log "✓ Backup completed successfully!"
    exit 0
else
    error "✗ Backup completed with errors!"
    exit 1
fi
