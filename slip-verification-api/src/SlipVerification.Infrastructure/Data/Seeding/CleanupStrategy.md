# Database Cleanup Strategy

This document outlines strategies for cleaning up seeded data in different scenarios.

## Overview

The seeding system is idempotent, but there are times when you need to clean up or reset data:
- Testing scenarios
- Development environment resets
- Data migration testing
- Performance testing

## Cleanup Approaches

### 1. Complete Database Reset

Most thorough approach - drops and recreates the entire database.

```bash
# Using Entity Framework CLI
dotnet ef database drop --force
dotnet ef database update

# Then re-seed
dotnet run seed
```

**Pros:**
- Clean slate
- Removes all data including migrations history
- Guaranteed fresh start

**Cons:**
- Destructive (loses all data)
- Requires re-running all migrations
- Slower than selective cleanup

### 2. Selective Table Cleanup

Remove data from specific tables while preserving structure.

```sql
-- Delete seeded slips (keeps admin data)
DELETE FROM "SlipVerifications" WHERE "UserId" != (
    SELECT "Id" FROM "Users" WHERE "Email" = 'admin@example.com'
);

-- Delete seeded orders
DELETE FROM "Orders" WHERE "UserId" != (
    SELECT "Id" FROM "Users" WHERE "Email" = 'admin@example.com'
);

-- Delete sample users (keep admin)
DELETE FROM "Users" WHERE "Email" != 'admin@example.com';

-- Reset sequences
ALTER SEQUENCE "Users_Id_seq" RESTART WITH 1;
ALTER SEQUENCE "Orders_Id_seq" RESTART WITH 1;
ALTER SEQUENCE "SlipVerifications_Id_seq" RESTART WITH 1;
```

**Pros:**
- Keeps database structure
- Faster than full reset
- Can preserve admin user

**Cons:**
- Must handle foreign key constraints
- Manual sequence management
- Risk of missing related data

### 3. Soft Delete Approach

Mark seeded data as deleted without actual removal.

```csharp
public async Task SoftDeleteSampleDataAsync()
{
    var sampleUsers = await _context.Users
        .Where(u => u.Email != "admin@example.com")
        .ToListAsync();
    
    foreach (var user in sampleUsers)
    {
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
    }
    
    await _context.SaveChangesAsync();
}
```

**Pros:**
- Non-destructive
- Can be reversed
- Preserves audit trail

**Cons:**
- Data still in database
- Affects query performance
- Requires cleanup later

### 4. Transaction Rollback (Testing)

For integration tests, wrap seeding in a transaction.

```csharp
[Fact]
public async Task TestWithSeedData()
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Seed test data
        var generator = new FakeDataGenerator();
        var users = generator.GenerateUsers(10);
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        
        // Run your test
        var result = await _service.GetUsersAsync();
        Assert.Equal(10, result.Count);
    }
    finally
    {
        // Automatic rollback on dispose
        await transaction.RollbackAsync();
    }
}
```

**Pros:**
- Automatic cleanup
- No persistent changes
- Fast and reliable

**Cons:**
- Only for testing
- Limited to single transaction scope
- May not work with distributed systems

### 5. Named Seed Sets

Tag seeded data for easy cleanup.

```csharp
public class FakeDataGenerator
{
    public List<User> GenerateUsers(int count, string seedSet = "default")
    {
        return _userFaker
            .RuleFor(u => u.Metadata, f => new { SeedSet = seedSet })
            .Generate(count);
    }
}

// Cleanup specific seed set
public async Task CleanupSeedSetAsync(string seedSet)
{
    var users = await _context.Users
        .Where(u => EF.Functions.JsonExtract<string>(u.Metadata, "$.SeedSet") == seedSet)
        .ToListAsync();
    
    _context.Users.RemoveRange(users);
    await _context.SaveChangesAsync();
}
```

**Pros:**
- Selective cleanup
- Multiple seed sets
- Flexible management

**Cons:**
- Requires metadata field
- More complex queries
- JSON operations may be slower

## Cleanup Script

Create a cleanup script for common scenarios:

```csharp
// CleanupSeeder.cs
public class CleanupSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CleanupSeeder> _logger;
    
    public CleanupSeeder(
        ApplicationDbContext context,
        ILogger<CleanupSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Removes all sample data, keeping admin user
    /// </summary>
    public async Task CleanupSampleDataAsync()
    {
        _logger.LogInformation("Starting sample data cleanup...");
        
        // Get admin user ID
        var admin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "admin@example.com");
        
        if (admin == null)
        {
            _logger.LogWarning("Admin user not found, skipping cleanup");
            return;
        }
        
        // Delete slips
        var slips = await _context.SlipVerifications
            .Where(s => s.UserId != admin.Id)
            .ToListAsync();
        _context.SlipVerifications.RemoveRange(slips);
        _logger.LogInformation("Removed {Count} sample slips", slips.Count);
        
        // Delete orders
        var orders = await _context.Orders
            .Where(o => o.UserId != admin.Id)
            .ToListAsync();
        _context.Orders.RemoveRange(orders);
        _logger.LogInformation("Removed {Count} sample orders", orders.Count);
        
        // Delete users
        var users = await _context.Users
            .Where(u => u.Id != admin.Id)
            .ToListAsync();
        _context.Users.RemoveRange(users);
        _logger.LogInformation("Removed {Count} sample users", users.Count);
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Sample data cleanup completed!");
    }
    
    /// <summary>
    /// Removes all data including admin
    /// </summary>
    public async Task CleanupAllDataAsync()
    {
        _logger.LogInformation("Starting full data cleanup...");
        
        _context.SlipVerifications.RemoveRange(_context.SlipVerifications);
        _context.Orders.RemoveRange(_context.Orders);
        _context.Users.RemoveRange(_context.Users);
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Full data cleanup completed!");
    }
}
```

## Best Practices

1. **Always backup before cleanup** in non-test environments
2. **Use transactions** for cleanup operations
3. **Log cleanup operations** for audit trail
4. **Handle foreign key constraints** properly
5. **Test cleanup scripts** in development first
6. **Document cleanup procedures** for team
7. **Automate common cleanup tasks**

## Environment-Specific Cleanup

### Development
- Can use aggressive cleanup (full reset)
- Automate cleanup in scripts
- Re-seed frequently

### Staging
- Use selective cleanup
- Preserve audit logs
- Coordinate with team

### Production
- **Never** use automated cleanup
- Manual, documented processes only
- Always backup first
- Review with team lead

## Testing Cleanup

For automated tests, use one of these patterns:

### Pattern 1: Transaction Per Test
```csharp
public class TestBase : IDisposable
{
    protected ApplicationDbContext Context;
    private IDbContextTransaction _transaction;
    
    public TestBase()
    {
        Context = CreateDbContext();
        _transaction = Context.Database.BeginTransaction();
    }
    
    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        Context?.Dispose();
    }
}
```

### Pattern 2: Database Per Test Class
```csharp
public class OrderTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    
    public OrderTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase(); // Cleanup before tests
    }
}
```

### Pattern 3: In-Memory Database
```csharp
protected ApplicationDbContext CreateInMemoryContext()
{
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    
    return new ApplicationDbContext(options);
}
```

## Monitoring Cleanup

Track cleanup operations:

```csharp
public class CleanupMetrics
{
    public DateTime CleanupTime { get; set; }
    public int UsersRemoved { get; set; }
    public int OrdersRemoved { get; set; }
    public int SlipsRemoved { get; set; }
    public TimeSpan Duration { get; set; }
}

public async Task<CleanupMetrics> CleanupWithMetricsAsync()
{
    var startTime = DateTime.UtcNow;
    var metrics = new CleanupMetrics { CleanupTime = startTime };
    
    // Perform cleanup and track counts
    // ...
    
    metrics.Duration = DateTime.UtcNow - startTime;
    _logger.LogInformation("Cleanup metrics: {@Metrics}", metrics);
    
    return metrics;
}
```

## Conclusion

Choose the cleanup strategy that fits your needs:
- **Testing**: Use transactions or in-memory databases
- **Development**: Full resets are fine
- **Staging**: Selective cleanup with backups
- **Production**: Manual, documented procedures only

Always test cleanup procedures before applying to important environments.
