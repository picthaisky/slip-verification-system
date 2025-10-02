#!/bin/bash
set -e

echo "========================================"
echo "Slip Verification API - Docker Startup"
echo "========================================"

# Function to wait for database
wait_for_database() {
    echo "Waiting for database to be ready..."
    
    max_retries=30
    retry_count=0
    
    until dotnet ef database update --no-build --project /app/SlipVerification.Infrastructure.dll --startup-project /app/SlipVerification.API.dll 2>/dev/null; do
        retry_count=$((retry_count + 1))
        
        if [ $retry_count -ge $max_retries ]; then
            echo "ERROR: Database connection timeout after $max_retries attempts"
            exit 1
        fi
        
        echo "Database is unavailable - sleeping (attempt $retry_count/$max_retries)"
        sleep 2
    done
    
    echo "Database is ready!"
}

# Wait for database
wait_for_database

# Run migrations
echo "Running database migrations..."
dotnet ef database update --no-build --project /app/SlipVerification.Infrastructure.dll --startup-project /app/SlipVerification.API.dll
echo "Migrations completed!"

# Seed database in development environment
if [ "$ASPNETCORE_ENVIRONMENT" = "Development" ]; then
    echo "Development environment detected"
    echo "Seeding database..."
    dotnet /app/SlipVerification.API.dll seed --no-build
    echo "Database seeding completed!"
fi

# Start the application
echo "Starting Slip Verification API..."
echo "Environment: ${ASPNETCORE_ENVIRONMENT:-Production}"
echo "========================================"

exec dotnet /app/SlipVerification.API.dll
