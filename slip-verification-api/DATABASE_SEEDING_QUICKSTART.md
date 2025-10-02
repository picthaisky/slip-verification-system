# Database Seeding - Quick Start Guide

Get started with database seeding in under 5 minutes!

## 🚀 Quick Commands

### Seed Development Database
```bash
cd slip-verification-api
./seed-examples.sh seed-dev
```

### Seed Production Database
```bash
cd slip-verification-api
./seed-examples.sh seed-prod
```

### Manual Seeding
```bash
cd src/SlipVerification.API
dotnet run seed
```

## 📦 What Gets Seeded?

### All Environments
- ✅ **Admin User**
  - Email: `admin@example.com`
  - Password: `Admin@123456`
  - Role: Admin
  - Status: Active, Email Verified

### Development Only
- ✅ **50 Sample Users** with realistic data
- ✅ **200 Sample Orders** with various statuses
- ✅ **150 Sample Slips** with different verification states

## 🔧 Configuration

### Environment Variable
```bash
# Development (seeds everything)
export ASPNETCORE_ENVIRONMENT=Development

# Production (admin only)
export ASPNETCORE_ENVIRONMENT=Production
```

### Connection String
Located in `src/SlipVerification.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SlipVerificationDb;Username=postgres;Password=postgres"
  }
}
```

## 🐳 Docker Usage

### Development
```yaml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
```

The `docker-entrypoint.sh` automatically:
1. Waits for database
2. Runs migrations
3. Seeds data (in Development)
4. Starts application

### Production
```yaml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

Only seeds admin user, no sample data.

## 💻 Programmatic Usage

### Using DatabaseSeeder
```csharp
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();
var seeder = new DatabaseSeeder(context, logger);

await seeder.SeedAsync();
```

### Using FakeDataGenerator
```csharp
var generator = new FakeDataGenerator();

// Generate users
var users = generator.GenerateUsers(50);

// Generate orders
var orders = generator.GenerateOrders(200);

// Generate slips
var slips = generator.GenerateSlips(150);
```

### Using TestDataFixtures
```csharp
// Create single entities
var user = TestDataFixtures.CreateTestUser("john@example.com");
var admin = TestDataFixtures.CreateTestAdmin();
var order = TestDataFixtures.CreateTestOrder(user.Id, 1000m);
var slip = TestDataFixtures.CreateTestSlip(order.Id, user.Id, 1000m);

// Create multiple entities
var users = TestDataFixtures.CreateTestUsers(10);
var orders = TestDataFixtures.CreateTestOrders(userId, 5);
```

## 🧪 Testing Examples

### Unit Test with Fixtures
```csharp
[Fact]
public async Task CreateOrder_ShouldSucceed()
{
    // Arrange
    var user = TestDataFixtures.CreateTestUser();
    var order = TestDataFixtures.CreateTestOrder(user.Id);
    
    await _context.Users.AddAsync(user);
    await _context.Orders.AddAsync(order);
    await _context.SaveChangesAsync();
    
    // Act
    var result = await _orderService.GetByIdAsync(order.Id);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(order.Amount, result.Amount);
}
```

### Integration Test with Generator
```csharp
[Fact]
public async Task BulkProcessing_ShouldSucceed()
{
    // Arrange
    var generator = new FakeDataGenerator();
    var users = generator.GenerateUsers(10);
    var orders = generator.GenerateOrders(100);
    
    await _context.Users.AddRangeAsync(users);
    await _context.SaveChangesAsync();
    
    // Assign orders to users
    var random = new Random();
    foreach (var order in orders)
    {
        order.UserId = users[random.Next(users.Count)].Id;
    }
    
    await _context.Orders.AddRangeAsync(orders);
    await _context.SaveChangesAsync();
    
    // Act & Assert
    var result = await _context.Orders.CountAsync();
    Assert.Equal(100, result);
}
```

## 🔄 Common Operations

### Reset Database
```bash
cd slip-verification-api
./seed-examples.sh cleanup
./seed-examples.sh seed-dev
```

### Re-seed (Idempotent)
```bash
# Safe to run multiple times
cd src/SlipVerification.API
dotnet run seed
```

The seeder checks if data exists before inserting.

### Check Seeded Data
```sql
-- Count users
SELECT COUNT(*) FROM "Users";

-- Count orders
SELECT COUNT(*) FROM "Orders";

-- Count slips
SELECT COUNT(*) FROM "SlipVerifications";

-- View admin user
SELECT "Email", "Username", "Role" FROM "Users" WHERE "Email" = 'admin@example.com';
```

## 🎯 Features

✅ **Idempotent** - Safe to run multiple times  
✅ **Environment-aware** - Different data per environment  
✅ **Bulk inserts** - Optimized performance  
✅ **Realistic data** - Using Bogus library  
✅ **Relationship integrity** - Proper foreign keys  
✅ **Comprehensive logging** - Track what's happening  
✅ **CLI support** - Easy command-line usage  
✅ **Docker integration** - Automatic seeding  
✅ **Test fixtures** - Ready for testing  

## 📊 Performance

Typical seeding times:
- Admin user: < 1 second
- 50 Users: 1-2 seconds  
- 200 Orders: 2-3 seconds  
- 150 Slips: 2-3 seconds  
- **Total**: ~5-10 seconds

## 🔐 Security

⚠️ **Important**: Change the default admin password after first login!

```csharp
// Default credentials (Development only)
Email: admin@example.com
Password: Admin@123456
```

In production, use environment variables or secure configuration.

## 🐛 Troubleshooting

### Issue: "Admin user already exists"
**Solution**: This is normal. The seeder is idempotent and skips existing data.

### Issue: "No database connection"
**Solution**: Check your connection string and ensure PostgreSQL is running.

### Issue: "Foreign key constraint violation"
**Solution**: Ensure you run migrations before seeding:
```bash
dotnet ef database update
```

### Issue: "Seed command not recognized"
**Solution**: Make sure you're in the API project directory:
```bash
cd src/SlipVerification.API
dotnet run seed
```

## 📚 Additional Resources

- [DATABASE_SEEDING.md](./DATABASE_SEEDING.md) - Complete documentation
- [seed-examples.sh](./seed-examples.sh) - Example scripts
- [docker-entrypoint.sh](./docker-entrypoint.sh) - Docker integration

## 🆘 Need Help?

1. Check logs for detailed error messages
2. Verify database connectivity
3. Ensure migrations are up to date
4. Review the complete documentation

## ⚡ Pro Tips

1. **Use fixtures for tests** - Faster and more consistent than generators
2. **Bulk operations** - Use `AddRangeAsync` for multiple entities
3. **Transaction scopes** - Wrap seeding in transactions for rollback capability
4. **Custom seeders** - Create specific seeders for different scenarios
5. **Verify relationships** - Always check foreign keys before bulk inserts

---

**Ready to go?** Run `./seed-examples.sh seed-dev` and you're all set! 🎉
