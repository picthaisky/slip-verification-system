# Entity Relationship Diagram (ERD)

## Overview
This document describes the entity relationships in the Slip Verification System database.

## ERD Diagram (Text Representation)

```
┌─────────────────┐
│     Users       │
├─────────────────┤
│ PK Id           │
│    Username     │
│    Email        │
│    PasswordHash │
│    FullName     │
│    PhoneNumber  │
│    Role         │
│    LineNotify...│
│    IsActive     │
│    EmailVerified│
│    LastLoginAt  │
│    CreatedAt    │
│    UpdatedAt    │
│    DeletedAt    │
└─────────────────┘
         │ 1
         │
         │ *
┌─────────────────┐
│     Orders      │
├─────────────────┤
│ PK Id           │
│    OrderNumber  │
│ FK UserId       │────────┐
│    Amount       │        │
│    Description  │        │
│    QrCodeData   │        │
│    Status       │        │
│    Notes        │        │
│    PaidAt       │        │
│    ExpiredAt    │        │
│    CreatedAt    │        │
│    UpdatedAt    │        │
│    DeletedAt    │        │
└─────────────────┘        │
         │ 1               │
         │                 │
         │ *               │
┌──────────────────────┐   │
│ SlipVerifications    │   │
├──────────────────────┤   │
│ PK Id                │   │
│ FK OrderId           │   │
│ FK UserId            │───┘
│    ImagePath         │
│    ImageHash         │
│    Amount            │
│    TransactionDate   │
│    TransactionTime   │
│    ReferenceNumber   │
│    BankName          │
│    BankAccountNumber │
│    Status            │
│    RawOcrText        │
│    OcrConfidence     │
│    VerificationNotes │
│ FK VerifiedBy        │───┐
│    VerifiedAt        │   │
│    CreatedAt         │   │
│    UpdatedAt         │   │
│    DeletedAt         │   │
└──────────────────────┘   │
         │ 1               │
         │                 │
         │ *               │
┌─────────────────┐        │
│  Transactions   │        │
├─────────────────┤        │
│ PK Id           │        │
│ FK OrderId      │        │
│ FK SlipVerif... │        │
│ FK UserId       │────────┤
│    Amount       │        │
│    TransactionType      │
│    Status       │        │
│    PaymentMethod│        │
│    Description  │        │
│    Metadata(JSON)       │
│    ProcessedAt  │        │
│    CreatedAt    │        │
│    UpdatedAt    │        │
│    DeletedAt    │        │
└─────────────────┘        │
                           │
┌─────────────────┐        │
│  Notifications  │        │
├─────────────────┤        │
│ PK Id           │        │
│ FK UserId       │────────┤
│    Type         │        │
│    Title        │        │
│    Message      │        │
│    Data (JSON)  │        │
│    Channel      │        │
│    Status       │        │
│    SentAt       │        │
│    ReadAt       │        │
│    ErrorMessage │        │
│    RetryCount   │        │
│    CreatedAt    │        │
│    UpdatedAt    │        │
│    DeletedAt    │        │
└─────────────────┘        │
                           │
┌─────────────────┐        │
│   AuditLogs     │        │
├─────────────────┤        │
│ PK Id           │        │
│ FK UserId       │────────┘
│    EntityType   │
│    EntityId     │
│    Action       │
│    OldValues(JSON)
│    NewValues(JSON)
│    IpAddress    │
│    UserAgent    │
│    CreatedAt    │
└─────────────────┘
```

## Entity Relationships

### 1. Users (Central Entity)
The Users table is the central entity that connects to almost all other tables.

**Outgoing Relationships:**
- **Users → Orders** (1:Many)
  - One user can have many orders
  - Foreign Key: Orders.UserId → Users.Id
  - Delete Rule: RESTRICT (prevent deletion of users with orders)

- **Users → SlipVerifications** (1:Many) - Uploader
  - One user can upload many slip verifications
  - Foreign Key: SlipVerifications.UserId → Users.Id
  - Delete Rule: RESTRICT

- **Users → SlipVerifications** (1:Many) - Verifier
  - One user can verify many slip verifications
  - Foreign Key: SlipVerifications.VerifiedBy → Users.Id
  - Delete Rule: RESTRICT

- **Users → Transactions** (1:Many)
  - One user can have many transactions
  - Foreign Key: Transactions.UserId → Users.Id
  - Delete Rule: RESTRICT

- **Users → Notifications** (1:Many)
  - One user can have many notifications
  - Foreign Key: Notifications.UserId → Users.Id
  - Delete Rule: CASCADE (delete notifications when user is deleted)

- **Users → AuditLogs** (1:Many)
  - One user can have many audit log entries
  - Foreign Key: AuditLogs.UserId → Users.Id
  - Delete Rule: SET NULL (keep audit logs even if user is deleted)

### 2. Orders
Orders represent payment requests that need verification.

**Relationships:**
- **Orders → Users** (Many:1)
  - Many orders belong to one user
  - Foreign Key: Orders.UserId → Users.Id

- **Orders → SlipVerifications** (1:Many)
  - One order can have multiple slip verification attempts
  - Foreign Key: SlipVerifications.OrderId → Orders.Id
  - Delete Rule: CASCADE (delete slips when order is deleted)

- **Orders → Transactions** (1:Many)
  - One order can have multiple transactions (refunds, partial payments)
  - Foreign Key: Transactions.OrderId → Orders.Id
  - Delete Rule: RESTRICT (prevent deletion of orders with transactions)

### 3. SlipVerifications
Slip verification records contain uploaded payment slips and OCR results.

**Relationships:**
- **SlipVerifications → Orders** (Many:1)
  - Many slips can be uploaded for one order (retries)
  - Foreign Key: SlipVerifications.OrderId → Orders.Id

- **SlipVerifications → Users** (Many:1) - Uploader
  - Many slips uploaded by one user
  - Foreign Key: SlipVerifications.UserId → Users.Id

- **SlipVerifications → Users** (Many:1) - Verifier
  - Many slips verified by one admin user
  - Foreign Key: SlipVerifications.VerifiedBy → Users.Id

- **SlipVerifications → Transactions** (1:Many)
  - One verified slip can generate one or more transactions
  - Foreign Key: Transactions.SlipVerificationId → SlipVerifications.Id
  - Delete Rule: RESTRICT

### 4. Transactions
Financial transaction records for payment processing.

**Relationships:**
- **Transactions → Orders** (Many:1)
  - Many transactions can belong to one order
  - Foreign Key: Transactions.OrderId → Orders.Id

- **Transactions → SlipVerifications** (Many:1)
  - Many transactions can reference one slip verification
  - Foreign Key: Transactions.SlipVerificationId → SlipVerifications.Id
  - **Note**: This is nullable as some transactions may not require slip verification

- **Transactions → Users** (Many:1)
  - Many transactions belong to one user
  - Foreign Key: Transactions.UserId → Users.Id

### 5. Notifications
Notification records for multi-channel user communications.

**Relationships:**
- **Notifications → Users** (Many:1)
  - Many notifications sent to one user
  - Foreign Key: Notifications.UserId → Users.Id

### 6. AuditLogs
Audit trail for compliance and security.

**Relationships:**
- **AuditLogs → Users** (Many:1)
  - Many audit entries created by one user
  - Foreign Key: AuditLogs.UserId → Users.Id
  - **Note**: UserId is nullable as some system actions may not have a user

## Cardinality Summary

| Relationship | Type | Foreign Key | Delete Rule |
|-------------|------|-------------|-------------|
| Users → Orders | 1:Many | Orders.UserId | RESTRICT |
| Users → SlipVerifications (Uploader) | 1:Many | SlipVerifications.UserId | RESTRICT |
| Users → SlipVerifications (Verifier) | 1:Many | SlipVerifications.VerifiedBy | RESTRICT |
| Users → Transactions | 1:Many | Transactions.UserId | RESTRICT |
| Users → Notifications | 1:Many | Notifications.UserId | CASCADE |
| Users → AuditLogs | 1:Many | AuditLogs.UserId | SET NULL |
| Orders → SlipVerifications | 1:Many | SlipVerifications.OrderId | CASCADE |
| Orders → Transactions | 1:Many | Transactions.OrderId | RESTRICT |
| SlipVerifications → Transactions | 1:Many | Transactions.SlipVerificationId | RESTRICT |

## Database Normalization

### Normal Form: 3NF (Third Normal Form)
The database design follows 3NF principles:

1. **1NF**: All tables have primary keys, and all columns contain atomic values
2. **2NF**: All non-key attributes are fully dependent on the primary key
3. **3NF**: No transitive dependencies exist

### Denormalization Considerations
Some strategic denormalization for performance:
- **Metadata (JSONB)**: Stored in Transactions and Notifications for flexibility
- **Soft Delete**: IsDeleted and DeletedAt columns are denormalized across all tables
- **Audit Fields**: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy in all tables

## Referential Integrity

### Cascade Rules Strategy
- **CASCADE**: Used only for dependent data (Notifications, SlipVerifications from Orders)
- **RESTRICT**: Used for financial/critical data (Transactions, Orders from Users)
- **SET NULL**: Used for audit trails (AuditLogs from Users)

### Constraint Checks
- **Amount checks**: Positive values only (CHK_Orders_Amount, CHK_Transactions_Amount)
- **Email validation**: Basic email format check (CHK_Users_Email)
- **Unique constraints**: Email, Username, OrderNumber, ImageHash

## JSON Fields

### 1. Transactions.Metadata (JSONB)
Flexible storage for transaction-specific data:
```json
{
  "bank_code": "004",
  "branch_code": "0001",
  "payment_channel": "mobile_banking",
  "device_id": "ABC123",
  "ip_address": "192.168.1.1"
}
```

### 2. Notifications.Data (JSONB)
Notification-specific data:
```json
{
  "order_id": "ORD-2025-0001",
  "amount": 1500.00,
  "action_url": "https://example.com/orders/123"
}
```

### 3. AuditLogs.OldValues / NewValues (JSONB)
Complete entity state snapshots:
```json
{
  "Id": "...",
  "Status": "Pending",
  "Amount": 1500.00,
  "UpdatedAt": "2025-01-01T10:00:00Z"
}
```

## Indexes and Performance

### Clustered Indexes
- All tables use UUID primary keys (non-sequential)
- Consider adding CreatedAt to primary key for time-series optimization

### Foreign Key Indexes
All foreign keys have corresponding indexes for join performance:
- Orders.UserId
- SlipVerifications.OrderId, UserId, VerifiedBy
- Transactions.OrderId, SlipVerificationId, UserId
- Notifications.UserId
- AuditLogs.UserId

See [INDEX_STRATEGY.md](INDEX_STRATEGY.md) for complete indexing details.

## Soft Delete Pattern

All tables implement soft delete:
- **IsDeleted** (boolean): Soft delete flag
- **DeletedAt** (timestamp): Deletion timestamp
- **Query Filters**: EF Core global query filters exclude soft-deleted records

Benefits:
- Data recovery capability
- Audit trail preservation
- Referential integrity maintenance
- Compliance with data retention policies

## Views

### 1. v_daily_transaction_summary
Aggregates daily transaction statistics:
- Total transactions
- Completed/Failed/Pending counts
- Total and average amounts

### 2. v_user_statistics
Aggregates per-user statistics:
- Total orders and transactions
- Total spent
- Slip upload count
- Last activity dates

## Future Considerations

### Potential Schema Enhancements
1. **User Roles Table**: Separate table for role management
2. **Payment Methods Table**: Normalize payment methods
3. **Bank Accounts Table**: Store user bank accounts
4. **Categories Table**: Order categorization
5. **Settings Table**: System and user preferences

### Partitioning Strategy
For high-volume tables:
- **Transactions**: Partition by month
- **AuditLogs**: Partition by month
- **Notifications**: Partition by status and date

### Archival Strategy
- Move old records to archive tables
- Maintain last 12 months in primary tables
- Archive older data for compliance

## Conclusion

This ERD design provides:
- Clear relationship structure
- Data integrity through constraints
- Flexibility through JSON fields
- Scalability through proper indexing
- Audit trail for compliance
- Soft delete for data recovery

The design balances normalization with practical performance considerations for a payment verification system handling 1,000+ transactions per day.
