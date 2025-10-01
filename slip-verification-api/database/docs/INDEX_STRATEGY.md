# Database Index Strategy

## Overview
This document outlines the indexing strategy for the Slip Verification System database, designed to optimize query performance for 1,000+ transactions per day.

## Index Types Used

### 1. **B-Tree Indexes** (Default)
Used for equality and range queries on scalar values.

### 2. **GIN (Generalized Inverted Index)**
Used for JSONB columns to enable efficient queries on JSON data.

### 3. **Partial Indexes**
Indexes with WHERE clauses to reduce index size and improve performance for specific query patterns.

### 4. **Unique Indexes**
Ensure data integrity while providing query optimization.

## Table-by-Table Index Analysis

### Users Table

#### Primary Index
- **Primary Key**: `Id` (UUID, B-Tree)
  - Purpose: Fast lookups by user ID
  - Used in: Foreign key relationships, authentication

#### Secondary Indexes
1. **IX_Users_Email**
   - Column: `Email`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Fast user lookup by email during login
   - Queries: Authentication, user search
   - Estimated Impact: ~95% of login queries

2. **IX_Users_Username**
   - Column: `Username`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Fast user lookup by username
   - Queries: Authentication, profile retrieval
   - Estimated Impact: ~90% of profile queries

3. **IX_Users_Role**
   - Column: `Role`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Filter users by role for admin interfaces
   - Queries: User management, role-based queries
   - Estimated Impact: Admin dashboard queries

4. **IX_Users_IsDeleted**
   - Column: `IsDeleted`
   - Type: B-Tree
   - Purpose: Soft delete filtering
   - Queries: All active user queries
   - Estimated Impact: Global query filter support

### Orders Table

#### Primary Index
- **Primary Key**: `Id` (UUID, B-Tree)

#### Secondary Indexes
1. **IX_Orders_UserId**
   - Column: `UserId`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Retrieve all orders for a specific user
   - Queries: User order history, dashboard
   - Estimated Impact: ~80% of order retrieval queries

2. **IX_Orders_Status**
   - Column: `Status`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Filter orders by status (Pending, Completed, etc.)
   - Queries: Order management, status-based filtering
   - Estimated Impact: ~70% of admin queries

3. **IX_Orders_CreatedAt**
   - Column: `CreatedAt DESC`
   - Type: Descending B-Tree
   - Purpose: Time-series queries, recent orders first
   - Queries: Order history, reporting
   - Estimated Impact: ~90% of time-based queries

4. **IX_Orders_OrderNumber**
   - Column: `OrderNumber`
   - Type: Partial Unique Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Fast lookup by order number
   - Queries: Order tracking, customer service
   - Estimated Impact: Direct order lookups

5. **IX_Orders_ExpiredAt**
   - Column: `ExpiredAt`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL AND "ExpiredAt" IS NOT NULL)
   - Purpose: Find expired orders for cleanup jobs
   - Queries: Scheduled tasks, order expiration
   - Estimated Impact: Background job optimization

### SlipVerifications Table

#### Primary Index
- **Primary Key**: `Id` (UUID, B-Tree)

#### Secondary Indexes
1. **IX_SlipVerifications_OrderId**
   - Column: `OrderId`
   - Type: B-Tree
   - Purpose: Retrieve all slips for an order
   - Queries: Order verification history
   - Estimated Impact: ~85% of slip queries

2. **IX_SlipVerifications_UserId**
   - Column: `UserId`
   - Type: B-Tree
   - Purpose: Retrieve all slips uploaded by a user
   - Queries: User slip history
   - Estimated Impact: ~75% of user-related queries

3. **IX_SlipVerifications_ReferenceNumber**
   - Column: `ReferenceNumber`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Find slips by transaction reference
   - Queries: Slip validation, duplicate detection
   - Estimated Impact: OCR verification queries

4. **IX_SlipVerifications_Status**
   - Column: `Status`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Filter slips by verification status
   - Queries: Admin verification queue
   - Estimated Impact: ~90% of admin verification queries

5. **IX_SlipVerifications_ImageHash**
   - Column: `ImageHash`
   - Type: Partial Unique Index (WHERE "DeletedAt" IS NULL AND "ImageHash" IS NOT NULL)
   - Purpose: Prevent duplicate slip uploads
   - Queries: Slip upload validation
   - Estimated Impact: Critical for duplicate detection

6. **IX_SlipVerifications_TransactionDate**
   - Column: `TransactionDate`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Time-based slip queries
   - Queries: Reporting, reconciliation
   - Estimated Impact: Financial reporting

### Transactions Table

#### Primary Index
- **Primary Key**: `Id` (UUID, B-Tree)

#### Secondary Indexes
1. **IX_Transactions_OrderId**
   - Column: `OrderId`
   - Type: B-Tree
   - Purpose: Retrieve all transactions for an order
   - Queries: Order payment history
   - Estimated Impact: ~85% of transaction queries

2. **IX_Transactions_UserId**
   - Column: `UserId`
   - Type: B-Tree
   - Purpose: Retrieve user transaction history
   - Queries: User financial history
   - Estimated Impact: ~80% of user queries

3. **IX_Transactions_Status**
   - Column: `Status`
   - Type: B-Tree
   - Purpose: Filter transactions by status
   - Queries: Financial reconciliation
   - Estimated Impact: ~75% of status-based queries

4. **IX_Transactions_CreatedAt**
   - Column: `CreatedAt DESC`
   - Type: Descending B-Tree
   - Purpose: Time-series queries, recent transactions first
   - Queries: Transaction history, reporting
   - Estimated Impact: ~95% of time-based queries

5. **IX_Transactions_Metadata**
   - Column: `Metadata`
   - Type: GIN (Generalized Inverted Index)
   - Purpose: Query JSONB metadata efficiently
   - Queries: Complex metadata searches
   - Estimated Impact: Advanced reporting queries

6. **IX_Transactions_ProcessedAt**
   - Column: `ProcessedAt`
   - Type: Partial Index (WHERE "ProcessedAt" IS NOT NULL)
   - Purpose: Find processed transactions
   - Queries: Settlement reports
   - Estimated Impact: Financial reconciliation

### Notifications Table

#### Primary Index
- **Primary Key**: `Id` (UUID, B-Tree)

#### Secondary Indexes
1. **IX_Notifications_UserId**
   - Column: `UserId`
   - Type: B-Tree
   - Purpose: Retrieve user notifications
   - Queries: User notification center
   - Estimated Impact: ~90% of notification queries

2. **IX_Notifications_Status**
   - Column: `Status`
   - Type: B-Tree
   - Purpose: Filter notifications by delivery status
   - Queries: Notification retry queue
   - Estimated Impact: Background jobs

3. **IX_Notifications_CreatedAt**
   - Column: `CreatedAt DESC`
   - Type: Descending B-Tree
   - Purpose: Recent notifications first
   - Queries: Notification timeline
   - Estimated Impact: ~95% of time-based queries

4. **IX_Notifications_ReadAt**
   - Column: `ReadAt`
   - Type: Partial Index (WHERE "ReadAt" IS NULL)
   - Purpose: Find unread notifications
   - Queries: Unread notification count
   - Estimated Impact: User notification badge

5. **IX_Notifications_Channel**
   - Column: `Channel`
   - Type: Partial Index (WHERE "DeletedAt" IS NULL)
   - Purpose: Filter by delivery channel
   - Queries: Channel-specific queries
   - Estimated Impact: Channel analytics

### AuditLogs Table

#### Primary Index
- **Primary Key**: `Id` (UUID, B-Tree)

#### Secondary Indexes
1. **IX_AuditLogs_UserId**
   - Column: `UserId`
   - Type: B-Tree
   - Purpose: Retrieve audit logs for a user
   - Queries: User activity history
   - Estimated Impact: Audit trail queries

2. **IX_AuditLogs_Entity**
   - Column: `EntityType, EntityId` (Composite)
   - Type: B-Tree
   - Purpose: Retrieve audit logs for a specific entity
   - Queries: Entity change history
   - Estimated Impact: ~90% of audit queries

3. **IX_AuditLogs_CreatedAt**
   - Column: `CreatedAt DESC`
   - Type: Descending B-Tree
   - Purpose: Time-series audit queries
   - Queries: Recent activity, reporting
   - Estimated Impact: ~95% of time-based audit queries

4. **IX_AuditLogs_Action**
   - Column: `Action`
   - Type: B-Tree
   - Purpose: Filter by action type (CREATE, UPDATE, DELETE)
   - Queries: Action-specific audit reports
   - Estimated Impact: Compliance reporting

## Index Maintenance

### Autovacuum Configuration
```sql
-- High-traffic tables
ALTER TABLE "Transactions" SET (autovacuum_vacuum_scale_factor = 0.05);
ALTER TABLE "AuditLogs" SET (autovacuum_vacuum_scale_factor = 0.05);
ALTER TABLE "Notifications" SET (autovacuum_vacuum_scale_factor = 0.1);
```

### Index Monitoring
```sql
-- Check index usage
SELECT 
    schemaname, tablename, indexname, 
    idx_scan as index_scans,
    idx_tup_read as tuples_read,
    idx_tup_fetch as tuples_fetched
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;

-- Find unused indexes
SELECT 
    schemaname, tablename, indexname
FROM pg_stat_user_indexes
WHERE idx_scan = 0
  AND indexname NOT LIKE '%_pkey';

-- Check index size
SELECT 
    tablename, indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as index_size
FROM pg_stat_user_indexes
ORDER BY pg_relation_size(indexrelid) DESC;
```

## Performance Considerations

### Index Size vs. Performance Trade-offs
- **Small tables (< 10,000 rows)**: Indexes may not provide significant benefit
- **Medium tables (10,000 - 1,000,000 rows)**: Indexes essential for performance
- **Large tables (> 1,000,000 rows)**: Consider partitioning + indexes

### Write Performance Impact
- Each additional index slows down INSERT/UPDATE operations
- Balance between read and write performance
- Current strategy: Optimized for read-heavy workload (typical for verification systems)

### Partial Indexes Benefits
- Smaller index size (30-50% reduction)
- Faster index scans
- Reduced maintenance overhead
- Used extensively for soft-delete pattern

## Query Optimization Tips

### Use EXPLAIN ANALYZE
```sql
EXPLAIN ANALYZE 
SELECT * FROM "Orders" 
WHERE "UserId" = '22222222-2222-2222-2222-222222222222' 
  AND "DeletedAt" IS NULL;
```

### Index-Only Scans
Ensure frequently queried columns are part of the index:
```sql
CREATE INDEX idx_example ON "Orders"("UserId", "Status", "CreatedAt");
```

### Avoid Index Bloat
- Regular REINDEX on heavily modified tables
- Monitor index size growth
- Use pg_repack for production systems

## Future Considerations

### Potential Partitioning Strategy (if needed)
- **Transactions**: Partition by month (time-series data)
- **AuditLogs**: Partition by month (append-only data)
- **Notifications**: Partition by status or date

### Additional Indexes for Scale
- Full-text search indexes on description fields
- Covering indexes for common query patterns
- Functional indexes for computed values

## Conclusion
This indexing strategy is designed to support:
- Fast authentication (< 50ms)
- Efficient order queries (< 100ms)
- Real-time transaction processing (< 200ms)
- Quick slip verification lookups (< 150ms)
- Scalable audit trail (< 500ms)

Regular monitoring and adjustment based on actual query patterns is essential for maintaining optimal performance.
