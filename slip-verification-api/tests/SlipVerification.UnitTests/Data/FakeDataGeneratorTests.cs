using SlipVerification.Domain.Enums;
using SlipVerification.Infrastructure.Data.Seeding;
using Xunit;

namespace SlipVerification.UnitTests.Data;

/// <summary>
/// Unit tests for FakeDataGenerator
/// </summary>
public class FakeDataGeneratorTests
{
    private readonly FakeDataGenerator _generator;

    public FakeDataGeneratorTests()
    {
        _generator = new FakeDataGenerator();
    }

    [Fact]
    public void GenerateUsers_ShouldCreateSpecifiedCount()
    {
        // Arrange
        const int count = 10;

        // Act
        var users = _generator.GenerateUsers(count);

        // Assert
        Assert.NotNull(users);
        Assert.Equal(count, users.Count);
    }

    [Fact]
    public void GenerateUsers_ShouldCreateUniqueUsers()
    {
        // Arrange
        const int count = 50;

        // Act
        var users = _generator.GenerateUsers(count);

        // Assert
        var uniqueEmails = users.Select(u => u.Email).Distinct().Count();
        var uniqueUsernames = users.Select(u => u.Username).Distinct().Count();
        var uniqueIds = users.Select(u => u.Id).Distinct().Count();
        
        Assert.Equal(count, uniqueEmails);
        Assert.Equal(count, uniqueUsernames);
        Assert.Equal(count, uniqueIds);
    }

    [Fact]
    public void GenerateUsers_ShouldHaveValidProperties()
    {
        // Arrange & Act
        var users = _generator.GenerateUsers(10);

        // Assert
        foreach (var user in users)
        {
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.NotEmpty(user.Email);
            Assert.Contains("@", user.Email);
            Assert.NotEmpty(user.Username);
            Assert.NotEmpty(user.PasswordHash);
            Assert.True(user.Role is UserRole.User or UserRole.Manager);
            Assert.True(user.CreatedAt <= DateTime.UtcNow);
        }
    }

    [Fact]
    public void GenerateOrders_ShouldCreateSpecifiedCount()
    {
        // Arrange
        const int count = 20;

        // Act
        var orders = _generator.GenerateOrders(count);

        // Assert
        Assert.NotNull(orders);
        Assert.Equal(count, orders.Count);
    }

    [Fact]
    public void GenerateOrders_ShouldHaveValidProperties()
    {
        // Arrange & Act
        var orders = _generator.GenerateOrders(10);

        // Assert
        foreach (var order in orders)
        {
            Assert.NotEqual(Guid.Empty, order.Id);
            Assert.NotEmpty(order.OrderNumber);
            Assert.True(order.Amount > 0);
            Assert.True(order.Amount >= 100 && order.Amount <= 10000);
            Assert.True(Enum.IsDefined(typeof(OrderStatus), order.Status));
            Assert.True(order.CreatedAt <= DateTime.UtcNow);
            Assert.NotNull(order.ExpiredAt);
        }
    }

    [Fact]
    public void GenerateOrders_PaidOrders_ShouldHavePaidAt()
    {
        // Arrange & Act
        var orders = _generator.GenerateOrders(100);
        var paidOrders = orders.Where(o => 
            o.Status == OrderStatus.Paid || 
            o.Status == OrderStatus.Completed).ToList();

        // Assert
        if (paidOrders.Any())
        {
            foreach (var order in paidOrders)
            {
                Assert.NotNull(order.PaidAt);
                Assert.True(order.PaidAt >= order.CreatedAt);
                Assert.True(order.PaidAt <= DateTime.UtcNow);
            }
        }
    }

    [Fact]
    public void GenerateSlips_ShouldCreateSpecifiedCount()
    {
        // Arrange
        const int count = 15;

        // Act
        var slips = _generator.GenerateSlips(count);

        // Assert
        Assert.NotNull(slips);
        Assert.Equal(count, slips.Count);
    }

    [Fact]
    public void GenerateSlips_ShouldHaveValidProperties()
    {
        // Arrange & Act
        var slips = _generator.GenerateSlips(10);

        // Assert
        foreach (var slip in slips)
        {
            Assert.NotEqual(Guid.Empty, slip.Id);
            Assert.NotEmpty(slip.ImagePath);
            Assert.Contains("slips/", slip.ImagePath);
            Assert.NotEmpty(slip.ImageHash);
            Assert.NotNull(slip.Amount);
            Assert.True(slip.Amount > 0);
            Assert.NotNull(slip.TransactionDate);
            Assert.NotNull(slip.TransactionTime);
            Assert.NotEmpty(slip.ReferenceNumber);
            Assert.NotEmpty(slip.BankName);
            Assert.True(Enum.IsDefined(typeof(VerificationStatus), slip.Status));
            Assert.NotNull(slip.OcrConfidence);
            Assert.True(slip.OcrConfidence >= 0.7m && slip.OcrConfidence <= 0.99m);
            Assert.True(slip.CreatedAt <= DateTime.UtcNow);
        }
    }

    [Fact]
    public void GenerateSlips_VerifiedSlips_ShouldHaveVerifiedAt()
    {
        // Arrange & Act
        var slips = _generator.GenerateSlips(100);
        var verifiedSlips = slips.Where(s => 
            s.Status == VerificationStatus.Verified || 
            s.Status == VerificationStatus.Rejected).ToList();

        // Assert
        if (verifiedSlips.Any())
        {
            foreach (var slip in verifiedSlips)
            {
                Assert.NotNull(slip.VerifiedAt);
                Assert.True(slip.VerifiedAt >= slip.CreatedAt);
                Assert.True(slip.VerifiedAt <= DateTime.UtcNow);
            }
        }
    }

    [Fact]
    public void GenerateUsers_MultipleInvocations_ShouldProduceDifferentData()
    {
        // Arrange & Act
        var users1 = _generator.GenerateUsers(5);
        var users2 = _generator.GenerateUsers(5);

        // Assert
        var emails1 = users1.Select(u => u.Email).ToList();
        var emails2 = users2.Select(u => u.Email).ToList();
        
        Assert.NotEqual(emails1, emails2);
    }
}
