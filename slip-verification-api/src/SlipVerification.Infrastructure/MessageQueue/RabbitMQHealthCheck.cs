using Microsoft.Extensions.Diagnostics.HealthChecks;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Health check for RabbitMQ connection
/// </summary>
public class RabbitMQHealthCheck : IHealthCheck
{
    private readonly IRabbitMQConnectionFactory _connectionFactory;

    public RabbitMQHealthCheck(IRabbitMQConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = _connectionFactory.CreateConnection();
            
            if (connection.IsOpen)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("RabbitMQ connection is healthy"));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("RabbitMQ connection is not open"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "RabbitMQ connection failed",
                    exception: ex));
        }
    }
}
