#!/bin/bash
################################################################################
# Migration Rollback Script
# 
# This script performs a safe rollback of database migrations
#
# Usage:
#   ./rollback-migration.sh [OPTIONS] <target-migration>
#
# Options:
#   -h, --host         Database host (default: localhost)
#   -p, --port         Database port (default: 5432)
#   -d, --database     Database name (default: SlipVerificationDb)
#   -u, --user         Database user (default: postgres)
#   -w, --password     Database password
#   -b, --backup       Backup file to restore from (optional)
#   --help             Show this help message
#
# Example:
#   ./rollback-migration.sh -h prod-db.example.com InitialCreate
#   ./rollback-migration.sh -b /backups/backup_20240101_120000.backup.gz
#
################################################################################

set -e

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Default configuration
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-SlipVerificationDb}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-}"
BACKUP_FILE=""
TARGET_MIGRATION=""
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

show_help() {
    head -n 25 "$0" | grep "^#" | sed 's/^# //;s/^#//'
    exit 0
}

parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            -h|--host)
                DB_HOST="$2"
                shift 2
                ;;
            -p|--port)
                DB_PORT="$2"
                shift 2
                ;;
            -d|--database)
                DB_NAME="$2"
                shift 2
                ;;
            -u|--user)
                DB_USER="$2"
                shift 2
                ;;
            -w|--password)
                DB_PASSWORD="$2"
                shift 2
                ;;
            -b|--backup)
                BACKUP_FILE="$2"
                shift 2
                ;;
            --help)
                show_help
                ;;
            *)
                if [ -z "$TARGET_MIGRATION" ]; then
                    TARGET_MIGRATION="$1"
                    shift
                else
                    print_error "Unknown option: $1"
                    show_help
                fi
                ;;
        esac
    done
}

# List available migrations
list_migrations() {
    print_info "Listing available migrations..."
    
    cd "$PROJECT_ROOT"
    
    dotnet ef migrations list \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --no-build
}

# Get current migration
get_current_migration() {
    cd "$PROJECT_ROOT"
    
    local conn_str="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER}"
    if [ -n "$DB_PASSWORD" ]; then
        conn_str="${conn_str};Password=${DB_PASSWORD}"
    fi
    
    export PGPASSWORD="$DB_PASSWORD"
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        -t -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 1;" 2>/dev/null | xargs || echo ""
}

# Rollback using migration name
rollback_to_migration() {
    local target=$1
    
    print_warning "Rolling back to migration: $target"
    print_warning "This action will revert database changes!"
    
    read -p "Are you sure you want to continue? (yes/no): " -r
    echo
    if [[ ! $REPLY =~ ^[Yy]es$ ]]; then
        print_info "Rollback cancelled by user"
        exit 0
    fi
    
    cd "$PROJECT_ROOT"
    
    local conn_str="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER}"
    if [ -n "$DB_PASSWORD" ]; then
        conn_str="${conn_str};Password=${DB_PASSWORD}"
    fi
    
    print_info "Executing rollback..."
    
    dotnet ef database update "$target" \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --connection "$conn_str" \
        --no-build \
        --verbose
    
    if [ $? -eq 0 ]; then
        print_success "Rollback completed successfully!"
        return 0
    else
        print_error "Rollback failed!"
        return 1
    fi
}

# Restore from backup
restore_from_backup() {
    local backup=$1
    
    if [ ! -f "$backup" ]; then
        print_error "Backup file not found: $backup"
        exit 1
    fi
    
    print_warning "Restoring database from backup: $backup"
    print_warning "This will REPLACE the current database!"
    
    read -p "Are you sure you want to continue? (yes/no): " -r
    echo
    if [[ ! $REPLY =~ ^[Yy]es$ ]]; then
        print_info "Restore cancelled by user"
        exit 0
    fi
    
    export PGPASSWORD="$DB_PASSWORD"
    
    # Decompress if gzipped
    local restore_file="$backup"
    if [[ "$backup" == *.gz ]]; then
        print_info "Decompressing backup..."
        restore_file="${backup%.gz}"
        gunzip -c "$backup" > "$restore_file"
    fi
    
    print_info "Restoring database..."
    
    # Terminate existing connections
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres \
        -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$DB_NAME' AND pid <> pg_backend_pid();" 2>/dev/null
    
    # Restore database
    pg_restore -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "$restore_file"
    
    if [ $? -eq 0 ]; then
        print_success "Database restored successfully!"
        
        # Clean up decompressed file if we created it
        if [[ "$backup" == *.gz ]] && [ -f "$restore_file" ]; then
            rm -f "$restore_file"
        fi
        
        return 0
    else
        print_error "Database restore failed!"
        return 1
    fi
}

# Verify rollback
verify_rollback() {
    print_info "Verifying rollback..."
    
    export PGPASSWORD="$DB_PASSWORD"
    
    # Check migration history
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        -c "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 5;"
    
    if [ $? -eq 0 ]; then
        print_success "Rollback verification successful"
        return 0
    else
        print_error "Rollback verification failed"
        return 1
    fi
}

main() {
    parse_args "$@"
    
    echo ""
    echo "================================================================================"
    echo "                      Migration Rollback Script"
    echo "================================================================================"
    echo ""
    
    # Check if backup restore or migration rollback
    if [ -n "$BACKUP_FILE" ]; then
        # Restore from backup
        restore_from_backup "$BACKUP_FILE"
    elif [ -n "$TARGET_MIGRATION" ]; then
        # Get current migration
        current=$(get_current_migration)
        print_info "Current migration: ${current:-None}"
        
        # List available migrations
        list_migrations
        echo ""
        
        # Rollback to target migration
        rollback_to_migration "$TARGET_MIGRATION"
        
        # Verify rollback
        verify_rollback
    else
        # No target specified, show help
        print_error "No target migration or backup file specified!"
        echo ""
        
        # Show current state
        current=$(get_current_migration)
        print_info "Current migration: ${current:-None}"
        echo ""
        
        # List migrations
        list_migrations
        echo ""
        
        print_info "Usage:"
        print_info "  Rollback to specific migration: $0 <migration-name>"
        print_info "  Restore from backup: $0 --backup <backup-file>"
        echo ""
        exit 1
    fi
    
    echo ""
    echo "================================================================================"
    print_success "Rollback operation completed"
    echo "================================================================================"
    echo ""
}

main "$@"
