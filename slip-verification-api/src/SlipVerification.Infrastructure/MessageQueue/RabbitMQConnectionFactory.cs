using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SlipVerification.Application.Interfaces.MessageQueue;

namespace SlipVerification.Infrastructure.MessageQueue;

/// <summary>
/// Factory for creating and managing RabbitMQ connections
/// </summary>
public class RabbitMQConnectionFactory : IRabbitMQConnectionFactory, IDisposable
{
    private readonly RabbitMQConfiguration _config;
    private readonly ILogger<RabbitMQConnectionFactory> _logger;
    private IConnection? _connection;
    private readonly object _lock = new();

    public RabbitMQConnectionFactory(
        IOptions<RabbitMQConfiguration> config,
        ILogger<RabbitMQConnectionFactory> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public IConnection CreateConnection()
    {
        lock (_lock)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _logger.LogInformation("Creating RabbitMQ connection to {HostName}:{Port}", 
                    _config.HostName, _config.Port);

                var factory = new ConnectionFactory
                {
                    HostName = _config.HostName,
                    Port = _config.Port,
                    UserName = _config.Username,
                    Password = _config.Password,
                    VirtualHost = _config.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60)
                };

                _connection = factory.CreateConnection();
                
                _connection.ConnectionShutdown += (sender, args) =>
                {
                    _logger.LogWarning("RabbitMQ connection shutdown: {Reason}", args.ReplyText);
                };

                _logger.LogInformation("RabbitMQ connection established successfully");
            }

            return _connection;
        }
    }

    public void Dispose()
    {
        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
            _connection.Dispose();
            _logger.LogInformation("RabbitMQ connection closed");
        }
    }
}
