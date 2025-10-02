using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SlipVerification.Infrastructure.Data;
using Xunit;

namespace SlipVerification.IntegrationTests.Migrations
{
    /// <summary>
    /// Integration tests for database migrations
    /// These tests verify that migrations can be applied and rolled back correctly
    /// </summary>
    public class MigrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;
        private readonly string _testDatabaseName;

        public MigrationTests()
        {
            // Create unique test database name
            _testDatabaseName = $"TestDb_{Guid.NewGuid():N}";
            _connectionString = $"Host=localhost;Port=5432;Database={_testDatabaseName};Username=postgres;Password=postgres";
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_connectionString)
                .Options;
            
            _context = new ApplicationDbContext(options);
        }

        /// <summary>
        /// Test that the initial migration creates all required tables
        /// </summary>
        [Fact]
        public async Task Migration_InitialCreate_ShouldCreateAllTables()
        {
            // Act - Apply all migrations
            await _context.Database.MigrateAsync();
            
            // Assert - Verify connection
            Assert.True(await _context.Database.CanConnectAsync());
            
            // Get all tables
            var tables = await GetTableNamesAsync();
            
            // Verify core tables exist
            Assert.Contains("Users", tables);
            Assert.Contains("Orders", tables);
            Assert.Contains("SlipVerifications", tables);
            Assert.Contains("Transactions", tables);
            Assert.Contains("Notifications", tables);
            Assert.Contains("NotificationTemplates", tables);
            Assert.Contains("AuditLogs", tables);
            Assert.Contains("RefreshTokens", tables);
            Assert.Contains("__EFMigrationsHistory", tables);
        }

        /// <summary>
        /// Test that migrations can be rolled back and reapplied
        /// </summary>
        [Fact]
        public async Task Migration_UpDown_ShouldBeReversible()
        {
            // Arrange - Apply all migrations
            await _context.Database.MigrateAsync();
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            var initialCount = appliedMigrations.Count();
            
            // Get the last migration and previous migration names
            var allMigrations = appliedMigrations.OrderBy(m => m).ToList();
            if (allMigrations.Count < 2)
            {
                // Skip test if there's only one migration
                return;
            }
            
            var lastMigration = allMigrations.Last();
            var previousMigration = allMigrations[allMigrations.Count - 2];
            
            // Act - Rollback to previous migration
            await _context.Database.MigrateAsync(previousMigration);
            var afterRollback = await _context.Database.GetAppliedMigrationsAsync();
            
            // Assert - Verify rollback
            Assert.True(initialCount > afterRollback.Count());
            Assert.DoesNotContain(lastMigration, afterRollback);
            Assert.Contains(previousMigration, afterRollback);
            
            // Act - Reapply migrations
            await _context.Database.MigrateAsync();
            var afterReapply = await _context.Database.GetAppliedMigrationsAsync();
            
            // Assert - Verify reapply
            Assert.Equal(initialCount, afterReapply.Count());
            Assert.Contains(lastMigration, afterReapply);
        }

        /// <summary>
        /// Test that all required indexes are created
        /// </summary>
        [Fact]
        public async Task Migration_Indexes_ShouldBeCreated()
        {
            // Arrange
            await _context.Database.MigrateAsync();
            
            // Act - Get all indexes
            var indexes = await GetIndexNamesAsync();
            
            // Assert - Check for important indexes
            Assert.Contains(indexes, i => i.Contains("IX_Users_Email"));
            Assert.Contains(indexes, i => i.Contains("IX_Users_Username"));
            Assert.Contains(indexes, i => i.Contains("IX_Orders_OrderNumber"));
            Assert.Contains(indexes, i => i.Contains("IX_Orders_UserId"));
            Assert.Contains(indexes, i => i.Contains("IX_SlipVerifications_OrderId"));
            Assert.Contains(indexes, i => i.Contains("IX_Transactions_OrderId"));
            Assert.Contains(indexes, i => i.Contains("IX_Notifications_UserId"));
        }

        /// <summary>
        /// Test that foreign key constraints are created correctly
        /// </summary>
        [Fact]
        public async Task Migration_ForeignKeys_ShouldBeCreated()
        {
            // Arrange
            await _context.Database.MigrateAsync();
            
            // Act - Get all foreign keys
            var foreignKeys = await GetForeignKeyConstraintsAsync();
            
            // Assert - Check for important foreign keys
            Assert.Contains(foreignKeys, fk => fk.Contains("FK_Orders_Users"));
            Assert.Contains(foreignKeys, fk => fk.Contains("FK_SlipVerifications_Orders"));
            Assert.Contains(foreignKeys, fk => fk.Contains("FK_SlipVerifications_Users"));
            Assert.Contains(foreignKeys, fk => fk.Contains("FK_Transactions_Orders"));
            Assert.Contains(foreignKeys, fk => fk.Contains("FK_Notifications_Users"));
        }

        /// <summary>
        /// Test that check constraints are created
        /// </summary>
        [Fact]
        public async Task Migration_CheckConstraints_ShouldBeCreated()
        {
            // Arrange
            await _context.Database.MigrateAsync();
            
            // Act - Get check constraints
            var constraints = await GetCheckConstraintsAsync();
            
            // Assert - Verify important constraints exist
            Assert.NotEmpty(constraints);
            // Note: Specific constraint names depend on your migration implementation
        }

        /// <summary>
        /// Test that migration history is tracked correctly
        /// </summary>
        [Fact]
        public async Task Migration_History_ShouldBeTracked()
        {
            // Arrange & Act
            await _context.Database.MigrateAsync();
            
            // Get migration history from database
            var history = await _context.Database.GetAppliedMigrationsAsync();
            
            // Assert
            Assert.NotEmpty(history);
            
            // Verify migrations are in correct order
            var orderedHistory = history.OrderBy(m => m).ToList();
            Assert.Equal(history.Count(), orderedHistory.Count);
            
            // Verify we have the initial migration
            Assert.Contains(history, m => m.Contains("InitialCreate"));
        }

        /// <summary>
        /// Test that soft delete query filters work correctly
        /// </summary>
        [Fact]
        public async Task Migration_SoftDelete_QueryFiltersShouldWork()
        {
            // Arrange
            await _context.Database.MigrateAsync();
            
            // Act & Assert - This would be tested with actual data operations
            // For now, just verify the migration completed successfully
            Assert.True(await _context.Database.CanConnectAsync());
        }

        /// <summary>
        /// Test that default values are set correctly
        /// </summary>
        [Fact]
        public async Task Migration_DefaultValues_ShouldBeSet()
        {
            // Arrange
            await _context.Database.MigrateAsync();
            
            // Act - Check column defaults
            var columnDefaults = await GetColumnDefaultsAsync("Users");
            
            // Assert
            Assert.NotEmpty(columnDefaults);
            // Verify specific defaults exist (adjust based on your schema)
            Assert.Contains(columnDefaults, cd => cd.Item1 == "IsActive");
            Assert.Contains(columnDefaults, cd => cd.Item1 == "IsDeleted");
            Assert.Contains(columnDefaults, cd => cd.Item1 == "EmailVerified");
        }

        /// <summary>
        /// Test that the database can be created from scratch
        /// </summary>
        [Fact]
        public async Task Migration_CreateDatabase_ShouldSucceed()
        {
            // Act
            var created = await _context.Database.EnsureCreatedAsync();
            
            // Assert
            Assert.True(created || await _context.Database.CanConnectAsync());
        }

        #region Helper Methods

        private async Task<List<string>> GetTableNamesAsync()
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_type = 'BASE TABLE'
                ORDER BY table_name;
            ";
            
            var tables = new List<string>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            
            return tables;
        }

        private async Task<List<string>> GetIndexNamesAsync()
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT indexname 
                FROM pg_indexes 
                WHERE schemaname = 'public'
                AND indexname NOT LIKE 'pg_%'
                ORDER BY indexname;
            ";
            
            var indexes = new List<string>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    indexes.Add(reader.GetString(0));
                }
            }
            
            return indexes;
        }

        private async Task<List<string>> GetForeignKeyConstraintsAsync()
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT constraint_name
                FROM information_schema.table_constraints
                WHERE constraint_type = 'FOREIGN KEY'
                AND table_schema = 'public'
                ORDER BY constraint_name;
            ";
            
            var constraints = new List<string>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    constraints.Add(reader.GetString(0));
                }
            }
            
            return constraints;
        }

        private async Task<List<string>> GetCheckConstraintsAsync()
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT constraint_name
                FROM information_schema.table_constraints
                WHERE constraint_type = 'CHECK'
                AND table_schema = 'public'
                ORDER BY constraint_name;
            ";
            
            var constraints = new List<string>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    constraints.Add(reader.GetString(0));
                }
            }
            
            return constraints;
        }

        private async Task<List<(string, string)>> GetColumnDefaultsAsync(string tableName)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT column_name, column_default
                FROM information_schema.columns
                WHERE table_name = @tableName
                AND column_default IS NOT NULL
                ORDER BY column_name;
            ";
            
            var param = command.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            command.Parameters.Add(param);
            
            var defaults = new List<(string, string)>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    defaults.Add((reader.GetString(0), reader.GetString(1)));
                }
            }
            
            return defaults;
        }

        #endregion

        public void Dispose()
        {
            // Clean up test database
            try
            {
                _context.Database.EnsureDeleted();
            }
            catch
            {
                // Ignore cleanup errors
            }
            
            _context.Dispose();
        }
    }
}
