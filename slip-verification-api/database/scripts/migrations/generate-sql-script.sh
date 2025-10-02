#!/bin/bash
################################################################################
# Generate Migration SQL Script
# 
# This script generates SQL scripts from Entity Framework migrations
# Useful for reviewing migrations before applying or for manual execution
#
# Usage:
#   ./generate-sql-script.sh [OPTIONS]
#
# Options:
#   --from             Starting migration (optional, defaults to beginning)
#   --to               Ending migration (optional, defaults to latest)
#   --output           Output file path (default: migrations_<timestamp>.sql)
#   --idempotent       Generate idempotent script (can run multiple times)
#   --help             Show this help message
#
# Examples:
#   # Generate script for all migrations
#   ./generate-sql-script.sh
#
#   # Generate incremental script between two migrations
#   ./generate-sql-script.sh --from InitialCreate --to AddNotificationEnhancements
#
#   # Generate idempotent script
#   ./generate-sql-script.sh --idempotent --output production.sql
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
FROM_MIGRATION=""
TO_MIGRATION=""
OUTPUT_FILE=""
IDEMPOTENT=false
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

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

show_help() {
    head -n 30 "$0" | grep "^#" | sed 's/^# //;s/^#//'
    exit 0
}

parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --from)
                FROM_MIGRATION="$2"
                shift 2
                ;;
            --to)
                TO_MIGRATION="$2"
                shift 2
                ;;
            --output)
                OUTPUT_FILE="$2"
                shift 2
                ;;
            --idempotent)
                IDEMPOTENT=true
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

# List migrations
list_migrations() {
    print_info "Available migrations:"
    echo ""
    
    cd "$PROJECT_ROOT"
    
    dotnet ef migrations list \
        --project src/SlipVerification.Infrastructure \
        --startup-project src/SlipVerification.API \
        --no-build
    
    echo ""
}

# Generate SQL script
generate_script() {
    cd "$PROJECT_ROOT"
    
    # Set default output file if not specified
    if [ -z "$OUTPUT_FILE" ]; then
        if [ -n "$FROM_MIGRATION" ] && [ -n "$TO_MIGRATION" ]; then
            OUTPUT_FILE="migration_${FROM_MIGRATION}_to_${TO_MIGRATION}_${TIMESTAMP}.sql"
        else
            OUTPUT_FILE="migrations_all_${TIMESTAMP}.sql"
        fi
    fi
    
    print_info "Generating SQL script..."
    print_info "Output file: $OUTPUT_FILE"
    
    # Build command
    local cmd="dotnet ef migrations script"
    cmd="$cmd --project src/SlipVerification.Infrastructure"
    cmd="$cmd --startup-project src/SlipVerification.API"
    cmd="$cmd --output \"$OUTPUT_FILE\""
    cmd="$cmd --no-build"
    
    # Add from migration if specified
    if [ -n "$FROM_MIGRATION" ]; then
        cmd="$cmd $FROM_MIGRATION"
    fi
    
    # Add to migration if specified
    if [ -n "$TO_MIGRATION" ]; then
        if [ -z "$FROM_MIGRATION" ]; then
            cmd="$cmd 0"  # From beginning
        fi
        cmd="$cmd $TO_MIGRATION"
    fi
    
    # Add idempotent flag if specified
    if [ "$IDEMPOTENT" = true ]; then
        cmd="$cmd --idempotent"
        print_info "Generating idempotent script (safe to run multiple times)"
    fi
    
    # Execute command
    eval $cmd
    
    if [ $? -eq 0 ]; then
        print_success "SQL script generated successfully!"
        
        # Show file size and line count
        local file_size=$(du -h "$OUTPUT_FILE" | cut -f1)
        local line_count=$(wc -l < "$OUTPUT_FILE")
        
        print_info "File size: $file_size"
        print_info "Line count: $line_count"
        
        # Show first few lines
        echo ""
        print_info "Script preview (first 20 lines):"
        echo "================================================================================"
        head -n 20 "$OUTPUT_FILE"
        echo "..."
        echo "================================================================================"
        echo ""
        
        return 0
    else
        print_error "Failed to generate SQL script!"
        return 1
    fi
}

# Analyze script
analyze_script() {
    local script_file=$1
    
    if [ ! -f "$script_file" ]; then
        return
    fi
    
    print_info "Script analysis:"
    echo ""
    
    # Count SQL operations
    local create_count=$(grep -c "CREATE " "$script_file" || echo 0)
    local alter_count=$(grep -c "ALTER " "$script_file" || echo 0)
    local drop_count=$(grep -c "DROP " "$script_file" || echo 0)
    local insert_count=$(grep -c "INSERT " "$script_file" || echo 0)
    local update_count=$(grep -c "UPDATE " "$script_file" || echo 0)
    
    echo "  CREATE statements: $create_count"
    echo "  ALTER statements:  $alter_count"
    echo "  DROP statements:   $drop_count"
    echo "  INSERT statements: $insert_count"
    echo "  UPDATE statements: $update_count"
    echo ""
    
    # List tables being created
    print_info "Tables being created:"
    grep "CREATE TABLE" "$script_file" | sed 's/.*CREATE TABLE /  - /' | sed 's/ (.*//' || echo "  None"
    echo ""
}

main() {
    parse_args "$@"
    
    echo ""
    echo "================================================================================"
    echo "                  Generate Migration SQL Script"
    echo "================================================================================"
    echo ""
    
    # Build project first
    print_info "Building project..."
    cd "$PROJECT_ROOT"
    dotnet build --configuration Release
    
    if [ $? -ne 0 ]; then
        print_error "Build failed!"
        exit 1
    fi
    
    print_success "Build completed"
    echo ""
    
    # List available migrations
    list_migrations
    
    # Generate script
    generate_script
    
    # Analyze generated script
    analyze_script "$OUTPUT_FILE"
    
    # Show instructions
    echo ""
    print_info "Next steps:"
    echo "  1. Review the generated SQL script: $OUTPUT_FILE"
    echo "  2. Test on development/staging environment first"
    echo "  3. Create database backup before applying to production"
    echo "  4. Apply script manually or use migration tools"
    echo ""
    
    if [ "$IDEMPOTENT" = false ]; then
        print_info "Tip: Use --idempotent flag to generate a script that can be run multiple times safely"
    fi
    
    echo ""
    echo "================================================================================"
    print_success "Script generation completed"
    echo "================================================================================"
    echo ""
}

main "$@"
