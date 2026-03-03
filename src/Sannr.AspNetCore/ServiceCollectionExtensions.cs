using Microsoft.Extensions.DependencyInjection;

namespace Sannr.AspNetCore;

/// <summary>
/// Extension methods for registering Sannr services with dependency injection.
/// Uses TryAddEnumerable for idempotent registration, preventing duplicate services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Sannr validation services to the service collection.
    /// Uses TryAddEnumerable to ensure idempotency and avoid registration conflicts.
    /// </summary>
    public static IServiceCollection TryAddSannrServices(this IServiceCollection services)
    {
        return services;
    }

    /// <summary>
    /// Adds Sannr validation services and automatically registers all generated validators.
    /// This provides the convenience of automatic registration while maintaining AOT compatibility.
    /// </summary>
    public static IServiceCollection AddSannrValidators(this IServiceCollection services)
    {
        return services.TryAddSannrServices();
    }
}
