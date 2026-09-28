using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Sannr.Core;

namespace Sannr.AspNetCore;

/// <summary>
/// An endpoint filter that automatically validates Sannr-attributed models in Minimal APIs.
/// </summary>
/// <remarks>
/// <para>
/// This filter resolves validators from <see cref="global::Sannr.SannrValidatorRegistry"/>, which
/// is the registry the source generator writes to.
/// </para>
/// <para>
/// Before 1.7.0 it resolved them from the unrelated, always-empty
/// <c>Sannr.AspNetCore.SannrValidatorRegistry</c>. Because that type sits in this namespace it
/// shadowed the real registry at the unqualified call site, so every lookup missed, every request
/// skipped validation, and the endpoint returned success. The registry reference below is
/// deliberately fully qualified so that shadowing cannot recur.
/// </para>
/// </remarks>
public class SannrValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var options = context.HttpContext.RequestServices.GetService<SannrValidationOptions>() ?? new SannrValidationOptions();

        for (int i = 0; i < context.Arguments.Count; i++)
        {
            var argument = context.Arguments[i];
            if (argument == null) continue;

            var modelType = argument.GetType();

            // Fully qualified on purpose. See the remarks on this type.
            if (!global::Sannr.SannrValidatorRegistry.TryGetValidator(modelType, out var validator) || validator == null)
            {
                continue;
            }

            var stopwatch = Stopwatch.StartNew();
            using var activity = Observability.ActivitySource.StartActivity($"Validation: {modelType.Name}");
            activity?.SetTag("sannr.model.type", modelType.FullName);
            activity?.SetTag("sannr.validation.group", null);

            var sannrContext = new SannrValidationContext(
                instance: argument,
                serviceProvider: context.HttpContext.RequestServices,
                items: context.HttpContext.Items,
                group: null
            );

            var result = await validator(sannrContext);
            stopwatch.Stop();

            activity?.SetTag("sannr.validation.is_valid", result.IsValid);
            activity?.SetTag("sannr.validation.duration_ms", stopwatch.Elapsed.TotalMilliseconds);

            if (!result.IsValid)
            {
                activity?.SetTag("sannr.validation.error_count", result.Errors.Count);
                activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");

                if (options.EnableEnhancedErrorResponses)
                {
                    var correlationId = context.HttpContext.Request.Headers["X-Correlation-ID"].ToString();
                    if (string.IsNullOrEmpty(correlationId))
                    {
                        correlationId = Guid.NewGuid().ToString();
                    }

                    var problemDetails = result.Errors.ToSannrValidationProblemDetails(
                        modelType: modelType.Name,
                        correlationId: correlationId,
                        validationDurationMs: options.IncludeValidationDuration ? stopwatch.Elapsed.TotalMilliseconds : null
                    );

                    return Results.Problem(problemDetails);
                }

                var errors = result.Errors
                    .GroupBy(e => e.MemberName ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Message).ToArray()
                    );

                return Results.ValidationProblem(errors,
                    title: "One or more validation errors occurred.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        return await next(context);
    }
}

/// <summary>
/// Extension methods for applying Sannr validation to Minimal API endpoints.
/// </summary>
public static class SannrEndpointExtensions
{
    /// <summary>
    /// Adds Sannr validation to the endpoint.
    /// </summary>
    /// <param name="builder">The endpoint builder.</param>
    /// <returns>The updated endpoint builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown during endpoint construction when the endpoint binds a model that has no registered
    /// validator and <see cref="SannrValidationOptions.RequireValidator"/> is enabled.
    /// </exception>
    public static RouteHandlerBuilder WithSannrValidation(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddEndpointFilterFactory(static (factoryContext, next) =>
        {
            SannrEndpointGuard.EnsureValidatorsRegistered(factoryContext);
            return next;
        });

        return builder.AddEndpointFilter<SannrValidationFilter>();
    }

    /// <summary>
    /// Adds Sannr validation to all endpoints in the group.
    /// </summary>
    /// <param name="builder">The route group builder.</param>
    /// <returns>The updated route group builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown during endpoint construction when an endpoint in the group binds a model that has no
    /// registered validator and <see cref="SannrValidationOptions.RequireValidator"/> is enabled.
    /// </exception>
    public static RouteGroupBuilder WithSannrValidation(this RouteGroupBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddEndpointFilterFactory(static (factoryContext, next) =>
        {
            SannrEndpointGuard.EnsureValidatorsRegistered(factoryContext);
            return next;
        });

        return builder.AddEndpointFilter<SannrValidationFilter>();
    }
}

/// <summary>
/// Startup-time guard that makes <c>WithSannrValidation</c> fail closed.
/// </summary>
/// <remarks>
/// An endpoint that asks for validation but has no validator to run is almost always a wiring or
/// generator failure. Left alone it produces an API that accepts every input and reports nothing,
/// which is the most dangerous failure mode available to a validation library. This guard turns
/// that silent condition into a loud one at endpoint construction, before any request is served.
/// </remarks>
internal static class SannrEndpointGuard
{
    internal static void EnsureValidatorsRegistered(EndpointFilterFactoryContext factoryContext)
    {
        var services = factoryContext.ApplicationServices;
        var options = services.GetService<SannrValidationOptions>()
            ?? services.GetService<IOptions<SannrValidationOptions>>()?.Value
            ?? new SannrValidationOptions();
        if (!options.RequireValidator)
        {
            return;
        }

        var isService = services.GetService<IServiceProviderIsService>();
        var missing = new List<Type>();

        foreach (var parameter in factoryContext.MethodInfo.GetParameters())
        {
            var type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;

            if (!IsBoundModel(type, parameter, isService))
            {
                continue;
            }

            if (!global::Sannr.SannrValidatorRegistry.TryGetValidator(type, out var validator) || validator == null)
            {
                missing.Add(type);
            }
        }

        if (missing.Count == 0)
        {
            return;
        }

        var names = string.Join(", ", missing.Select(static t => t.FullName ?? t.Name));
        throw new InvalidOperationException(
            $"WithSannrValidation was applied to an endpoint that binds {names}, but no Sannr " +
            $"validator is registered for {(missing.Count == 1 ? "that type" : "those types")}. " +
            "Refusing to build the endpoint, because it would otherwise accept every request " +
            "without validating it. Check that the model is annotated, that the Sannr generator " +
            "ran for the declaring assembly, and that the generated registration is invoked at " +
            "startup. To opt out deliberately, set SannrValidationOptions.RequireValidator to false.");
    }

    /// <summary>
    /// Determines whether a parameter is a model bound from the request, as opposed to an injected
    /// service, a framework primitive, or a simple scalar route/query value.
    /// </summary>
    private static bool IsBoundModel(Type type, ParameterInfo parameter, IServiceProviderIsService? isService)
    {
        if (isService?.IsService(type) == true)
        {
            return false;
        }

        if (parameter.GetCustomAttributes().Any(static a =>
                a.GetType().Name is "FromServicesAttribute" or "FromKeyedServicesAttribute"))
        {
            return false;
        }

        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly) ||
            type == typeof(TimeOnly) || type == typeof(TimeSpan) || type == typeof(Guid) || type == typeof(Uri))
        {
            return false;
        }

        // Interfaces and abstract types are service contracts, not bound models.
        if (type.IsInterface || type.IsAbstract)
        {
            return false;
        }

        var ns = type.Namespace ?? string.Empty;
        if (ns.StartsWith("System", StringComparison.Ordinal) ||
            ns.StartsWith("Microsoft", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }
}
