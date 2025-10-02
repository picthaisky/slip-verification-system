using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SlipVerification.Infrastructure.Data;

namespace SlipVerification.IntegrationTests.Helpers;

/// <summary>
/// Custom Web Application Factory for integration tests
/// Configures in-memory database and test services
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            // Add in-memory database for testing
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
            });

            // Remove Redis connection (not needed for basic tests)
            services.RemoveAll(typeof(StackExchange.Redis.IConnectionMultiplexer));

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Create scope and get database context
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Ensure database is created
                db.Database.EnsureCreated();

                // Seed test data if needed
                SeedTestData(db);
            }
        });

        builder.UseEnvironment("Testing");
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        // Add any test data seeding here if needed
        // For now, keep it empty as tests should manage their own data
    }
}
