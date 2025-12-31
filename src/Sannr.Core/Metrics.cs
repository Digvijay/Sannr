using System.Diagnostics.Metrics;

namespace Sannr.Core;

/// <summary>
/// Provides metrics for observability in Sannr validation operations.
/// Uses System.Diagnostics.Metrics for native OpenTelemetry integration.
/// </summary>
public static class Metrics
{
    /// <summary>
    /// The meter for Sannr metrics, enabling distributed tracing and monitoring.
    /// </summary>
    public static readonly Meter Meter = new("Sannr.Core", "1.0.0");

    /// <summary>
    /// Counter for tracking validation events, crucial for performance monitoring.
    /// </summary>
    public static readonly Counter<long> ValidationEvents = Meter.CreateCounter<long>(
        "validation_events",
        "events",
        "Tracks validation events for observability and performance analysis");

    /// <summary>
    /// Counter for serialization events, integrated with source generator for seamless metrics.
    /// </summary>
    public static readonly Counter<long> SerializationEvents = Meter.CreateCounter<long>(
        "serialization_events",
        "events",
        "Tracks serialization events for observability");
}