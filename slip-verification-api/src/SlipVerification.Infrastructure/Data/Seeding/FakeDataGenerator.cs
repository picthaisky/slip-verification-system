using Bogus;
using SlipVerification.Domain.Entities;
using SlipVerification.Domain.Enums;

namespace SlipVerification.Infrastructure.Data.Seeding;

/// <summary>
/// Fake data generator using Bogus library
/// </summary>
public class FakeDataGenerator
{
    private readonly Faker<User> _userFaker;
    private readonly Faker<Order> _orderFaker;
    private readonly Faker<Domain.Entities.SlipVerification> _slipFaker;
    
    public FakeDataGenerator()
    {
        // Configure user faker
        _userFaker = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Username, f => f.Internet.UserName())
            .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("Password123!"))
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("##########"))
            .RuleFor(u => u.Role, f => f.PickRandom(UserRole.User, UserRole.Manager))
            .RuleFor(u => u.EmailVerified, f => f.Random.Bool(0.8f)) // 80% verified
            .RuleFor(u => u.IsActive, f => true)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1))
            .RuleFor(u => u.LastLoginAt, (f, u) => 
                u.IsActive ? f.Date.Between(u.CreatedAt, DateTime.UtcNow) : null);
        
        // Configure order faker
        _orderFaker = new Faker<Order>()
            .RuleFor(o => o.Id, f => Guid.NewGuid())
            .RuleFor(o => o.OrderNumber, f => f.Random.AlphaNumeric(10).ToUpper())
            .RuleFor(o => o.Amount, f => f.Finance.Amount(100, 10000, 2))
            .RuleFor(o => o.Description, f => f.Commerce.ProductDescription())
            .RuleFor(o => o.Status, f => f.PickRandom<OrderStatus>())
            .RuleFor(o => o.CreatedAt, f => f.Date.Past(30))
            .RuleFor(o => o.PaidAt, (f, o) => 
                o.Status == OrderStatus.Paid || o.Status == OrderStatus.Completed ? 
                f.Date.Between(o.CreatedAt, DateTime.UtcNow) : null)
            .RuleFor(o => o.ExpiredAt, (f, o) => 
                o.CreatedAt.AddDays(7))
            .RuleFor(o => o.QrCodeData, f => 
                f.Random.Bool(0.7f) ? f.Random.AlphaNumeric(32) : null)
            .RuleFor(o => o.Notes, f => 
                f.Random.Bool(0.3f) ? f.Lorem.Sentence() : null);
        
        // Configure slip faker
        _slipFaker = new Faker<Domain.Entities.SlipVerification>()
            .RuleFor(s => s.Id, f => Guid.NewGuid())
            .RuleFor(s => s.ImagePath, f => $"slips/{f.Random.Guid()}.jpg")
            .RuleFor(s => s.ImageHash, f => f.Random.Hash(64))
            .RuleFor(s => s.Amount, f => f.Finance.Amount(100, 10000, 2))
            .RuleFor(s => s.TransactionDate, f => f.Date.Past(7).Date)
            .RuleFor(s => s.TransactionTime, f => f.Date.Past().TimeOfDay)
            .RuleFor(s => s.ReferenceNumber, f => f.Random.AlphaNumeric(12).ToUpper())
            .RuleFor(s => s.BankName, f => f.PickRandom("Bangkok Bank", "Kasikorn Bank", "SCB", "Krungthai Bank", "TMB"))
            .RuleFor(s => s.BankAccountNumber, f => f.Finance.Account(10))
            .RuleFor(s => s.Status, f => f.PickRandom<VerificationStatus>())
            .RuleFor(s => s.OcrConfidence, f => f.Random.Decimal(0.7m, 0.99m))
            .RuleFor(s => s.RawOcrText, f => 
                f.Random.Bool(0.8f) ? f.Lorem.Paragraph() : null)
            .RuleFor(s => s.VerificationNotes, (f, s) => 
                s.Status == VerificationStatus.Failed || s.Status == VerificationStatus.Rejected ? 
                f.Lorem.Sentence() : null)
            .RuleFor(s => s.CreatedAt, f => f.Date.Past(7))
            .RuleFor(s => s.VerifiedAt, (f, s) => 
                s.Status == VerificationStatus.Verified || s.Status == VerificationStatus.Rejected ?
                f.Date.Between(s.CreatedAt, DateTime.UtcNow) : null);
    }
    
    /// <summary>
    /// Generates fake users
    /// </summary>
    /// <param name="count">Number of users to generate</param>
    /// <returns>List of generated users</returns>
    public List<User> GenerateUsers(int count) => _userFaker.Generate(count);
    
    /// <summary>
    /// Generates fake orders
    /// </summary>
    /// <param name="count">Number of orders to generate</param>
    /// <returns>List of generated orders</returns>
    public List<Order> GenerateOrders(int count) => _orderFaker.Generate(count);
    
    /// <summary>
    /// Generates fake slip verifications
    /// </summary>
    /// <param name="count">Number of slips to generate</param>
    /// <returns>List of generated slip verifications</returns>
    public List<Domain.Entities.SlipVerification> GenerateSlips(int count) => _slipFaker.Generate(count);
}
