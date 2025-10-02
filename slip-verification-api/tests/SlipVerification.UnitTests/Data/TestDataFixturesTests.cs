using SlipVerification.Domain.Enums;
using SlipVerification.Infrastructure.Data.Seeding;
using Xunit;

namespace SlipVerification.UnitTests.Data;

/// <summary>
/// Unit tests for TestDataFixtures
/// </summary>
public class TestDataFixturesTests
{
    [Fact]
    public void CreateTestUser_WithDefaults_ShouldCreateValidUser()
    {
        // Act
        var user = TestDataFixtures.CreateTestUser();

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("test", user.Username);
        Assert.NotEmpty(user.PasswordHash);
        Assert.Equal("Test User", user.FullName);
        Assert.Equal(UserRole.User, user.Role);
        Assert.True(user.EmailVerified);
        Assert.True(user.IsActive);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void CreateTestUser_WithCustomEmail_ShouldUseProvidedEmail()
    {
        // Arrange
        const string email = "john@example.com";

        // Act
        var user = TestDataFixtures.CreateTestUser(email);

        // Assert
        Assert.Equal(email, user.Email);
        Assert.Equal("john", user.Username);
    }

    [Fact]
    public void CreateTestUser_WithCustomRole_ShouldUseProvidedRole()
    {
        // Act
        var user = TestDataFixtures.CreateTestUser("test@example.com", UserRole.Manager);

        // Assert
        Assert.Equal(UserRole.Manager, user.Role);
    }

    [Fact]
    public void CreateTestAdmin_ShouldCreateAdminUser()
    {
        // Act
        var admin = TestDataFixtures.CreateTestAdmin();

        // Assert
        Assert.Equal(UserRole.Admin, admin.Role);
        Assert.Contains("admin", admin.Email);
        Assert.True(admin.EmailVerified);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public void CreateTestManager_ShouldCreateManagerUser()
    {
        // Act
        var manager = TestDataFixtures.CreateTestManager();

        // Assert
        Assert.Equal(UserRole.Manager, manager.Role);
        Assert.Contains("manager", manager.Email);
        Assert.True(manager.EmailVerified);
        Assert.True(manager.IsActive);
    }

    [Fact]
    public void CreateTestOrder_WithDefaults_ShouldCreateValidOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var order = TestDataFixtures.CreateTestOrder(userId);

        // Assert
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(1000m, order.Amount);
        Assert.Equal(OrderStatus.PendingPayment, order.Status);
        Assert.NotEmpty(order.OrderNumber);
        Assert.Contains("ORD", order.OrderNumber);
        Assert.Equal("Test order", order.Description);
        Assert.True(order.CreatedAt <= DateTime.UtcNow);
        Assert.NotNull(order.ExpiredAt);
    }

    [Fact]
    public void CreateTestOrder_WithCustomAmount_ShouldUseProvidedAmount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const decimal amount = 5000m;

        // Act
        var order = TestDataFixtures.CreateTestOrder(userId, amount);

        // Assert
        Assert.Equal(amount, order.Amount);
    }

    [Fact]
    public void CreateTestOrder_WithCustomStatus_ShouldUseProvidedStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var order = TestDataFixtures.CreateTestOrder(userId, 1000m, OrderStatus.Paid);

        // Assert
        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void CreateTestSlip_WithDefaults_ShouldCreateValidSlip()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const decimal amount = 1000m;

        // Act
        var slip = TestDataFixtures.CreateTestSlip(orderId, userId, amount);

        // Assert
        Assert.NotEqual(Guid.Empty, slip.Id);
        Assert.Equal(orderId, slip.OrderId);
        Assert.Equal(userId, slip.UserId);
        Assert.Equal(amount, slip.Amount);
        Assert.Equal(VerificationStatus.Verified, slip.Status);
        Assert.Equal("test/slip.jpg", slip.ImagePath);
        Assert.NotEmpty(slip.ImageHash);
        Assert.Equal("Test Bank", slip.BankName);
        Assert.NotNull(slip.TransactionDate);
        Assert.NotNull(slip.TransactionTime);
        Assert.NotEmpty(slip.ReferenceNumber);
        Assert.Contains("REF", slip.ReferenceNumber);
        Assert.Equal(0.95m, slip.OcrConfidence);
        Assert.True(slip.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void CreateTestSlip_WithCustomStatus_ShouldUseProvidedStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var slip = TestDataFixtures.CreateTestSlip(
            orderId, userId, 1000m, VerificationStatus.Pending);

        // Assert
        Assert.Equal(VerificationStatus.Pending, slip.Status);
    }

    [Fact]
    public void CreateTestUsers_ShouldCreateSpecifiedCount()
    {
        // Arrange
        const int count = 5;

        // Act
        var users = TestDataFixtures.CreateTestUsers(count);

        // Assert
        Assert.Equal(count, users.Count);
    }

    [Fact]
    public void CreateTestUsers_ShouldCreateUniqueUsers()
    {
        // Arrange
        const int count = 10;

        // Act
        var users = TestDataFixtures.CreateTestUsers(count);

        // Assert
        var uniqueIds = users.Select(u => u.Id).Distinct().Count();
        var uniqueEmails = users.Select(u => u.Email).Distinct().Count();
        
        Assert.Equal(count, uniqueIds);
        Assert.Equal(count, uniqueEmails);
    }

    [Fact]
    public void CreateTestOrders_ShouldCreateSpecifiedCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int count = 5;

        // Act
        var orders = TestDataFixtures.CreateTestOrders(userId, count);

        // Assert
        Assert.Equal(count, orders.Count);
    }

    [Fact]
    public void CreateTestOrders_ShouldCreateUniqueOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int count = 10;

        // Act
        var orders = TestDataFixtures.CreateTestOrders(userId, count);

        // Assert
        var uniqueIds = orders.Select(o => o.Id).Distinct().Count();
        var uniqueAmounts = orders.Select(o => o.Amount).Distinct().Count();
        
        Assert.Equal(count, uniqueIds);
        Assert.Equal(count, uniqueAmounts); // Each order should have different amount
    }

    [Fact]
    public void CreateTestOrders_ShouldIncreaseAmountForEachOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int count = 5;

        // Act
        var orders = TestDataFixtures.CreateTestOrders(userId, count);

        // Assert
        for (int i = 0; i < count - 1; i++)
        {
            Assert.True(orders[i + 1].Amount > orders[i].Amount);
        }
    }

    [Fact]
    public void CreateTestOrders_AllOrdersShouldBelongToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const int count = 5;

        // Act
        var orders = TestDataFixtures.CreateTestOrders(userId, count);

        // Assert
        Assert.All(orders, order => Assert.Equal(userId, order.UserId));
    }
}
