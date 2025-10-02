using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Data.Seeding;

/// <summary>
/// Test data fixtures for unit and integration testing
/// </summary>
public static class TestDataFixtures
{
    /// <summary>
    /// Creates a test user with specified email
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="role">User role</param>
    /// <returns>Test user</returns>
    public static User CreateTestUser(
        string email = "test@example.com",
        UserRole role = UserRole.User)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = email.Split('@')[0],
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            FullName = "Test User",
            Role = role,
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a test order with specified parameters
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="amount">Order amount</param>
    /// <param name="status">Order status</param>
    /// <returns>Test order</returns>
    public static Order CreateTestOrder(
        Guid userId, 
        decimal amount = 1000m,
        OrderStatus status = OrderStatus.PendingPayment)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}",
            UserId = userId,
            Amount = amount,
            Description = "Test order",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            ExpiredAt = DateTime.UtcNow.AddDays(7)
        };
    }
    
    /// <summary>
    /// Creates a test slip verification with specified parameters
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="amount">Slip amount</param>
    /// <param name="status">Verification status</param>
    /// <returns>Test slip verification</returns>
    public static Domain.Entities.SlipVerification CreateTestSlip(
        Guid orderId, 
        Guid userId, 
        decimal amount,
        VerificationStatus status = VerificationStatus.Verified)
    {
        return new Domain.Entities.SlipVerification
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            ImagePath = "test/slip.jpg",
            ImageHash = Guid.NewGuid().ToString("N"),
            Amount = amount,
            TransactionDate = DateTime.UtcNow.Date,
            TransactionTime = DateTime.UtcNow.TimeOfDay,
            ReferenceNumber = "REF" + Guid.NewGuid().ToString("N")[..10],
            BankName = "Test Bank",
            Status = status,
            OcrConfidence = 0.95m,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Creates a test admin user
    /// </summary>
    /// <returns>Test admin user</returns>
    public static User CreateTestAdmin()
    {
        return CreateTestUser("admin@test.com", UserRole.Admin);
    }
    
    /// <summary>
    /// Creates a test manager user
    /// </summary>
    /// <returns>Test manager user</returns>
    public static User CreateTestManager()
    {
        return CreateTestUser("manager@test.com", UserRole.Manager);
    }
    
    /// <summary>
    /// Creates a batch of test users
    /// </summary>
    /// <param name="count">Number of users to create</param>
    /// <returns>List of test users</returns>
    public static List<User> CreateTestUsers(int count)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateTestUser($"test{i}@example.com"));
        }
        return users;
    }
    
    /// <summary>
    /// Creates a batch of test orders
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="count">Number of orders to create</param>
    /// <returns>List of test orders</returns>
    public static List<Order> CreateTestOrders(Guid userId, int count)
    {
        var orders = new List<Order>();
        for (int i = 0; i < count; i++)
        {
            var amount = 1000m + (i * 100);
            orders.Add(CreateTestOrder(userId, amount));
        }
        return orders;
    }
}
