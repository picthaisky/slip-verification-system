using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Data.Seeding;

/// <summary>
/// Database seeder for initial and sample data
/// </summary>
public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    
    public DatabaseSeeder(
        ApplicationDbContext context,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Seeds database with initial and environment-specific data
    /// </summary>
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");
        
        // Always seed essential data
        await SeedAdminUserAsync();
        
        // Only seed sample data in development
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment == "Development")
        {
            _logger.LogInformation("Development environment detected, seeding sample data...");
            await SeedSampleUsersAsync();
            await SeedSampleOrdersAsync();
            await SeedSampleSlipsAsync();
        }
        
        _logger.LogInformation("Database seeding completed!");
    }
    
    /// <summary>
    /// Seeds the admin user (idempotent)
    /// </summary>
    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@example.com";
        
        if (await _context.Users.AnyAsync(u => u.Email == adminEmail))
        {
            _logger.LogInformation("Admin user already exists, skipping...");
            return;
        }
        
        var admin = new User
        {
            Email = adminEmail,
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
            FullName = "System Administrator",
            Role = UserRole.Admin,
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        await _context.Users.AddAsync(admin);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded admin user: {Email}", adminEmail);
    }
    
    /// <summary>
    /// Seeds sample users for development (idempotent)
    /// </summary>
    private async Task SeedSampleUsersAsync()
    {
        const int userCount = 50;
        
        var currentUserCount = await _context.Users.CountAsync();
        if (currentUserCount >= userCount)
        {
            _logger.LogInformation("Sample users already exist ({Count} users), skipping...", currentUserCount);
            return;
        }
        
        var generator = new FakeDataGenerator();
        var usersToCreate = userCount - currentUserCount;
        var users = generator.GenerateUsers(usersToCreate);
        
        // Bulk insert for performance
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} sample users", usersToCreate);
    }
    
    /// <summary>
    /// Seeds sample orders for development (idempotent)
    /// </summary>
    private async Task SeedSampleOrdersAsync()
    {
        const int orderCount = 200;
        
        var currentOrderCount = await _context.Orders.CountAsync();
        if (currentOrderCount >= orderCount)
        {
            _logger.LogInformation("Sample orders already exist ({Count} orders), skipping...", currentOrderCount);
            return;
        }
        
        var users = await _context.Users
            .Where(u => u.DeletedAt == null)
            .Take(50)
            .ToListAsync();
        
        if (!users.Any())
        {
            _logger.LogWarning("No users found to assign orders to. Skipping order seeding.");
            return;
        }
        
        var generator = new FakeDataGenerator();
        var ordersToCreate = orderCount - currentOrderCount;
        var orders = generator.GenerateOrders(ordersToCreate);
        
        // Assign random users to orders
        var random = new Random();
        foreach (var order in orders)
        {
            order.UserId = users[random.Next(users.Count)].Id;
        }
        
        // Bulk insert
        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} sample orders", ordersToCreate);
    }
    
    /// <summary>
    /// Seeds sample slip verifications for development (idempotent)
    /// </summary>
    private async Task SeedSampleSlipsAsync()
    {
        const int slipCount = 150;
        
        var currentSlipCount = await _context.SlipVerifications.CountAsync();
        if (currentSlipCount >= slipCount)
        {
            _logger.LogInformation("Sample slips already exist ({Count} slips), skipping...", currentSlipCount);
            return;
        }
        
        var orders = await _context.Orders
            .Include(o => o.User)
            .Where(o => o.DeletedAt == null)
            .Take(150)
            .ToListAsync();
        
        if (!orders.Any())
        {
            _logger.LogWarning("No orders found to assign slips to. Skipping slip seeding.");
            return;
        }
        
        var generator = new FakeDataGenerator();
        var slipsToCreate = Math.Min(slipCount - currentSlipCount, orders.Count);
        var slips = generator.GenerateSlips(slipsToCreate);
        
        // Assign slips to orders
        for (int i = 0; i < slipsToCreate; i++)
        {
            slips[i].OrderId = orders[i].Id;
            slips[i].UserId = orders[i].UserId;
            
            // Match slip amount with order amount for verified slips
            if (slips[i].Status == VerificationStatus.Verified)
            {
                slips[i].Amount = orders[i].Amount;
            }
        }
        
        // Bulk insert
        await _context.SlipVerifications.AddRangeAsync(slips);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} sample slips", slipsToCreate);
    }
}
