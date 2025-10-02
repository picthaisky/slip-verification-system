#!/bin/bash
################################################################################
# Production Database Migration Script
# 
# This script safely applies Entity Framework Core migrations to production
# database with backup, validation, and rollback capabilities.
#
# Features:
# - Automatic backup before migration
# - Connection testing
# - Migration validation
# - Automatic rollback on failure
# - Detailed logging
#
# Usage:
#   ./production-migration.sh [OPTIONS]
#
# Options:
#   -h, --host         Database host (default: localhost)
#   -p, --port         Database port (default: 5432)
#   -d, --database     Database name (default: SlipVerificationDb)
#   -u, --user         Database user (default: postgres)
#   -w, --password     Database password (prompted if not provided)
#   -b, --backup-dir   Backup directory (default: /backups)
#   -s, --skip-backup  Skip backup creation (NOT RECOMMENDED)
#   --dry-run          Generate SQL script without applying
#   --help             Show this help message
#
# Example:
#   ./production-migration.sh -h prod-db.example.com -d SlipVerificationDb -u app_user
#
################################################################################

set -e  # Exit on error
set -o pipefail  # Exit on pipe failure

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default configuration
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-SlipVerificationDb}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-}"
BACKUP_DIR="${BACKUP_DIR:-/backups}"
SKIP_BACKUP=false
DRY_RUN=false
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
LOG_FILE="${BACKUP_DIR}/migration_${TIMESTAMP}.log"

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"

################################################################################
# Functions
################################################################################

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1" | tee -a "${LOG_FILE}"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1" | tee -a "${LOG_FILE}"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1" | tee -a "${LOG_FILE}"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "${LOG_FILE}"
}

show_help() {
    head -n 30 "$0" | grep "^#" | sed 's/^# //;s/^#//'
    exit 0
}

# Parse command line arguments
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
            -b|--backup-dir)
                BACKUP_DIR="$2"
                shift 2
                ;;
            -s|--skip-backup)
                SKIP_BACKUP=true
                shift
                ;;
            --dry-run)
                DRY_RUN=true
                shift
                ;;
            --help)
                show_help
                ;;
            *)
                print_error "Unknown option: $1"
                show_help
                ;;
        esac
    done
}

# Check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check if dotnet is installed
    if ! command -v dotnet &> /dev/null; then
        print_error "dotnet CLI not found. Please install .NET SDK."
        exit 1
    fi
    
    # Check if dotnet-ef is installed
    if ! dotnet ef --version &> /dev/null; then
        print_error "dotnet-ef tool not found. Installing..."
        dotnet tool install --global dotnet-ef || {
            print_error "Failed to install dotnet-ef tool."
            exit 1
        }
    fi
    
    # Check if pg_dump is installed (for backup)
    if ! command -v pg_dump &> /dev/null && [ "$SKIP_BACKUP" = false ]; then
        print_error "pg_dump not found. Please install PostgreSQL client tools or use --skip-backup."
        exit 1
    fi
    
    # Check if backup directory exists
    if [ ! -d "$BACKUP_DIR" ]; then
        print_info "Creating backup directory: $BACKUP_DIR"
        mkdir -p "$BACKUP_DIR" || {
            print_error "Failed to create backup directory: $BACKUP_DIR"
            exit 1
        }
    fi
    
    print_success "Prerequisites check passed"
}

# Test database connection
test_connection() {
    print_info "Testing database connection..."
    
    # Build connection string
    local conn_str="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER}"
    if [ -n "$DB_PASSWORD" ]; then
        conn_str="${conn_str};Password=${DB_PASSWORD}"
    fi
    
    # Test connection using psql
    export PGPASSWORD="$DB_PASSWORD"
    if psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT version();" &> /dev/null; then
        print_success "Database connection successful"
        return 0
    else
        print_error "Database connection failed!"
        print_error "Host: $DB_HOST, Port: $DB_PORT, Database: $DB_NAME, User: $DB_USER"
        exit 1
    fi
}

# Create database backup
create_backup() {
    if [ "$SKIP_BACKUP" = true ]; then
        print_warning "Skipping backup creation (--skip-backup flag set)"
        return 0
    fi
    
    print_info "Creating database backup..."
    
    local backup_file="${BACKUP_DIR}/${DB_NAME}_${TIMESTAMP}.backup"
    
    export PGPASSWORD="$DB_PASSWORD"
    pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -F c -f "$backup_file" 2>&1 | tee -a "${LOG_FILE}"
    
    if [ ${PIPESTATUS[0]} -eq 0 ]; then
        # Compress backup
        gzip "$backup_file" 2>&1 | tee -a "${LOG_FILE}"
        print_success "Backup created successfully: ${backup_file}.gz"
        
        # Store backup file path for potential rollback
        BACKUP_FILE="${backup_file}.gz"
        
        # Show backup size
        local backup_size=$(du -h "$BACKUP_FILE" | cut -f1)
        print_info "Backup size: $backup_size"
        
        return 0
    else
        print_error "Backup failed! Aborting migration."
        exit 1
    fi
}

# List pending migrations
list_pending_migrations() {
    print_info "Checking for pending migrations..."
    
    cd "$PROJECT_ROOT"
    
    # List all migrations
    dotnet ef migrations list \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --no-build \
        2>&1 | tee -a "${LOG_FILE}"
    
    if [ ${PIPESTATUS[0]} -ne 0 ]; then
        print_error "Failed to list migrations"
        return 1
    fi
    
    print_success "Migration list retrieved"
}

# Generate SQL script (for dry run)
generate_sql_script() {
    print_info "Generating SQL migration script..."
    
    cd "$PROJECT_ROOT"
    
    local sql_file="${BACKUP_DIR}/migration_${TIMESTAMP}.sql"
    
    dotnet ef migrations script \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --output "$sql_file" \
        --idempotent \
        --no-build \
        2>&1 | tee -a "${LOG_FILE}"
    
    if [ ${PIPESTATUS[0]} -eq 0 ]; then
        print_success "SQL script generated: $sql_file"
        print_info "Review the script before applying to production"
        return 0
    else
        print_error "Failed to generate SQL script"
        return 1
    fi
}

# Apply migrations
apply_migrations() {
    print_info "Applying database migrations..."
    
    cd "$PROJECT_ROOT"
    
    # Build connection string
    local conn_str="Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER}"
    if [ -n "$DB_PASSWORD" ]; then
        conn_str="${conn_str};Password=${DB_PASSWORD}"
    fi
    
    # Apply migrations
    dotnet ef database update \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --connection "$conn_str" \
        --no-build \
        --verbose \
        2>&1 | tee -a "${LOG_FILE}"
    
    if [ ${PIPESTATUS[0]} -eq 0 ]; then
        print_success "Migrations applied successfully!"
        return 0
    else
        print_error "Migration failed!"
        return 1
    fi
}

# Verify migration
verify_migration() {
    print_info "Verifying migration..."
    
    export PGPASSWORD="$DB_PASSWORD"
    
    # Check if __EFMigrationsHistory table exists and has records
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        -c "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 5;" \
        2>&1 | tee -a "${LOG_FILE}"
    
    if [ ${PIPESTATUS[0]} -eq 0 ]; then
        print_success "Migration verification successful"
        return 0
    else
        print_error "Migration verification failed"
        return 1
    fi
}

# Rollback on failure
rollback_migration() {
    print_error "Migration failed! Initiating rollback..."
    
    if [ -z "$BACKUP_FILE" ] || [ ! -f "$BACKUP_FILE" ]; then
        print_error "Backup file not found. Manual intervention required."
        print_error "Please restore database from backup manually."
        exit 1
    fi
    
    print_info "Restoring database from backup: $BACKUP_FILE"
    
    export PGPASSWORD="$DB_PASSWORD"
    
    # Decompress backup
    gunzip -c "$BACKUP_FILE" > "${BACKUP_FILE%.gz}" 2>&1 | tee -a "${LOG_FILE}"
    
    # Restore backup
    pg_restore -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "${BACKUP_FILE%.gz}" 2>&1 | tee -a "${LOG_FILE}"
    
    if [ ${PIPESTATUS[0]} -eq 0 ]; then
        print_success "Database restored from backup successfully"
        
        # Clean up decompressed backup
        rm -f "${BACKUP_FILE%.gz}"
    else
        print_error "Database restore failed! Manual intervention required."
        print_error "Backup file location: $BACKUP_FILE"
        exit 1
    fi
}

# Show summary
show_summary() {
    echo ""
    echo "================================================================================"
    echo -e "${GREEN}Migration Summary${NC}"
    echo "================================================================================"
    echo "Database Host:     $DB_HOST:$DB_PORT"
    echo "Database Name:     $DB_NAME"
    echo "Database User:     $DB_USER"
    echo "Timestamp:         $TIMESTAMP"
    if [ -n "$BACKUP_FILE" ]; then
        echo "Backup File:       $BACKUP_FILE"
    fi
    echo "Log File:          $LOG_FILE"
    echo "================================================================================"
    echo ""
}

################################################################################
# Main execution
################################################################################

main() {
    # Parse arguments
    parse_args "$@"
    
    # Print banner
    echo ""
    echo "================================================================================"
    echo "                   Production Database Migration Script"
    echo "================================================================================"
    echo ""
    
    # Initialize log file
    echo "Migration started at: $(date)" > "${LOG_FILE}"
    
    # Check prerequisites
    check_prerequisites
    
    # Test connection
    test_connection
    
    # List pending migrations
    list_pending_migrations
    
    # Prompt for confirmation
    if [ "$DRY_RUN" = false ]; then
        echo ""
        print_warning "You are about to apply migrations to: ${DB_HOST}:${DB_PORT}/${DB_NAME}"
        read -p "Do you want to continue? (yes/no): " -r
        echo ""
        if [[ ! $REPLY =~ ^[Yy]es$ ]]; then
            print_info "Migration cancelled by user"
            exit 0
        fi
    fi
    
    # Create backup
    create_backup
    
    if [ "$DRY_RUN" = true ]; then
        # Dry run - generate SQL script only
        generate_sql_script
        print_info "Dry run completed. Review the generated SQL script before applying."
        show_summary
        exit 0
    fi
    
    # Apply migrations
    if apply_migrations; then
        # Verify migration
        verify_migration
        
        print_success "Migration completed successfully!"
        show_summary
        
        # Log completion
        echo "Migration completed successfully at: $(date)" >> "${LOG_FILE}"
        exit 0
    else
        # Migration failed - rollback
        echo "Migration failed at: $(date)" >> "${LOG_FILE}"
        
        if [ "$SKIP_BACKUP" = false ]; then
            rollback_migration
        else
            print_error "Migration failed and backup was skipped. Manual intervention required."
        fi
        
        show_summary
        exit 1
    fi
}

# Run main function
main "$@"
