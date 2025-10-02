# Migration Quick Reference

Quick reference guide for common database migration tasks.

## 📋 Prerequisites

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version
```

## 🚀 Common Commands

### Create Migration

```bash
# Create new migration
dotnet ef migrations add MigrationName \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# Examples:
dotnet ef migrations add AddUserProfileFields
dotnet ef migrations add UpdateOrderStatus
dotnet ef migrations add AddPaymentGateway
```

### Apply Migrations

```bash
# Apply all pending migrations
dotnet ef database update \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# Apply up to specific migration
dotnet ef database update MigrationName \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### List Migrations

```bash
# List all migrations (applied and pending)
dotnet ef migrations list \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### Remove Migration

```bash
# Remove last migration (only if not applied)
dotnet ef migrations remove \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

### Generate SQL Script

```bash
# Generate SQL for all migrations
dotnet ef migrations script \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --output migrations.sql

# Generate incremental SQL (from one to another)
dotnet ef migrations script FromMigration ToMigration \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --output incremental.sql

# Generate idempotent SQL (safe to run multiple times)
dotnet ef migrations script \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API \
  --output migrations.sql \
  --idempotent
```

### Rollback Migration

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigration \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API

# Rollback all migrations
dotnet ef database update 0 \
  --project src/SlipVerification.Infrastructure \
  --startup-project src/SlipVerification.API
```

## 🛠️ Using Helper Scripts

### Test Migrations

```bash
# Test all migrations in safe environment
cd database/scripts/migrations
./test-migrations.sh
```

### Production Migration

```bash
# Apply to production with backup
cd database/scripts/migrations
./production-migration.sh \
  --host prod-db.example.com \
  --database SlipVerificationDb \
  --user postgres

# Dry run (review only)
./production-migration.sh --dry-run
```

### Generate SQL Script

```bash
# Generate SQL for review
cd database/scripts/migrations
./generate-sql-script.sh

# Generate idempotent script
./generate-sql-script.sh --idempotent --output prod.sql
```

### Rollback

```bash
# Rollback to specific migration
cd database/scripts/migrations
./rollback-migration.sh InitialCreate

# Restore from backup
./rollback-migration.sh --backup /backups/backup.gz
```

## 📝 Migration Patterns

### Adding Column

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "NewColumn",
        table: "TableName",
        type: "varchar(255)",
        maxLength: 255,
        nullable: true,
        defaultValue: "DefaultValue");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "NewColumn",
        table: "TableName");
}
```

### Creating Index

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateIndex(
        name: "IX_TableName_ColumnName",
        table: "TableName",
        column: "ColumnName",
        unique: false);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_TableName_ColumnName",
        table: "TableName");
}
```

### Adding Foreign Key

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateIndex(
        name: "IX_ChildTable_ParentId",
        table: "ChildTable",
        column: "ParentId");

    migrationBuilder.AddForeignKey(
        name: "FK_ChildTable_ParentTable_ParentId",
        table: "ChildTable",
        column: "ParentId",
        principalTable: "ParentTable",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropForeignKey(
        name: "FK_ChildTable_ParentTable_ParentId",
        table: "ChildTable");

    migrationBuilder.DropIndex(
        name: "IX_ChildTable_ParentId",
        table: "ChildTable");
}
```

### Seed Data

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "TableName",
        columns: new[] { "Id", "Name", "Value" },
        values: new object[] { Guid.NewGuid(), "Name", "Value" });
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DeleteData(
        table: "TableName",
        keyColumn: "Name",
        keyValue: "Name");
}
```

### Raw SQL

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        CREATE OR REPLACE FUNCTION my_function()
        RETURNS trigger AS $$
        BEGIN
            -- Function body
            RETURN NEW;
        END;
        $$ LANGUAGE plpgsql;
    ");
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql("DROP FUNCTION IF EXISTS my_function();");
}
```

## 🔍 Troubleshooting

### Migration Already Applied

```bash
# Remove from database history
DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = 'MigrationName';

# Then remove migration file
dotnet ef migrations remove
```

### Cannot Connect to Database

```bash
# Test connection
psql -h localhost -U postgres -d SlipVerificationDb -c "SELECT 1;"

# Check connection string in appsettings.json
```

### Migration Fails

```bash
# Check verbose output
dotnet ef database update --verbose

# Rollback to previous
dotnet ef database update PreviousMigration

# Or restore from backup
./rollback-migration.sh --backup /backups/latest.gz
```

## 📊 Check Migration Status

```bash
# List applied migrations
psql -h localhost -U postgres -d SlipVerificationDb \
  -c "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 10;"

# Count tables
psql -h localhost -U postgres -d SlipVerificationDb \
  -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public';"

# List all tables
psql -h localhost -U postgres -d SlipVerificationDb -c "\dt"

# Describe table
psql -h localhost -U postgres -d SlipVerificationDb -c "\d TableName"
```

## 🔐 Connection Strings

### Development
```
Host=localhost;Port=5432;Database=SlipVerificationDb;Username=postgres;Password=postgres
```

### Production
```
Host=prod-db.example.com;Port=5432;Database=SlipVerificationDb;Username=app_user;Password=***;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100
```

### Docker Compose
```
Host=postgres;Port=5432;Database=SlipVerificationDb;Username=postgres;Password=postgres
```

## 📚 Resources

- [Complete Migration Strategy](docs/MIGRATION_STRATEGY.md)
- [Migration Scripts Documentation](scripts/migrations/README.md)
- [EF Core Migrations Docs](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/16/)

## 💡 Tips

1. **Always test in development first**
2. **Create backups before production migrations**
3. **Use descriptive migration names**
4. **Review generated SQL before applying**
5. **Keep migrations small and focused**
6. **Test rollback procedures**
7. **Document breaking changes**
8. **Monitor during production migration**

## 🎯 Cheat Sheet

| Task | Command |
|------|---------|
| Create migration | `dotnet ef migrations add Name` |
| Apply migrations | `dotnet ef database update` |
| List migrations | `dotnet ef migrations list` |
| Remove last migration | `dotnet ef migrations remove` |
| Generate SQL | `dotnet ef migrations script --output file.sql` |
| Rollback | `dotnet ef database update PreviousMigration` |
| Test migrations | `./test-migrations.sh` |
| Production deploy | `./production-migration.sh` |

---

**Quick Access**: Save this as a bookmark for fast reference during development and deployment.
