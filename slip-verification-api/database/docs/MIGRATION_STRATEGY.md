# Complete Migration Strategy Guide

## Overview
This guide provides a comprehensive migration strategy for the Slip Verification System PostgreSQL database using Entity Framework Core 9.

## Table of Contents
1. [Entity Framework Core Migrations](#entity-framework-core-migrations)
2. [Migration Class Examples](#migration-class-examples)
3. [Data Migration (Seed Data)](#data-migration-seed-data)
4. [Complex Migration Examples](#complex-migration-examples)
5. [Production Migration Process](#production-migration-process)
6. [Migration Testing Strategy](#migration-testing-strategy)
7. [Rollback Procedures](#rollback-procedures)
8. [Best Practices](#best-practices)

---

## Entity Framework Core Migrations

### Prerequisites
```bash
# Install EF Core tools globally
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef

# Verify installation
dotnet ef --version
```

### Basic Migration Commands

#### Create Initial Migration
```bash
# Create initial migration
dotnet ef migrations add InitialCreate \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# The migration files will be created in:
# src/SlipVerification.Infrastructure/Migrations/
```

#### Add New Migration
```bash
# Add new migration for changes
dotnet ef migrations add AddSlipVerificationTable \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

#### Update Database
```bash
# Apply all pending migrations
dotnet ef database update \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# Apply migrations up to a specific migration
dotnet ef database update AddSlipVerificationTable \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

#### Rollback to Specific Migration
```bash
# Rollback to a previous migration
dotnet ef database update PreviousMigrationName \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# Rollback all migrations (remove database)
dotnet ef database update 0 \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

#### Remove Last Migration
```bash
# Remove the last migration (only if not applied to database)
dotnet ef migrations remove \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

#### Generate SQL Scripts
```bash
# Generate SQL script for all migrations
dotnet ef migrations script \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --output migrations.sql

# Generate incremental SQL script (from one migration to another)
dotnet ef migrations script FromMigration ToMigration \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --output incremental.sql

# Generate idempotent script (can be run multiple times safely)
dotnet ef migrations script \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --output migrations.sql \
  --idempotent
```

#### List Migrations
```bash
# List all migrations
dotnet ef migrations list \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# List migrations with connection string
dotnet ef migrations list \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --connection "Host=localhost;Database=SlipVerificationDb;Username=postgres;Password=postgres"
```

---

## Migration Class Examples

### Example 1: Basic Table Creation Migration

```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Users table
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", nullable: false),
                    Username = table.Column<string>(type: "varchar(100)", nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", nullable: false),
                    FullName = table.Column<string>(type: "varchar(255)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    Role = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: "User"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
            
            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
            
            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
            
            // Create Orders table with foreign key
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false, defaultValue: "Pending"),
                    QrCodeData = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.CheckConstraint("CK_Orders_Amount", "\"Amount\" > 0");
                });
            
            // Create indexes for Orders
            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");
            
            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true,
                filter: "\"DeletedAt\" IS NULL");
            
            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status",
                filter: "\"DeletedAt\" IS NULL");
            
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt",
                descending: true);
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Orders");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
```

### Example 2: Adding Columns to Existing Table

```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to Users table
            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsLockedOut",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
            
            // Create index for new column
            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailVerificationToken",
                table: "Users",
                column: "EmailVerificationToken");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_EmailVerificationToken",
                table: "Users");
            
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Users");
            
            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Users");
            
            migrationBuilder.DropColumn(
                name: "IsLockedOut",
                table: "Users");
            
            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Users");
        }
    }
}
```

---

## Data Migration (Seed Data)

### Example: Seed Initial Data Migration

```csharp
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed admin user
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Username", "PasswordHash", "FullName", "Role", "EmailVerified", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted" },
                values: new object[] 
                { 
                    adminId, 
                    "admin@slipverification.com", 
                    "admin", 
                    // Note: In production, use proper password hashing
                    "$2a$11$KGJ5KGJ5KGJ5KGJ5KGJ5KOxQmq1h9ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5", // BCrypt hash for "Admin@123456"
                    "System Administrator",
                    0, // Admin role enum value
                    true,
                    true,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false
                });
            
            // Seed manager user
            var managerId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Username", "PasswordHash", "FullName", "Role", "EmailVerified", "IsActive", "CreatedAt", "UpdatedAt", "IsDeleted" },
                values: new object[] 
                { 
                    managerId, 
                    "manager@slipverification.com", 
                    "manager", 
                    "$2a$11$KGJ5KGJ5KGJ5KGJ5KGJ5KOxQmq1h9ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5",
                    "System Manager",
                    1, // Manager role enum value
                    true,
                    true,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false
                });
            
            // Seed sample notification templates
            migrationBuilder.Sql(@"
                INSERT INTO ""NotificationTemplates"" (""Id"", ""Code"", ""Name"", ""Channel"", ""Subject"", ""Body"", ""Language"", ""IsActive"", ""CreatedAt"", ""IsDeleted"")
                VALUES 
                (
                    gen_random_uuid(),
                    'ORDER_CREATED',
                    'Order Created Notification',
                    0, -- Email channel
                    'Your order has been created',
                    'Dear {{FullName}}, your order {{OrderNumber}} has been created successfully. Amount: {{Amount}} THB',
                    'en',
                    true,
                    NOW(),
                    false
                ),
                (
                    gen_random_uuid(),
                    'SLIP_VERIFIED',
                    'Slip Verification Success',
                    0, -- Email channel
                    'Payment slip verified successfully',
                    'Dear {{FullName}}, your payment slip for order {{OrderNumber}} has been verified successfully.',
                    'en',
                    true,
                    NOW(),
                    false
                );
            ");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seeded data
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Email",
                keyValue: "admin@slipverification.com");
            
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Email",
                keyValue: "manager@slipverification.com");
            
            migrationBuilder.Sql(@"
                DELETE FROM ""NotificationTemplates""
                WHERE ""Code"" IN ('ORDER_CREATED', 'SLIP_VERIFIED');
            ");
        }
    }
}
```

---

## Complex Migration Examples

### Example 1: Full-Text Search Migration

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add tsvector column for full-text search
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders""
                ADD COLUMN search_vector tsvector;
            ");
            
            // Create function to update search vector
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION orders_search_vector_update()
                RETURNS trigger AS $$
                BEGIN
                    NEW.search_vector :=
                        setweight(to_tsvector('english', coalesce(NEW.""OrderNumber"", '')), 'A') ||
                        setweight(to_tsvector('english', coalesce(NEW.""Description"", '')), 'B');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            // Create trigger
            migrationBuilder.Sql(@"
                CREATE TRIGGER orders_search_vector_trigger
                BEFORE INSERT OR UPDATE ON ""Orders""
                FOR EACH ROW
                EXECUTE FUNCTION orders_search_vector_update();
            ");
            
            // Create GIN index for fast full-text search
            migrationBuilder.Sql(@"
                CREATE INDEX idx_orders_search_vector
                ON ""Orders""
                USING GIN(search_vector);
            ");
            
            // Update existing rows
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET search_vector = 
                    setweight(to_tsvector('english', coalesce(""OrderNumber"", '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(""Description"", '')), 'B')
                WHERE search_vector IS NULL;
            ");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS orders_search_vector_trigger ON ""Orders"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS orders_search_vector_update();");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_orders_search_vector;");
            migrationBuilder.Sql(@"ALTER TABLE ""Orders"" DROP COLUMN IF EXISTS search_vector;");
        }
    }
}
```

### Example 2: Audit Trail with Triggers

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create audit function
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION audit_log_changes()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF (TG_OP = 'INSERT') THEN
                        INSERT INTO ""AuditLogs"" (""Id"", ""EntityType"", ""EntityId"", ""Action"", ""NewValues"", ""CreatedAt"")
                        VALUES (
                            gen_random_uuid(),
                            TG_TABLE_NAME,
                            NEW.""Id"",
                            'INSERT',
                            row_to_json(NEW)::jsonb,
                            NOW()
                        );
                        RETURN NEW;
                    ELSIF (TG_OP = 'UPDATE') THEN
                        INSERT INTO ""AuditLogs"" (""Id"", ""EntityType"", ""EntityId"", ""Action"", ""OldValues"", ""NewValues"", ""CreatedAt"")
                        VALUES (
                            gen_random_uuid(),
                            TG_TABLE_NAME,
                            NEW.""Id"",
                            'UPDATE',
                            row_to_json(OLD)::jsonb,
                            row_to_json(NEW)::jsonb,
                            NOW()
                        );
                        RETURN NEW;
                    ELSIF (TG_OP = 'DELETE') THEN
                        INSERT INTO ""AuditLogs"" (""Id"", ""EntityType"", ""EntityId"", ""Action"", ""OldValues"", ""CreatedAt"")
                        VALUES (
                            gen_random_uuid(),
                            TG_TABLE_NAME,
                            OLD.""Id"",
                            'DELETE',
                            row_to_json(OLD)::jsonb,
                            NOW()
                        );
                        RETURN OLD;
                    END IF;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            // Create triggers for key tables
            migrationBuilder.Sql(@"
                CREATE TRIGGER audit_orders_trigger
                AFTER INSERT OR UPDATE OR DELETE ON ""Orders""
                FOR EACH ROW EXECUTE FUNCTION audit_log_changes();
            ");
            
            migrationBuilder.Sql(@"
                CREATE TRIGGER audit_slipverifications_trigger
                AFTER INSERT OR UPDATE OR DELETE ON ""SlipVerifications""
                FOR EACH ROW EXECUTE FUNCTION audit_log_changes();
            ");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS audit_orders_trigger ON ""Orders"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS audit_slipverifications_trigger ON ""SlipVerifications"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS audit_log_changes();");
        }
    }
}
```

### Example 3: Partitioning Strategy

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogsPartitioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create partitioned audit logs table
            migrationBuilder.Sql(@"
                -- Create partition master table
                CREATE TABLE IF NOT EXISTS ""AuditLogs_Partitioned"" (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid,
                    ""EntityType"" varchar(100) NOT NULL,
                    ""EntityId"" uuid NOT NULL,
                    ""Action"" varchar(50) NOT NULL,
                    ""OldValues"" jsonb,
                    ""NewValues"" jsonb,
                    ""IpAddress"" varchar(50),
                    ""UserAgent"" text,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    PRIMARY KEY (""Id"", ""CreatedAt"")
                ) PARTITION BY RANGE (""CreatedAt"");
                
                -- Create partitions for current and next months
                CREATE TABLE ""AuditLogs_2024_01"" PARTITION OF ""AuditLogs_Partitioned""
                FOR VALUES FROM ('2024-01-01') TO ('2024-02-01');
                
                CREATE TABLE ""AuditLogs_2024_02"" PARTITION OF ""AuditLogs_Partitioned""
                FOR VALUES FROM ('2024-02-01') TO ('2024-03-01');
                
                -- Create index on partitioned table
                CREATE INDEX idx_auditlogs_partitioned_entitytype_entityid
                ON ""AuditLogs_Partitioned"" (""EntityType"", ""EntityId"");
            ");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""AuditLogs_Partitioned"" CASCADE;");
        }
    }
}
```

---

## Production Migration Process

### Pre-Migration Checklist

- [ ] Review all pending migrations
- [ ] Test migrations in development environment
- [ ] Test migrations in staging environment
- [ ] Create database backup
- [ ] Notify stakeholders of maintenance window
- [ ] Prepare rollback plan
- [ ] Review estimated downtime

### Migration Execution Steps

1. **Put application in maintenance mode**
2. **Create backup**
3. **Run migrations**
4. **Verify migration success**
5. **Test critical functionality**
6. **Take application out of maintenance mode**
7. **Monitor for issues**

### Production Migration Script

See `database/scripts/migrations/production-migration.sh` for the complete production migration script.

---

## Migration Testing Strategy

### Unit Tests for Migrations

```csharp
using Microsoft.EntityFrameworkCore;
using SlipVerification.Infrastructure.Data;
using Xunit;

namespace SlipVerification.IntegrationTests.Migrations;

public class MigrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public MigrationTests()
    {
        _connectionString = $"Host=localhost;Port=5432;Database=TestDb_{Guid.NewGuid()};Username=postgres;Password=postgres";
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Migration_InitialCreate_ShouldCreateAllTables()
    {
        // Act
        await _context.Database.MigrateAsync();
        
        // Assert
        Assert.True(await _context.Database.CanConnectAsync());
        
        // Verify tables exist
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public' 
            AND table_type = 'BASE TABLE';
        ";
        
        var tables = new List<string>();
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }
        
        Assert.Contains("Users", tables);
        Assert.Contains("Orders", tables);
        Assert.Contains("SlipVerifications", tables);
        Assert.Contains("Transactions", tables);
        Assert.Contains("Notifications", tables);
        Assert.Contains("AuditLogs", tables);
    }

    [Fact]
    public async Task Migration_UpDown_ShouldBeReversible()
    {
        // Arrange - Apply all migrations
        await _context.Database.MigrateAsync();
        var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
        var initialCount = appliedMigrations.Count();
        
        // Act - Get the last migration name
        var lastMigration = appliedMigrations.OrderBy(m => m).Last();
        var previousMigration = appliedMigrations.OrderBy(m => m).Reverse().Skip(1).FirstOrDefault();
        
        if (previousMigration != null)
        {
            // Rollback to previous migration
            await _context.Database.MigrateAsync(previousMigration);
            var afterRollback = await _context.Database.GetAppliedMigrationsAsync();
            
            // Assert
            Assert.True(initialCount > afterRollback.Count());
            Assert.DoesNotContain(lastMigration, afterRollback);
        }
    }

    [Fact]
    public async Task Migration_Indexes_ShouldBeCreated()
    {
        // Arrange
        await _context.Database.MigrateAsync();
        
        // Act - Query for indexes
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT indexname 
            FROM pg_indexes 
            WHERE schemaname = 'public'
            AND indexname LIKE 'IX_%';
        ";
        
        var indexes = new List<string>();
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                indexes.Add(reader.GetString(0));
            }
        }
        
        // Assert - Check for important indexes
        Assert.Contains(indexes, i => i.Contains("IX_Users_Email"));
        Assert.Contains(indexes, i => i.Contains("IX_Orders_OrderNumber"));
        Assert.Contains(indexes, i => i.Contains("IX_SlipVerifications_OrderId"));
    }

    public void Dispose()
    {
        // Clean up test database
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
```

---

## Rollback Procedures

### Automatic Rollback

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### Manual Rollback with Backup

```bash
#!/bin/bash
# rollback-migration.sh

set -e

# Configuration
DB_HOST="localhost"
DB_NAME="SlipVerificationDb"
DB_USER="postgres"
BACKUP_FILE="/backups/backup_before_migration.dump"

echo "Rolling back migration..."

# 1. Stop application
echo "Stopping application..."
# kubectl scale deployment/backend-api --replicas=0 -n slip-verification

# 2. Restore from backup
echo "Restoring database from backup..."
pg_restore -h $DB_HOST -U $DB_USER -d $DB_NAME -c $BACKUP_FILE

# 3. Verify restoration
echo "Verifying database restoration..."
psql -h $DB_HOST -U $DB_USER -d $DB_NAME -c "SELECT COUNT(*) FROM \"Users\";"

# 4. Restart application
echo "Restarting application..."
# kubectl scale deployment/backend-api --replicas=3 -n slip-verification

echo "Rollback completed successfully!"
```

### Emergency Rollback Checklist

- [ ] Stop application immediately
- [ ] Assess the situation
- [ ] Restore from the most recent backup
- [ ] Verify data integrity
- [ ] Test critical functionality
- [ ] Restart application
- [ ] Monitor for issues
- [ ] Document the incident
- [ ] Plan corrective actions

---

## Best Practices

### 1. Migration Design

- **Keep migrations small and focused**: One logical change per migration
- **Always provide Down() method**: Ensure migrations are reversible
- **Use meaningful migration names**: Describe what the migration does
- **Test migrations thoroughly**: Test both Up and Down methods
- **Document complex migrations**: Add comments for complex logic

### 2. Data Safety

- **Always backup before migration**: Create full database backup
- **Use transactions**: Wrap data changes in transactions
- **Test on non-production first**: Development → Staging → Production
- **Verify data integrity**: Check data after migration
- **Monitor during migration**: Watch for errors and performance issues

### 3. Performance Considerations

- **Create indexes after data load**: For large data migrations
- **Use batch operations**: For bulk data updates
- **Monitor query performance**: Check execution plans
- **Consider maintenance windows**: Run during low-traffic periods
- **Disable triggers if needed**: For performance-critical migrations

### 4. Version Control

- **Commit migrations to source control**: Track all migration files
- **Never modify applied migrations**: Create new migrations instead
- **Use consistent naming**: Follow team conventions
- **Document breaking changes**: Note in commit messages
- **Tag releases**: Tag database schema versions

### 5. Production Safety

- **Use idempotent scripts**: Scripts that can run multiple times safely
- **Implement health checks**: Verify system health after migration
- **Have rollback plan**: Always have a way to revert
- **Communicate with team**: Notify all stakeholders
- **Keep audit trail**: Log all migration activities

### 6. Zero-Downtime Deployments

#### Expand-Contract Pattern

1. **Expand Phase**: Add new schema elements
2. **Migrate Data**: Copy/transform data to new structure
3. **Deploy New Code**: Update application to use new schema
4. **Contract Phase**: Remove old schema elements

#### Example: Renaming a Column

```csharp
// Migration 1: Add new column
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "FullName",
        table: "Users",
        type: "varchar(255)",
        nullable: true);
    
    // Copy data from old column
    migrationBuilder.Sql(@"
        UPDATE ""Users""
        SET ""FullName"" = ""Name"";
    ");
}

// Migration 2: Remove old column (after application deployment)
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "Name",
        table: "Users");
}
```

### 7. Monitoring and Validation

- **Monitor migration duration**: Track how long migrations take
- **Check for locked tables**: Identify blocking queries
- **Verify data counts**: Ensure no data loss
- **Test critical queries**: Verify performance
- **Review migration logs**: Check for warnings or errors

### 8. Documentation

- **Document migration purpose**: Explain why the migration is needed
- **List affected tables**: Document schema changes
- **Note breaking changes**: Highlight incompatibilities
- **Update API documentation**: If schema affects API
- **Maintain changelog**: Keep migration history

---

## Additional Resources

### Official Documentation
- [Entity Framework Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/16/)
- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)

### Tools
- **dotnet-ef**: EF Core CLI tools
- **pgAdmin**: PostgreSQL administration tool
- **DBeaver**: Universal database tool
- **Flyway**: Alternative migration tool
- **Liquibase**: Another migration alternative

### Related Documentation
- [Database Schema Documentation](../docs/ERD.md)
- [Backup and Restore Procedures](BACKUP_RESTORE.md)
- [Performance Tuning Guide](PERFORMANCE_TUNING.md)
- [Index Strategy](INDEX_STRATEGY.md)

---

## Troubleshooting

### Common Issues

#### Migration fails with "relation already exists"
```bash
# Check applied migrations
dotnet ef migrations list

# Remove migration if not applied
dotnet ef migrations remove

# Or rollback and reapply
dotnet ef database update PreviousMigration
```

#### Cannot connect to database
```bash
# Check connection string
# Verify PostgreSQL is running
systemctl status postgresql

# Test connection
psql -h localhost -U postgres -d SlipVerificationDb -c "SELECT 1;"
```

#### Migration takes too long
```bash
# Generate SQL script and review
dotnet ef migrations script --output review.sql

# Apply manually with EXPLAIN ANALYZE
psql -h localhost -U postgres -d SlipVerificationDb -f review.sql
```

---

## Summary

This migration strategy provides:
- ✅ Complete EF Core migration workflow
- ✅ Production-ready migration scripts
- ✅ Comprehensive testing approach
- ✅ Rollback procedures
- ✅ Best practices and guidelines
- ✅ Zero-downtime deployment strategies

For questions or issues, contact the database team at admin@slipverification.com
