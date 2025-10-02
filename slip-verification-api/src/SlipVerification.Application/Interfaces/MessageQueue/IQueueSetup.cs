namespace SlipVerification.Application.Interfaces.MessageQueue;

/// <summary>
/// Interface for setting up RabbitMQ queues, exchanges, and bindings
/// </summary>
public interface IQueueSetup
{
    /// <summary>
    /// Declares all queues, exchanges, and bindings
    /// </summary>
    void DeclareQueues();
}
