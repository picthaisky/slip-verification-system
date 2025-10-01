-- =============================================
-- Slip Verification System - Seed Data
-- PostgreSQL 16
-- =============================================
-- Description: Initial seed data for development and testing
-- =============================================

-- =============================================
-- Seed Users
-- =============================================
-- Password: "Admin@123" (hashed - you should use proper password hashing in your application)
INSERT INTO "Users" ("Id", "Username", "Email", "PasswordHash", "FullName", "PhoneNumber", "Role", "IsActive", "EmailVerified", "CreatedAt")
VALUES 
    ('11111111-1111-1111-1111-111111111111', 'admin', 'admin@slipverification.com', 
     '$2a$11$KpgOjLgK1CjX9HQWFHXHX.KkVZ5H5P5Z5P5Z5P5Z5P5Z5P5Z5P5Z5', -- Placeholder hash
     'System Administrator', '0812345678', 'Admin', true, true, NOW()),
    ('22222222-2222-2222-2222-222222222222', 'user1', 'user1@example.com',
     '$2a$11$KpgOjLgK1CjX9HQWFHXHX.KkVZ5H5P5Z5P5Z5P5Z5P5Z5P5Z5P5Z5',
     'John Doe', '0823456789', 'User', true, true, NOW()),
    ('33333333-3333-3333-3333-333333333333', 'user2', 'user2@example.com',
     '$2a$11$KpgOjLgK1CjX9HQWFHXHX.KkVZ5H5P5Z5P5Z5P5Z5P5Z5P5Z5P5Z5',
     'Jane Smith', '0834567890', 'User', true, true, NOW()),
    ('44444444-4444-4444-4444-444444444444', 'merchant', 'merchant@example.com',
     '$2a$11$KpgOjLgK1CjX9HQWFHXHX.KkVZ5H5P5Z5P5Z5P5Z5P5Z5P5Z5P5Z5',
     'Merchant User', '0845678901', 'Merchant', true, true, NOW());

-- =============================================
-- Seed Orders
-- =============================================
INSERT INTO "Orders" ("Id", "OrderNumber", "UserId", "Amount", "Description", "Status", "CreatedAt")
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'ORD-2025-0001', '22222222-2222-2222-2222-222222222222', 
     1500.00, 'Product Purchase - Item #001', 'Pending', NOW() - INTERVAL '2 days'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'ORD-2025-0002', '22222222-2222-2222-2222-222222222222',
     2500.00, 'Product Purchase - Item #002', 'Completed', NOW() - INTERVAL '1 day'),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'ORD-2025-0003', '33333333-3333-3333-3333-333333333333',
     750.00, 'Service Payment - Monthly Subscription', 'Pending', NOW() - INTERVAL '3 hours'),
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'ORD-2025-0004', '33333333-3333-3333-3333-333333333333',
     3200.00, 'Product Bundle - Premium Package', 'Completed', NOW() - INTERVAL '5 days');

-- =============================================
-- Seed SlipVerifications
-- =============================================
INSERT INTO "SlipVerifications" ("Id", "OrderId", "UserId", "ImagePath", "ImageHash", "Amount", 
    "TransactionDate", "TransactionTime", "ReferenceNumber", "BankName", "BankAccountNumber", 
    "Status", "OcrConfidence", "VerifiedBy", "VerifiedAt", "CreatedAt")
VALUES
    ('11111111-aaaa-aaaa-aaaa-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 
     '22222222-2222-2222-2222-222222222222', '/uploads/slips/slip-001.jpg',
     'abcd1234567890abcd1234567890abcd1234567890abcd1234567890abcd1234', 2500.00,
     CURRENT_DATE - INTERVAL '1 day', '14:30:00', 'TXN2025010112345', 'กสิกรไทย', '123-4-56789-0',
     'Verified', 0.95, '11111111-1111-1111-1111-111111111111', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'),
    ('22222222-aaaa-aaaa-aaaa-222222222222', 'dddddddd-dddd-dddd-dddd-dddddddddddd',
     '33333333-3333-3333-3333-333333333333', '/uploads/slips/slip-002.jpg',
     'efgh1234567890efgh1234567890efgh1234567890efgh1234567890efgh1234', 3200.00,
     CURRENT_DATE - INTERVAL '5 days', '10:15:00', 'TXN2024122812345', 'ไทยพาณิชย์', '987-6-54321-0',
     'Verified', 0.92, '11111111-1111-1111-1111-111111111111', NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days');

-- =============================================
-- Seed Transactions
-- =============================================
INSERT INTO "Transactions" ("Id", "OrderId", "SlipVerificationId", "UserId", "Amount", 
    "TransactionType", "Status", "PaymentMethod", "Description", "ProcessedAt", "CreatedAt")
VALUES
    ('11111111-1111-aaaa-aaaa-111111111111', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
     '11111111-aaaa-aaaa-aaaa-111111111111', '22222222-2222-2222-2222-222222222222',
     2500.00, 'Payment', 'Completed', 'Bank Transfer', 'Payment via slip verification',
     NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'),
    ('22222222-2222-aaaa-aaaa-222222222222', 'dddddddd-dddd-dddd-dddd-dddddddddddd',
     '22222222-aaaa-aaaa-aaaa-222222222222', '33333333-3333-3333-3333-333333333333',
     3200.00, 'Payment', 'Completed', 'Bank Transfer', 'Payment via slip verification',
     NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days');

-- =============================================
-- Seed Notifications
-- =============================================
INSERT INTO "Notifications" ("Id", "UserId", "Type", "Title", "Message", "Channel", "Status", "SentAt", "CreatedAt")
VALUES
    ('11111111-1111-1111-aaaa-111111111111', '22222222-2222-2222-2222-222222222222',
     'OrderCreated', 'Order Created', 'Your order ORD-2025-0001 has been created successfully.',
     'EMAIL', 'Sent', NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days'),
    ('22222222-2222-2222-aaaa-222222222222', '22222222-2222-2222-2222-222222222222',
     'PaymentVerified', 'Payment Verified', 'Your payment for order ORD-2025-0002 has been verified.',
     'EMAIL', 'Sent', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day'),
    ('33333333-3333-3333-aaaa-333333333333', '33333333-3333-3333-3333-333333333333',
     'OrderCreated', 'Order Created', 'Your order ORD-2025-0003 has been created successfully.',
     'LINE', 'Pending', NULL, NOW() - INTERVAL '3 hours');

-- =============================================
-- Seed AuditLogs (sample entries)
-- =============================================
INSERT INTO "AuditLogs" ("Id", "UserId", "EntityType", "EntityId", "Action", "NewValues", "CreatedAt")
VALUES
    ('11111111-1111-1111-1111-aaaaaaaaaa11', '11111111-1111-1111-1111-111111111111',
     'Users', '22222222-2222-2222-2222-222222222222', 'CREATE',
     '{"Username": "user1", "Email": "user1@example.com", "Role": "User"}'::jsonb,
     NOW() - INTERVAL '10 days'),
    ('22222222-2222-2222-2222-aaaaaaaaaa22', '22222222-2222-2222-2222-222222222222',
     'Orders', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'CREATE',
     '{"OrderNumber": "ORD-2025-0001", "Amount": 1500.00, "Status": "Pending"}'::jsonb,
     NOW() - INTERVAL '2 days');

-- =============================================
-- Update Statistics
-- =============================================
ANALYZE "Users";
ANALYZE "Orders";
ANALYZE "SlipVerifications";
ANALYZE "Transactions";
ANALYZE "Notifications";
ANALYZE "AuditLogs";

-- =============================================
-- Verification queries (optional - for testing)
-- =============================================
-- SELECT COUNT(*) as user_count FROM "Users";
-- SELECT COUNT(*) as order_count FROM "Orders";
-- SELECT COUNT(*) as slip_count FROM "SlipVerifications";
-- SELECT COUNT(*) as transaction_count FROM "Transactions";
-- SELECT COUNT(*) as notification_count FROM "Notifications";
-- SELECT * FROM v_daily_transaction_summary;
-- SELECT * FROM v_user_statistics;

-- =============================================
-- End of seed data
-- =============================================
