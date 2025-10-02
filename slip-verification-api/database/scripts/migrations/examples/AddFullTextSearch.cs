using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlipVerification.Infrastructure.Migrations.Examples
{
    /// <summary>
    /// Example migration demonstrating PostgreSQL full-text search implementation
    /// This adds full-text search capabilities to the Orders table
    /// </summary>
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =============================================================================
            // 1. Add tsvector column for full-text search
            // =============================================================================
            migrationBuilder.Sql(@"
                ALTER TABLE ""Orders""
                ADD COLUMN search_vector tsvector;
            ");
            
            // =============================================================================
            // 2. Create function to update search vector
            // This function will be triggered on INSERT or UPDATE
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION orders_search_vector_update()
                RETURNS trigger AS $$
                BEGIN
                    -- Build search vector from multiple columns with weights
                    -- 'A' = highest weight, 'B' = medium, 'C' = low, 'D' = lowest
                    NEW.search_vector :=
                        setweight(to_tsvector('english', coalesce(NEW.""OrderNumber"", '')), 'A') ||
                        setweight(to_tsvector('english', coalesce(NEW.""Description"", '')), 'B') ||
                        setweight(to_tsvector('english', coalesce(NEW.""Status"", '')), 'C');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            // =============================================================================
            // 3. Create trigger to automatically update search vector
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE TRIGGER orders_search_vector_trigger
                BEFORE INSERT OR UPDATE ON ""Orders""
                FOR EACH ROW
                EXECUTE FUNCTION orders_search_vector_update();
            ");
            
            // =============================================================================
            // 4. Create GIN index for fast full-text search
            // GIN (Generalized Inverted Index) is optimal for full-text search
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE INDEX idx_orders_search_vector
                ON ""Orders""
                USING GIN(search_vector);
            ");
            
            // =============================================================================
            // 5. Update existing rows with search vectors
            // =============================================================================
            migrationBuilder.Sql(@"
                UPDATE ""Orders""
                SET search_vector = 
                    setweight(to_tsvector('english', coalesce(""OrderNumber"", '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(""Description"", '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(""Status"", '')), 'C')
                WHERE search_vector IS NULL;
            ");
            
            // =============================================================================
            // 6. Create helper view for search results with ranking
            // =============================================================================
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW v_orders_search AS
                SELECT 
                    ""Id"",
                    ""OrderNumber"",
                    ""Description"",
                    ""Status"",
                    ""Amount"",
                    ""CreatedAt"",
                    search_vector
                FROM ""Orders""
                WHERE ""IsDeleted"" = false;
            ");
            
            // =============================================================================
            // Optional: Add the same for SlipVerifications table
            // =============================================================================
            migrationBuilder.Sql(@"
                ALTER TABLE ""SlipVerifications""
                ADD COLUMN search_vector tsvector;
            ");
            
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION slipverifications_search_vector_update()
                RETURNS trigger AS $$
                BEGIN
                    NEW.search_vector :=
                        setweight(to_tsvector('english', coalesce(NEW.""RawOcrText"", '')), 'A') ||
                        setweight(to_tsvector('english', coalesce(NEW.""VerificationNotes"", '')), 'B');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");
            
            migrationBuilder.Sql(@"
                CREATE TRIGGER slipverifications_search_vector_trigger
                BEFORE INSERT OR UPDATE ON ""SlipVerifications""
                FOR EACH ROW
                EXECUTE FUNCTION slipverifications_search_vector_update();
            ");
            
            migrationBuilder.Sql(@"
                CREATE INDEX idx_slipverifications_search_vector
                ON ""SlipVerifications""
                USING GIN(search_vector);
            ");
            
            migrationBuilder.Sql(@"
                UPDATE ""SlipVerifications""
                SET search_vector = 
                    setweight(to_tsvector('english', coalesce(""RawOcrText"", '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(""VerificationNotes"", '')), 'B')
                WHERE search_vector IS NULL;
            ");
        }
        
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // =============================================================================
            // Remove full-text search from SlipVerifications
            // =============================================================================
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS slipverifications_search_vector_trigger ON ""SlipVerifications"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS slipverifications_search_vector_update();");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_slipverifications_search_vector;");
            migrationBuilder.Sql(@"ALTER TABLE ""SlipVerifications"" DROP COLUMN IF EXISTS search_vector;");
            
            // =============================================================================
            // Remove full-text search from Orders
            // =============================================================================
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS v_orders_search;");
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS orders_search_vector_trigger ON ""Orders"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS orders_search_vector_update();");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_orders_search_vector;");
            migrationBuilder.Sql(@"ALTER TABLE ""Orders"" DROP COLUMN IF EXISTS search_vector;");
        }
    }
}

/* 
 * USAGE EXAMPLES:
 * 
 * 1. Simple search:
 * SELECT * FROM "Orders"
 * WHERE search_vector @@ to_tsquery('english', 'payment');
 * 
 * 2. Search with ranking:
 * SELECT 
 *     "OrderNumber",
 *     "Description",
 *     ts_rank(search_vector, query) AS rank
 * FROM "Orders", to_tsquery('english', 'payment & slip') query
 * WHERE search_vector @@ query
 * ORDER BY rank DESC;
 * 
 * 3. Phrase search:
 * SELECT * FROM "Orders"
 * WHERE search_vector @@ phraseto_tsquery('english', 'payment completed');
 * 
 * 4. Search with highlighting:
 * SELECT 
 *     "OrderNumber",
 *     ts_headline('english', "Description", query) AS highlighted_description
 * FROM "Orders", to_tsquery('english', 'payment') query
 * WHERE search_vector @@ query;
 * 
 * 5. Complex search with multiple conditions:
 * SELECT * FROM "Orders"
 * WHERE search_vector @@ to_tsquery('english', 'payment | transfer')
 *   AND "Status" = 'Pending'
 *   AND "CreatedAt" >= NOW() - INTERVAL '7 days'
 * ORDER BY ts_rank(search_vector, to_tsquery('english', 'payment | transfer')) DESC;
 */
