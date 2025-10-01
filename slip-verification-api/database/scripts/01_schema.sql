-- =============================================
-- Slip Verification System - Database Schema
-- PostgreSQL 16
-- Entity Framework Core 9 Compatible
-- =============================================
-- Author: Database Architect
-- Date: 2025-01-01
-- Description: Complete database schema for QR Code payment verification system
--              Designed to handle 1,000+ transactions per day
-- =============================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- =============================================
-- Drop existing tables (for clean install)
-- =============================================
DROP TABLE IF EXISTS "AuditLogs" CASCADE;
DROP TABLE IF EXISTS "Notifications" CASCADE;
DROP TABLE IF EXISTS "Transactions" CASCADE;
DROP TABLE IF EXISTS "SlipVerifications" CASCADE;
DROP TABLE IF EXISTS "Orders" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;

-- Drop existing views
DROP VIEW IF EXISTS v_daily_transaction_summary CASCADE;
DROP VIEW IF EXISTS v_user_statistics CASCADE;

-- Drop existing functions
DROP FUNCTION IF EXISTS update_updated_at_column() CASCADE;
DROP FUNCTION IF EXISTS audit_log_function() CASCADE;

-- =============================================
-- Table: Users
-- Description: System users with authentication
-- =============================================
CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "Username" VARCHAR(100) UNIQUE NOT NULL,
    "Email" VARCHAR(255) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(500) NOT NULL,
    "FullName" VARCHAR(255),
    "PhoneNumber" VARCHAR(20),
    "Role" VARCHAR(50) NOT NULL DEFAULT 'User',
    "LineNotifyToken" VARCHAR(255),
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "EmailVerified" BOOLEAN NOT NULL DEFAULT false,
    "LastLoginAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" UUID,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "DeletedAt" TIMESTAMP,
    CONSTRAINT "CHK_Users_Email" CHECK ("Email" ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$')
);

-- Indexes for Users
CREATE INDEX "IX_Users_Email" ON "Users"("Email") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_Users_Username" ON "Users"("Username") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_Users_IsDeleted" ON "Users"("IsDeleted");
CREATE INDEX "IX_Users_Role" ON "Users"("Role") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_Users_EmailVerified" ON "Users"("EmailVerified") WHERE "DeletedAt" IS NULL;

-- =============================================
-- Table: Orders
-- Description: Customer payment orders
-- =============================================
CREATE TABLE "Orders" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderNumber" VARCHAR(50) UNIQUE NOT NULL,
    "UserId" UUID NOT NULL,
    "Amount" DECIMAL(12,2) NOT NULL,
    "Description" VARCHAR(500),
    "QrCodeData" TEXT,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "Notes" VARCHAR(1000),
    "PaidAt" TIMESTAMP,
    "ExpiredAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" UUID,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "DeletedAt" TIMESTAMP,
    CONSTRAINT "CHK_Orders_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_Orders_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- Indexes for Orders
CREATE INDEX "IX_Orders_UserId" ON "Orders"("UserId") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_Orders_Status" ON "Orders"("Status") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_Orders_CreatedAt" ON "Orders"("CreatedAt" DESC);
CREATE INDEX "IX_Orders_OrderNumber" ON "Orders"("OrderNumber") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_Orders_IsDeleted" ON "Orders"("IsDeleted");
CREATE INDEX "IX_Orders_ExpiredAt" ON "Orders"("ExpiredAt") WHERE "DeletedAt" IS NULL AND "ExpiredAt" IS NOT NULL;

-- =============================================
-- Table: SlipVerifications
-- Description: Payment slip verification records
-- =============================================
CREATE TABLE "SlipVerifications" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "ImagePath" VARCHAR(500) NOT NULL,
    "ImageHash" VARCHAR(64),
    "Amount" DECIMAL(12,2),
    "TransactionDate" DATE,
    "TransactionTime" TIME,
    "ReferenceNumber" VARCHAR(100),
    "BankName" VARCHAR(100),
    "BankAccountNumber" VARCHAR(50),
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Processing',
    "RawOcrText" TEXT,
    "OcrConfidence" DECIMAL(5,2),
    "VerificationNotes" VARCHAR(1000),
    "VerifiedBy" UUID,
    "VerifiedAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" UUID,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "DeletedAt" TIMESTAMP,
    CONSTRAINT "FK_SlipVerifications_Orders" FOREIGN KEY ("OrderId") 
        REFERENCES "Orders"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SlipVerifications_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_SlipVerifications_Verifiers" FOREIGN KEY ("VerifiedBy") 
        REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- Indexes for SlipVerifications
CREATE INDEX "IX_SlipVerifications_OrderId" ON "SlipVerifications"("OrderId");
CREATE INDEX "IX_SlipVerifications_UserId" ON "SlipVerifications"("UserId");
CREATE INDEX "IX_SlipVerifications_ReferenceNumber" ON "SlipVerifications"("ReferenceNumber") WHERE "DeletedAt" IS NULL;
CREATE INDEX "IX_SlipVerifications_Status" ON "SlipVerifications"("Status") WHERE "DeletedAt" IS NULL;
CREATE UNIQUE INDEX "IX_SlipVerifications_ImageHash" ON "SlipVerifications"("ImageHash") WHERE "DeletedAt" IS NULL AND "ImageHash" IS NOT NULL;
CREATE INDEX "IX_SlipVerifications_CreatedAt" ON "SlipVerifications"("CreatedAt");
CREATE INDEX "IX_SlipVerifications_IsDeleted" ON "SlipVerifications"("IsDeleted");
CREATE INDEX "IX_SlipVerifications_TransactionDate" ON "SlipVerifications"("TransactionDate") WHERE "DeletedAt" IS NULL;

-- =============================================
-- Table: Transactions
-- Description: Payment transaction records
-- =============================================
CREATE TABLE "Transactions" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "OrderId" UUID NOT NULL,
    "SlipVerificationId" UUID,
    "UserId" UUID NOT NULL,
    "Amount" DECIMAL(12,2) NOT NULL,
    "TransactionType" VARCHAR(50) NOT NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "PaymentMethod" VARCHAR(50),
    "Description" TEXT,
    "Metadata" JSONB,
    "ProcessedAt" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" UUID,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "DeletedAt" TIMESTAMP,
    CONSTRAINT "CHK_Transactions_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_Transactions_Orders" FOREIGN KEY ("OrderId") 
        REFERENCES "Orders"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Transactions_SlipVerifications" FOREIGN KEY ("SlipVerificationId") 
        REFERENCES "SlipVerifications"("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Transactions_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE RESTRICT
);

-- Indexes for Transactions
CREATE INDEX "IX_Transactions_OrderId" ON "Transactions"("OrderId");
CREATE INDEX "IX_Transactions_UserId" ON "Transactions"("UserId");
CREATE INDEX "IX_Transactions_Status" ON "Transactions"("Status");
CREATE INDEX "IX_Transactions_CreatedAt" ON "Transactions"("CreatedAt" DESC);
CREATE INDEX "IX_Transactions_IsDeleted" ON "Transactions"("IsDeleted");
CREATE INDEX "IX_Transactions_Metadata" ON "Transactions" USING GIN("Metadata");
CREATE INDEX "IX_Transactions_ProcessedAt" ON "Transactions"("ProcessedAt") WHERE "ProcessedAt" IS NOT NULL;

-- =============================================
-- Table: Notifications
-- Description: User notifications (LINE, EMAIL, etc.)
-- =============================================
CREATE TABLE "Notifications" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Title" VARCHAR(255) NOT NULL,
    "Message" TEXT NOT NULL,
    "Data" JSONB,
    "Channel" VARCHAR(50) NOT NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "SentAt" TIMESTAMP,
    "ReadAt" TIMESTAMP,
    "ErrorMessage" TEXT,
    "RetryCount" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" UUID,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "DeletedAt" TIMESTAMP,
    CONSTRAINT "FK_Notifications_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE CASCADE
);

-- Indexes for Notifications
CREATE INDEX "IX_Notifications_UserId" ON "Notifications"("UserId");
CREATE INDEX "IX_Notifications_Status" ON "Notifications"("Status");
CREATE INDEX "IX_Notifications_CreatedAt" ON "Notifications"("CreatedAt" DESC);
CREATE INDEX "IX_Notifications_ReadAt" ON "Notifications"("ReadAt") WHERE "ReadAt" IS NULL;
CREATE INDEX "IX_Notifications_IsDeleted" ON "Notifications"("IsDeleted");
CREATE INDEX "IX_Notifications_Channel" ON "Notifications"("Channel") WHERE "DeletedAt" IS NULL;

-- =============================================
-- Table: AuditLogs
-- Description: System audit trail
-- =============================================
CREATE TABLE "AuditLogs" (
    "Id" UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    "UserId" UUID,
    "EntityType" VARCHAR(100) NOT NULL,
    "EntityId" UUID NOT NULL,
    "Action" VARCHAR(50) NOT NULL,
    "OldValues" JSONB,
    "NewValues" JSONB,
    "IpAddress" VARCHAR(45),
    "UserAgent" TEXT,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatedBy" UUID,
    "UpdatedAt" TIMESTAMP,
    "UpdatedBy" UUID,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT false,
    "DeletedAt" TIMESTAMP,
    CONSTRAINT "FK_AuditLogs_Users" FOREIGN KEY ("UserId") 
        REFERENCES "Users"("Id") ON DELETE SET NULL
);

-- Indexes for AuditLogs
CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs"("UserId");
CREATE INDEX "IX_AuditLogs_Entity" ON "AuditLogs"("EntityType", "EntityId");
CREATE INDEX "IX_AuditLogs_CreatedAt" ON "AuditLogs"("CreatedAt" DESC);
CREATE INDEX "IX_AuditLogs_Action" ON "AuditLogs"("Action");

-- =============================================
-- Functions: Auto Update Timestamp
-- =============================================
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW."UpdatedAt" = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Apply triggers to tables
CREATE TRIGGER update_users_updated_at 
    BEFORE UPDATE ON "Users"
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_orders_updated_at 
    BEFORE UPDATE ON "Orders"
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_slipverifications_updated_at 
    BEFORE UPDATE ON "SlipVerifications"
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_transactions_updated_at 
    BEFORE UPDATE ON "Transactions"
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_notifications_updated_at 
    BEFORE UPDATE ON "Notifications"
    FOR EACH ROW 
    EXECUTE FUNCTION update_updated_at_column();

-- =============================================
-- Functions: Audit Log Trigger
-- =============================================
CREATE OR REPLACE FUNCTION audit_log_function()
RETURNS TRIGGER AS $$
BEGIN
    IF (TG_OP = 'UPDATE') THEN
        INSERT INTO "AuditLogs" ("EntityType", "EntityId", "Action", "OldValues", "NewValues")
        VALUES (TG_TABLE_NAME, OLD."Id", 'UPDATE', row_to_json(OLD), row_to_json(NEW));
        RETURN NEW;
    ELSIF (TG_OP = 'INSERT') THEN
        INSERT INTO "AuditLogs" ("EntityType", "EntityId", "Action", "NewValues")
        VALUES (TG_TABLE_NAME, NEW."Id", 'CREATE', row_to_json(NEW));
        RETURN NEW;
    ELSIF (TG_OP = 'DELETE') THEN
        INSERT INTO "AuditLogs" ("EntityType", "EntityId", "Action", "OldValues")
        VALUES (TG_TABLE_NAME, OLD."Id", 'DELETE', row_to_json(OLD));
        RETURN OLD;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Apply audit triggers (optional - can be enabled per table as needed)
-- CREATE TRIGGER audit_users AFTER INSERT OR UPDATE OR DELETE ON "Users"
--     FOR EACH ROW EXECUTE FUNCTION audit_log_function();

-- CREATE TRIGGER audit_orders AFTER INSERT OR UPDATE OR DELETE ON "Orders"
--     FOR EACH ROW EXECUTE FUNCTION audit_log_function();

-- CREATE TRIGGER audit_transactions AFTER INSERT OR UPDATE OR DELETE ON "Transactions"
--     FOR EACH ROW EXECUTE FUNCTION audit_log_function();

-- =============================================
-- Views: Daily Transaction Summary
-- =============================================
CREATE VIEW v_daily_transaction_summary AS
SELECT 
    DATE("CreatedAt") as transaction_date,
    COUNT(*) as total_transactions,
    COUNT(*) FILTER (WHERE "Status" = 'Completed') as completed_count,
    COUNT(*) FILTER (WHERE "Status" = 'Failed') as failed_count,
    COUNT(*) FILTER (WHERE "Status" = 'Pending') as pending_count,
    SUM("Amount") FILTER (WHERE "Status" = 'Completed') as total_amount,
    AVG("Amount") FILTER (WHERE "Status" = 'Completed') as average_amount
FROM "Transactions"
WHERE "DeletedAt" IS NULL
GROUP BY DATE("CreatedAt")
ORDER BY transaction_date DESC;

-- =============================================
-- Views: User Statistics
-- =============================================
CREATE VIEW v_user_statistics AS
SELECT 
    u."Id",
    u."Username",
    u."Email",
    u."FullName",
    COUNT(DISTINCT o."Id") as total_orders,
    COUNT(DISTINCT t."Id") as total_transactions,
    SUM(t."Amount") FILTER (WHERE t."Status" = 'Completed') as total_spent,
    COUNT(DISTINCT sv."Id") as total_slips_uploaded,
    MAX(t."CreatedAt") as last_transaction_date,
    MAX(u."LastLoginAt") as last_login_date
FROM "Users" u
LEFT JOIN "Orders" o ON u."Id" = o."UserId" AND o."DeletedAt" IS NULL
LEFT JOIN "Transactions" t ON o."Id" = t."OrderId" AND t."DeletedAt" IS NULL
LEFT JOIN "SlipVerifications" sv ON u."Id" = sv."UserId" AND sv."DeletedAt" IS NULL
WHERE u."DeletedAt" IS NULL
GROUP BY u."Id", u."Username", u."Email", u."FullName";

-- =============================================
-- Comments on tables
-- =============================================
COMMENT ON TABLE "Users" IS 'System users with authentication and authorization';
COMMENT ON TABLE "Orders" IS 'Customer payment orders awaiting verification';
COMMENT ON TABLE "SlipVerifications" IS 'Payment slip verification records with OCR data';
COMMENT ON TABLE "Transactions" IS 'Financial transaction records';
COMMENT ON TABLE "Notifications" IS 'User notifications via multiple channels';
COMMENT ON TABLE "AuditLogs" IS 'System audit trail for compliance';

-- =============================================
-- Grant permissions (adjust as needed)
-- =============================================
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app_user;
-- GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA public TO app_user;

-- =============================================
-- Performance optimization settings
-- =============================================
-- ALTER TABLE "Transactions" SET (autovacuum_vacuum_scale_factor = 0.05);
-- ALTER TABLE "AuditLogs" SET (autovacuum_vacuum_scale_factor = 0.05);
-- ALTER TABLE "Notifications" SET (autovacuum_vacuum_scale_factor = 0.1);

-- =============================================
-- End of schema
-- =============================================
