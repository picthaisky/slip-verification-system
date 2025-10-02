using Prometheus;

namespace SlipVerification.API.Services;

/// <summary>
/// Prometheus metrics service for application monitoring
/// </summary>
public class MetricsService : IMetrics
{
    private readonly Counter _slipVerificationCounter;
    private readonly Histogram _slipProcessingDuration;
    private readonly Gauge _activeConnections;
    private readonly Counter _errorCounter;
    private readonly Histogram _requestDuration;

    public MetricsService()
    {
        _slipVerificationCounter = Prometheus.Metrics.CreateCounter(
            "slip_verifications_total",
            "Total number of slip verifications",
            new CounterConfiguration
            {
                LabelNames = new[] { "status", "bank" }
            }
        );

        _slipProcessingDuration = Prometheus.Metrics.CreateHistogram(
            "slip_processing_duration_seconds",
            "Histogram of slip processing duration",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.1, 2, 10)
            }
        );

        _activeConnections = Prometheus.Metrics.CreateGauge(
            "active_websocket_connections",
            "Number of active WebSocket connections"
        );

        _errorCounter = Prometheus.Metrics.CreateCounter(
            "errors_total",
            "Total number of errors",
            new CounterConfiguration
            {
                LabelNames = new[] { "type", "endpoint" }
            }
        );

        _requestDuration = Prometheus.Metrics.CreateHistogram(
            "http_request_duration_seconds",
            "HTTP request duration in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "path", "status_code" },
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 15) // 1ms to ~32s
            }
        );
    }

    public void IncrementSlipVerification(string status, string bank)
    {
        _slipVerificationCounter.WithLabels(status, bank).Inc();
    }

    public IDisposable MeasureSlipProcessing()
    {
        return _slipProcessingDuration.NewTimer();
    }

    public void RecordError(string type, string endpoint)
    {
        _errorCounter.WithLabels(type, endpoint).Inc();
    }

    public void RecordRequestDuration(string method, string path, int statusCode, double durationSeconds)
    {
        _requestDuration
            .WithLabels(method, path, statusCode.ToString())
            .Observe(durationSeconds);
    }

    public void SetActiveConnections(int count)
    {
        _activeConnections.Set(count);
    }

    public void IncrementActiveConnections()
    {
        _activeConnections.Inc();
    }

    public void DecrementActiveConnections()
    {
        _activeConnections.Dec();
    }
}
