#!/bin/bash
################################################################################
# Migration Testing Script
# 
# This script tests migrations in a safe test environment before applying
# to production.
#
# Usage:
#   ./test-migrations.sh
#
################################################################################

set -e

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
TEST_DB_NAME="SlipVerificationDb_Test_$(date +%s)"
DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_USER="${DB_USER:-postgres}"
DB_PASSWORD="${DB_PASSWORD:-postgres}"

# Script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../../.." && pwd)"

print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Create test database
create_test_database() {
    print_info "Creating test database: $TEST_DB_NAME"
    
    export PGPASSWORD="$DB_PASSWORD"
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres \
        -c "CREATE DATABASE \"$TEST_DB_NAME\";" 2>/dev/null || {
        print_error "Failed to create test database"
        return 1
    }
    
    print_success "Test database created"
}

# Test migration UP
test_migration_up() {
    print_info "Testing migration UP (apply all migrations)..."
    
    cd "$PROJECT_ROOT"
    
    local conn_str="Host=${DB_HOST};Port=${DB_PORT};Database=${TEST_DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
    
    dotnet ef database update \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --connection "$conn_str" \
        --no-build
    
    if [ $? -eq 0 ]; then
        print_success "Migration UP successful"
        return 0
    else
        print_error "Migration UP failed"
        return 1
    fi
}

# Verify schema
verify_schema() {
    print_info "Verifying database schema..."
    
    export PGPASSWORD="$DB_PASSWORD"
    
    # Check tables
    local table_count=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$TEST_DB_NAME" \
        -t -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE';")
    
    print_info "Tables created: $table_count"
    
    # List tables
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$TEST_DB_NAME" \
        -c "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE' ORDER BY table_name;"
    
    # Check indexes
    local index_count=$(psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$TEST_DB_NAME" \
        -t -c "SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'public';")
    
    print_info "Indexes created: $index_count"
    
    print_success "Schema verification completed"
}

# Test migration DOWN (rollback)
test_migration_down() {
    print_info "Testing migration DOWN (rollback to first migration)..."
    
    cd "$PROJECT_ROOT"
    
    # Get first migration
    local first_migration=$(dotnet ef migrations list \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --no-build | grep -v "Build started" | grep -v "Build succeeded" | head -n 1 | awk '{print $1}')
    
    if [ -z "$first_migration" ]; then
        print_error "No migrations found"
        return 1
    fi
    
    local conn_str="Host=${DB_HOST};Port=${DB_PORT};Database=${TEST_DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
    
    print_info "Rolling back to: $first_migration"
    
    dotnet ef database update "$first_migration" \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --connection "$conn_str" \
        --no-build
    
    if [ $? -eq 0 ]; then
        print_success "Migration DOWN successful"
        return 0
    else
        print_error "Migration DOWN failed"
        return 1
    fi
}

# Test migration UP again
test_migration_up_again() {
    print_info "Testing migration UP again (reapply all migrations)..."
    
    cd "$PROJECT_ROOT"
    
    local conn_str="Host=${DB_HOST};Port=${DB_PORT};Database=${TEST_DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
    
    dotnet ef database update \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --connection "$conn_str" \
        --no-build
    
    if [ $? -eq 0 ]; then
        print_success "Migration UP again successful"
        return 0
    else
        print_error "Migration UP again failed"
        return 1
    fi
}

# Clean up test database
cleanup_test_database() {
    print_info "Cleaning up test database: $TEST_DB_NAME"
    
    export PGPASSWORD="$DB_PASSWORD"
    psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres \
        -c "DROP DATABASE IF EXISTS \"$TEST_DB_NAME\";" 2>/dev/null
    
    print_success "Test database cleaned up"
}

# Main execution
main() {
    echo ""
    echo "================================================================================"
    echo "                      Migration Testing Script"
    echo "================================================================================"
    echo ""
    
    # Build project first
    print_info "Building project..."
    cd "$PROJECT_ROOT"
    dotnet build --configuration Release
    
    if [ $? -ne 0 ]; then
        print_error "Build failed"
        exit 1
    fi
    
    print_success "Build completed"
    echo ""
    
    # Create test database
    create_test_database || exit 1
    echo ""
    
    # Test UP migration
    test_migration_up || {
        cleanup_test_database
        exit 1
    }
    echo ""
    
    # Verify schema
    verify_schema
    echo ""
    
    # Test DOWN migration (rollback)
    test_migration_down || {
        cleanup_test_database
        exit 1
    }
    echo ""
    
    # Test UP migration again
    test_migration_up_again || {
        cleanup_test_database
        exit 1
    }
    echo ""
    
    # Final schema verification
    verify_schema
    echo ""
    
    # Clean up
    cleanup_test_database
    
    echo ""
    echo "================================================================================"
    print_success "All migration tests passed!"
    echo "================================================================================"
    echo ""
}

# Run main
main "$@"
