using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations.Examples
{
    /// <summary>
    /// Example migration demonstrating audit trail implementation with triggers
    /// This creates automatic audit logging for key tables
    /// </summary>
    /// <inheritdoc />
    public partial class AddAuditTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================================================
            // 1. Create audit function that logs all changes
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION audit_log_changes()
                RETURNS TRIGGER AS $$
                DECLARE
                    audit_id uuid;
                    user_id_val uuid;
                BEGIN
                    -- Generate new audit ID
                    audit_id := gen_random_uuid();
                    
                    -- Try to get user ID from current context (if available)
                    -- This requires application to set session variable
                    BEGIN
                        user_id_val := current_setting('app.current_user_id', true)::uuid;
                    EXCEPTION
                        WHEN OTHERS THEN
                            user_id_val := NULL;
                    END;
                    
                    -- Handle INSERT
                    IF (TG_OP = 'INSERT') THEN
                        INSERT INTO ""AuditLogs"" (
                            ""Id"",
                            ""UserId"",
                            ""EntityType"",
                            ""EntityId"",
                            ""Action"",
                            ""NewValues"",
                            ""IpAddress"",
                            ""CreatedAt""
                        )
                        VALUES (
                            audit_id,
                            user_id_val,
                            TG_TABLE_NAME,
                            NEW.""Id"",
                            'INSERT',
                            row_to_json(NEW)::jsonb,
                            current_setting('app.client_ip', true),
                            NOW()
                        );
                        RETURN NEW;
                        
                    -- Handle UPDATE
                    ELSIF (TG_OP = 'UPDATE') THEN
                        -- Only log if there are actual changes
                        IF row_to_json(OLD)::jsonb != row_to_json(NEW)::jsonb THEN
                            INSERT INTO ""AuditLogs"" (
                                ""Id"",
                                ""UserId"",
                                ""EntityType"",
                                ""EntityId"",
                                ""Action"",
                                ""OldValues"",
                                ""NewValues"",
                                ""IpAddress"",
                                ""CreatedAt""
                            )
                            VALUES (
                                audit_id,
                                user_id_val,
                                TG_TABLE_NAME,
                                NEW.""Id"",
                                'UPDATE',
                                row_to_json(OLD)::jsonb,
                                row_to_json(NEW)::jsonb,
                                current_setting('app.client_ip', true),
                                NOW()
                            );
                        END IF;
                        RETURN NEW;
                        
                    -- Handle DELETE (soft delete check)
                    ELSIF (TG_OP = 'DELETE') THEN
                        INSERT INTO ""AuditLogs"" (
                            ""Id"",
                            ""UserId"",
                            ""EntityType"",
                            ""EntityId"",
                            ""Action"",
                            ""OldValues"",
                            ""IpAddress"",
                            ""CreatedAt""
                        )
                        VALUES (
                            audit_id,
                            user_id_val,
                            TG_TABLE_NAME,
                            OLD.""Id"",
                            'DELETE',
                            row_to_json(OLD)::jsonb,
                            current_setting('app.client_ip', true),
                            NOW()
                        );
                        RETURN OLD;
                    END IF;
                    
                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            // =============================================================================
            // 2. Create triggers for critical tables
            // =============================================================================
            
            // Audit Orders table
            migrationBuilder.Sql(@"
                CREATE TRIGGER audit_orders_trigger
                AFTER INSERT OR UPDATE OR DELETE ON ""Orders""
                FOR EACH ROW EXECUTE FUNCTION audit_log_changes();
            ");
            
            // Audit SlipVerifications table
            migrationBuilder.Sql(@"
                CREATE TRIGGER audit_slipverifications_trigger
                AFTER INSERT OR UPDATE OR DELETE ON ""SlipVerifications""
                FOR EACH ROW EXECUTE FUNCTION audit_log_changes();
            ");
            
            // Audit Transactions table
            migrationBuilder.Sql(@"
                CREATE TRIGGER audit_transactions_trigger
                AFTER INSERT OR UPDATE OR DELETE ON ""Transactions""
                FOR EACH ROW EXECUTE FUNCTION audit_log_changes();
            ");
            
            // Audit Users table (sensitive data)
            migrationBuilder.Sql(@"
                CREATE TRIGGER audit_users_trigger
                AFTER INSERT OR UPDATE OR DELETE ON ""Users""
                FOR EACH ROW EXECUTE FUNCTION audit_log_changes();
            ");
            
            // =============================================================================
            // 3. Create helper function to get audit history for an entity
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION get_entity_audit_history(
                    p_entity_type VARCHAR,
                    p_entity_id UUID
                )
                RETURNS TABLE (
                    audit_id UUID,
                    action VARCHAR,
                    old_values JSONB,
                    new_values JSONB,
                    changed_by UUID,
                    changed_at TIMESTAMP WITH TIME ZONE
                ) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT 
                        ""Id"" as audit_id,
                        ""Action"" as action,
                        ""OldValues"" as old_values,
                        ""NewValues"" as new_values,
                        ""UserId"" as changed_by,
                        ""CreatedAt"" as changed_at
                    FROM ""AuditLogs""
                    WHERE ""EntityType"" = p_entity_type
                        AND ""EntityId"" = p_entity_id
                    ORDER BY ""CreatedAt"" DESC;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            // =============================================================================
            // 4. Create helper function to get changes between two versions
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION get_field_changes(
                    old_data JSONB,
                    new_data JSONB
                )
                RETURNS TABLE (
                    field_name TEXT,
                    old_value TEXT,
                    new_value TEXT
                ) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT 
                        key as field_name,
                        old_data->key as old_value,
                        new_data->key as new_value
                    FROM jsonb_object_keys(new_data) key
                    WHERE old_data->key IS DISTINCT FROM new_data->key;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            // =============================================================================
            // 5. Create view for easy audit log querying
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW v_audit_log_summary AS
                SELECT 
                    al.""Id"",
                    al.""EntityType"",
                    al.""EntityId"",
                    al.""Action"",
                    al.""CreatedAt"",
                    u.""Username"" as ""ChangedByUsername"",
                    u.""Email"" as ""ChangedByEmail"",
                    al.""IpAddress""
                FROM ""AuditLogs"" al
                LEFT JOIN ""Users"" u ON al.""UserId"" = u.""Id""
                ORDER BY al.""CreatedAt"" DESC;
            ");
            
            // =============================================================================
            // 6. Create materialized view for audit statistics
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW mv_audit_statistics AS
                SELECT 
                    ""EntityType"",
                    ""Action"",
                    COUNT(*) as ""Count"",
                    DATE_TRUNC('day', ""CreatedAt"") as ""Date""
                FROM ""AuditLogs""
                WHERE ""CreatedAt"" >= NOW() - INTERVAL '90 days'
                GROUP BY ""EntityType"", ""Action"", DATE_TRUNC('day', ""CreatedAt"")
                ORDER BY DATE_TRUNC('day', ""CreatedAt"") DESC;
                
                CREATE UNIQUE INDEX idx_mv_audit_statistics 
                ON mv_audit_statistics (""EntityType"", ""Action"", ""Date"");
            ");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =============================================================================
            // Drop materialized view and its index
            // =============================================================================
            migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS mv_audit_statistics;");
            
            // =============================================================================
            // Drop views
            // =============================================================================
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS v_audit_log_summary;");
            
            // =============================================================================
            // Drop helper functions
            // =============================================================================
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS get_field_changes(JSONB, JSONB);");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS get_entity_audit_history(VARCHAR, UUID);");
            
            // =============================================================================
            // Drop triggers from all tables
            // =============================================================================
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS audit_users_trigger ON ""Users"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS audit_transactions_trigger ON ""Transactions"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS audit_slipverifications_trigger ON ""SlipVerifications"";");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS audit_orders_trigger ON ""Orders"";");
            
            // =============================================================================
            // Drop audit function
            // =============================================================================
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS audit_log_changes();");
        }
    }
}

/*
 * USAGE EXAMPLES IN APPLICATION CODE:
 * 
 * 1. Set user context before making changes:
 * 
 * public async Task<Order> CreateOrderAsync(CreateOrderDto dto, Guid userId)
 * {
 *     // Set user context for audit logging
 *     await _context.Database.ExecuteSqlRawAsync(
 *         "SELECT set_config('app.current_user_id', {0}, false);",
 *         userId.ToString()
 *     );
 *     
 *     // Set IP address context
 *     await _context.Database.ExecuteSqlRawAsync(
 *         "SELECT set_config('app.client_ip', {0}, false);",
 *         _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown"
 *     );
 *     
 *     var order = new Order { ... };
 *     _context.Orders.Add(order);
 *     await _context.SaveChangesAsync();
 *     
 *     return order;
 * }
 * 
 * 2. Query audit history for an entity:
 * 
 * SELECT * FROM get_entity_audit_history('Orders', '123e4567-e89b-12d3-a456-426614174000');
 * 
 * 3. Get field-level changes:
 * 
 * SELECT 
 *     al."Action",
 *     al."CreatedAt",
 *     fc.field_name,
 *     fc.old_value,
 *     fc.new_value
 * FROM "AuditLogs" al
 * CROSS JOIN LATERAL get_field_changes(al."OldValues", al."NewValues") fc
 * WHERE al."EntityType" = 'Orders' 
 *   AND al."EntityId" = '123e4567-e89b-12d3-a456-426614174000'
 * ORDER BY al."CreatedAt" DESC;
 * 
 * 4. Query audit log summary view:
 * 
 * SELECT * FROM v_audit_log_summary
 * WHERE "EntityType" = 'Orders'
 *   AND "CreatedAt" >= NOW() - INTERVAL '7 days'
 * ORDER BY "CreatedAt" DESC;
 * 
 * 5. Refresh audit statistics (run daily):
 * 
 * REFRESH MATERIALIZED VIEW CONCURRENTLY mv_audit_statistics;
 */
