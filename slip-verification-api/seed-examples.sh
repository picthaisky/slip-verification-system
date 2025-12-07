#!/bin/bash
# Database Seeding Examples and Usage
# This script demonstrates various seeding scenarios

set -e

echo "=========================================="
echo "Database Seeding Examples"
echo "=========================================="
echo ""

# Function to display usage
show_usage() {
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  seed-dev       Seed database for development (admin + sample data)"
    echo "  seed-prod      Seed database for production (admin only)"
    echo "  seed-test      Seed database for testing"
    echo "  cleanup        Drop and recreate database"
    echo "  help           Show this help message"
    echo ""
}

# Function to run migrations
run_migrations() {
    echo "Running database migrations..."
    cd src/SlipVerification.API
    dotnet ef database update
    cd ../..
    echo "Migrations completed!"
}

# Function to seed for development
seed_dev() {
    echo "Seeding database for Development environment..."
    export ASPNETCORE_ENVIRONMENT=Development
    cd src/SlipVerification.API
    dotnet run seed
    cd ../..
    echo "Development seeding completed!"
    echo ""
    echo "Seeded data:"
    echo "  - 1 Admin user (admin@example.com / Admin@123456)"
    echo "  - 50 Sample users"
    echo "  - 200 Sample orders"
    echo "  - 150 Sample slip verifications"
}

# Function to seed for production
seed_prod() {
    echo "Seeding database for Production environment..."
    export ASPNETCORE_ENVIRONMENT=Production
    cd src/SlipVerification.API
    dotnet run seed
    cd ../..
    echo "Production seeding completed!"
    echo ""
    echo "Seeded data:"
    echo "  - 1 Admin user (admin@example.com / Admin@123456)"
    echo ""
    echo "⚠️  WARNING: Change the default admin password immediately!"
}

# Function to seed for testing
seed_test() {
    echo "Setting up test database..."
    export ASPNETCORE_ENVIRONMENT=Test
    
    # Drop and recreate database
    cd src/SlipVerification.API
    dotnet ef database drop --force
    dotnet ef database update
    
    # Seed basic data
    export ASPNETCORE_ENVIRONMENT=Development
    dotnet run seed
    cd ../..
    
    echo "Test database ready!"
}

# Function to cleanup database
cleanup() {
    echo "⚠️  WARNING: This will DROP the database!"
    echo "Press Ctrl+C to cancel, or Enter to continue..."
    read
    
    echo "Dropping database..."
    cd src/SlipVerification.API
    dotnet ef database drop --force
    echo "Database dropped!"
    
    echo "Recreating database..."
    dotnet ef database update
    echo "Database recreated!"
    cd ../..
}

# Main script logic
case "${1:-help}" in
    seed-dev)
        run_migrations
        seed_dev
        ;;
    seed-prod)
        run_migrations
        seed_prod
        ;;
    seed-test)
        seed_test
        ;;
    cleanup)
        cleanup
        ;;
    help)
        show_usage
        ;;
    *)
        echo "Error: Unknown command '$1'"
        echo ""
        show_usage
        exit 1
        ;;
esac

echo ""
echo "=========================================="
echo "Operation completed successfully!"
echo "=========================================="
