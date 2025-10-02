namespace SlipVerification.API.Services;

/// <summary>
/// Interface for application metrics tracking
/// </summary>
public interface IMetrics
{
    /// <summary>
    /// Increment slip verification counter
    /// </summary>
    void IncrementSlipVerification(string status, string bank);

    /// <summary>
    /// Measure slip processing duration
    /// </summary>
    IDisposable MeasureSlipProcessing();

    /// <summary>
    /// Record error occurrence
    /// </summary>
    void RecordError(string type, string endpoint);

    /// <summary>
    /// Record request duration
    /// </summary>
    void RecordRequestDuration(string method, string path, int statusCode, double durationSeconds);

    /// <summary>
    /// Set active WebSocket connections gauge
    /// </summary>
    void SetActiveConnections(int count);

    /// <summary>
    /// Increment active connections
    /// </summary>
    void IncrementActiveConnections();

    /// <summary>
    /// Decrement active connections
    /// </summary>
    void DecrementActiveConnections();
}
