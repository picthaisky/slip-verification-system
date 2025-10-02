using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.UnitTests.Helpers;

/// <summary>
/// Test data factory for creating test entities
/// </summary>
public static class TestDataFactory
{
    public static Order CreateOrder(
        Guid? id = null,
        Guid? userId = null,
        string? orderNumber = null,
        decimal? amount = null,
        OrderStatus? status = null)
    {
        return new Order
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            OrderNumber = orderNumber ?? $"ORD-{DateTime.UtcNow.Ticks}",
            Amount = amount ?? 1000m,
            Status = status ?? OrderStatus.PendingPayment,
            Description = "Test Order",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Domain.Entities.SlipVerification CreateSlipVerification(
        Guid? id = null,
        Guid? userId = null,
        Guid? orderId = null,
        string? imagePath = null,
        decimal? amount = null,
        string? bankName = null,
        string? referenceNumber = null,
        VerificationStatus? status = null)
    {
        return new Domain.Entities.SlipVerification
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            OrderId = orderId ?? Guid.NewGuid(),
            ImagePath = imagePath ?? "slips/test-slip.jpg",
            ImageHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(new byte[] { 0x01, 0x02, 0x03 })),
            Amount = amount,
            TransactionDate = DateTime.UtcNow.Date,
            TransactionTime = DateTime.UtcNow.TimeOfDay,
            ReferenceNumber = referenceNumber ?? $"REF{DateTime.UtcNow.Ticks}",
            BankName = bankName ?? "Test Bank",
            BankAccountNumber = "123-4-56789-0",
            Status = status ?? VerificationStatus.Pending,
            OcrConfidence = 0.95m,
            RawOcrText = "Test OCR Text",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static User CreateUser(
        Guid? id = null,
        string? email = null,
        string? fullName = null,
        UserRole? role = null)
    {
        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Username = $"testuser{DateTime.UtcNow.Ticks}",
            Email = email ?? $"testuser{DateTime.UtcNow.Ticks}@example.com",
            PasswordHash = "$2a$11$test.hash.value.for.testing.only",
            FullName = fullName ?? "Test User",
            Role = role ?? UserRole.User,
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static byte[] CreateValidJpegImageData()
    {
        // Minimal valid JPEG header
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
            0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01,
            0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9
        };
    }

    public static byte[] CreateValidPngImageData()
    {
        // Minimal valid PNG header
        return new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52
        };
    }

    public static byte[] CreateInvalidImageData()
    {
        return new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05 };
    }
}
