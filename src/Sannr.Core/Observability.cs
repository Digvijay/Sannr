using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Sannr.Core;

/// <summary>
/// Provides metrics and tracing for observability in Sannr validation operations.
/// Uses System.Diagnostics.Metrics and System.Diagnostics.ActivitySource for native OpenTelemetry integration.
/// </summary>
public static class Observability
{
    private const string ServiceName = "Sannr";
    private const string ServiceVersion = "1.0.0";

    /// <summary>
    /// The meter for Sannr metrics, enabling distributed tracing and monitoring.
    /// </summary>
    public static readonly Meter Meter = new(ServiceName, ServiceVersion);

    /// <summary>
    /// The ActivitySource for Sannr tracing.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    /// <summary>
    /// Counter for tracking validation events, crucial for performance monitoring.
    /// </summary>
    public static readonly Counter<long> ValidationEvents = Meter.CreateCounter<long>(
        "sannr.validation.events",
        "events",
        "Tracks validation events for observability and performance analysis");

    /// <summary>
    /// Counter for serialization events.
    /// </summary>
    public static readonly Counter<long> SerializationEvents = Meter.CreateCounter<long>(
        "sannr.serialization.events",
        "events",
        "Tracks serialization events for observability");
}

/// <summary>
/// Legacy compatibility class for Metrics.
/// </summary>
[Obsolete("Use Observability instead")]
public static class Metrics
{
    public static Meter Meter => Observability.Meter;
    public static Counter<long> ValidationEvents => Observability.ValidationEvents;
    public static Counter<long> SerializationEvents => Observability.SerializationEvents;
}
