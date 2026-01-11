using Microsoft.Extensions.DependencyInjection;

namespace Sannr.AspNetCore;

/// <summary>
/// Extension methods for registering Sannr services with dependency injection.
/// Uses TryAddEnumerable for idempotent registration, preventing duplicate services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Auto-generated validator registrations.
    /// This method is implemented by the source generator.
    /// </summary>
    static partial void RegisterGeneratedValidators(IServiceCollection services);

    /// <summary>
    /// Adds Sannr validation services to the service collection.
    /// Uses TryAddEnumerable to ensure idempotency and avoid registration conflicts.
    /// </summary>
    public static IServiceCollection TryAddSannrServices(this IServiceCollection services)
    {
        // Example: Register validation services idempotently
        // services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidationService, DefaultValidationService>()); // Idempotent DI registration
        // Ensure DummyForOpenApi is compiled
        _ = typeof(DummyForOpenApi);
        return services;
    }

    /// <summary>
    /// Adds Sannr validation services and automatically registers all generated validators.
    /// This provides the convenience of automatic registration while maintaining AOT compatibility.
    /// </summary>
    public static IServiceCollection AddSannrValidators(this IServiceCollection services)
    {
        services.TryAddSannrServices();
        // Register all generated validators
        RegisterGeneratedValidators(services);
        return services;
    }
}
