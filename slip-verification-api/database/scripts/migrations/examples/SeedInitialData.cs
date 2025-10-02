using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations.Examples
{
    /// <summary>
    /// Example migration for seeding initial data
    /// This demonstrates how to seed default users, roles, and configuration data
    /// </summary>
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================================================
            // Seed Admin User
            // =============================================================================
            var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] 
                { 
                    "Id", 
                    "Email", 
                    "Username", 
                    "PasswordHash", 
                    "FullName", 
                    "Role", 
                    "EmailVerified", 
                    "IsActive", 
                    "CreatedAt", 
                    "UpdatedAt", 
                    "IsDeleted" 
                },
                values: new object[] 
                { 
                    adminId, 
                    "admin@slipverification.com", 
                    "admin", 
                    // BCrypt hash for "Admin@123456" (use proper hashing in production)
                    "$2a$11$KGJ5KGJ5KGJ5KGJ5KGJ5KOxQmq1h9ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5",
                    "System Administrator",
                    0, // Admin role enum value (UserRole.Admin)
                    true,
                    true,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false
                });

            // =============================================================================
            // Seed Manager User
            // =============================================================================
            var managerId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] 
                { 
                    "Id", 
                    "Email", 
                    "Username", 
                    "PasswordHash", 
                    "FullName", 
                    "Role", 
                    "EmailVerified", 
                    "IsActive", 
                    "CreatedAt", 
                    "UpdatedAt", 
                    "IsDeleted" 
                },
                values: new object[] 
                { 
                    managerId, 
                    "manager@slipverification.com", 
                    "manager", 
                    "$2a$11$KGJ5KGJ5KGJ5KGJ5KGJ5KOxQmq1h9ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5",
                    "System Manager",
                    1, // Manager role enum value (UserRole.Manager)
                    true,
                    true,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false
                });

            // =============================================================================
            // Seed Regular User (for testing)
            // =============================================================================
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000003");
            
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] 
                { 
                    "Id", 
                    "Email", 
                    "Username", 
                    "PasswordHash", 
                    "FullName", 
                    "Role", 
                    "EmailVerified", 
                    "IsActive", 
                    "CreatedAt", 
                    "UpdatedAt", 
                    "IsDeleted" 
                },
                values: new object[] 
                { 
                    userId, 
                    "user@slipverification.com", 
                    "user", 
                    "$2a$11$KGJ5KGJ5KGJ5KGJ5KGJ5KOxQmq1h9ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5ZQ5",
                    "Regular User",
                    2, // User role enum value (UserRole.User)
                    true,
                    true,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    false
                });

            // =============================================================================
            // Seed Notification Templates
            // =============================================================================
            
            // Using raw SQL for more complex seeding
            migrationBuilder.Sql(@"
                INSERT INTO ""NotificationTemplates"" 
                (""Id"", ""Code"", ""Name"", ""Channel"", ""Subject"", ""Body"", ""Language"", ""IsActive"", ""CreatedAt"", ""IsDeleted"")
                VALUES 
                -- Order Created Template
                (
                    gen_random_uuid(),
                    'ORDER_CREATED',
                    'Order Created Notification',
                    0, -- Email channel
                    'Your order {{OrderNumber}} has been created',
                    'Dear {{FullName}},

Your order has been created successfully!

Order Details:
- Order Number: {{OrderNumber}}
- Amount: {{Amount}} THB
- Created At: {{CreatedAt}}

Please upload your payment slip to complete the verification.

Best regards,
Slip Verification Team',
                    'en',
                    true,
                    NOW(),
                    false
                ),
                
                -- Slip Verified Template
                (
                    gen_random_uuid(),
                    'SLIP_VERIFIED',
                    'Payment Slip Verified Successfully',
                    0, -- Email channel
                    'Payment slip verified for order {{OrderNumber}}',
                    'Dear {{FullName}},

Your payment slip has been verified successfully!

Order Details:
- Order Number: {{OrderNumber}}
- Amount: {{Amount}} THB
- Verified At: {{VerifiedAt}}

Thank you for your payment!

Best regards,
Slip Verification Team',
                    'en',
                    true,
                    NOW(),
                    false
                ),
                
                -- Slip Rejected Template
                (
                    gen_random_uuid(),
                    'SLIP_REJECTED',
                    'Payment Slip Rejected',
                    0, -- Email channel
                    'Payment slip rejected for order {{OrderNumber}}',
                    'Dear {{FullName}},

Unfortunately, your payment slip could not be verified.

Order Details:
- Order Number: {{OrderNumber}}
- Amount: {{Amount}} THB
- Reason: {{RejectionReason}}

Please upload a new payment slip.

Best regards,
Slip Verification Team',
                    'en',
                    true,
                    NOW(),
                    false
                ),
                
                -- Welcome Email Template
                (
                    gen_random_uuid(),
                    'WELCOME_EMAIL',
                    'Welcome to Slip Verification System',
                    0, -- Email channel
                    'Welcome to Slip Verification System',
                    'Dear {{FullName}},

Welcome to Slip Verification System!

Your account has been created successfully. You can now start creating orders and verifying payment slips.

Account Details:
- Username: {{Username}}
- Email: {{Email}}

Please verify your email by clicking the link below:
{{VerificationLink}}

Best regards,
Slip Verification Team',
                    'en',
                    true,
                    NOW(),
                    false
                ),
                
                -- Password Reset Template
                (
                    gen_random_uuid(),
                    'PASSWORD_RESET',
                    'Password Reset Request',
                    0, -- Email channel
                    'Password Reset Request',
                    'Dear {{FullName}},

We received a request to reset your password.

Click the link below to reset your password:
{{ResetLink}}

This link will expire in 24 hours.

If you did not request a password reset, please ignore this email.

Best regards,
Slip Verification Team',
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
            // =============================================================================
            // Remove seeded notification templates
            // =============================================================================
            migrationBuilder.Sql(@"
                DELETE FROM ""NotificationTemplates""
                WHERE ""Code"" IN (
                    'ORDER_CREATED', 
                    'SLIP_VERIFIED', 
                    'SLIP_REJECTED', 
                    'WELCOME_EMAIL', 
                    'PASSWORD_RESET'
                );
            ");

            // =============================================================================
            // Remove seeded users
            // =============================================================================
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Email",
                keyValue: "admin@slipverification.com");
            
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Email",
                keyValue: "manager@slipverification.com");
            
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Email",
                keyValue: "user@slipverification.com");
        }
    }
}
