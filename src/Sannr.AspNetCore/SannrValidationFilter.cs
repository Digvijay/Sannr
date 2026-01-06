using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Sannr.Core;
using System.Diagnostics;

namespace Sannr.AspNetCore;

/// <summary>
/// An endpoint filter that automatically validates Sannr-attributed models in Minimal APIs.
/// </summary>
public class SannrValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        for (int i = 0; i < context.Arguments.Count; i++)
        {
            var argument = context.Arguments[i];
            if (argument == null) continue;

            var modelType = argument.GetType();
            if (SannrValidatorRegistry.TryGetValidator(modelType, out var validator))
            {
                var options = context.HttpContext.RequestServices.GetService<SannrValidationOptions>() ?? new SannrValidationOptions();
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using var activity = Observability.ActivitySource.StartActivity($"Validation: {modelType.Name}");
                activity?.SetTag("sannr.model.type", modelType.FullName);
                activity?.SetTag("sannr.validation.group", null);

                var sannrContext = new SannrValidationContext(
                    instance: argument,
                    serviceProvider: context.HttpContext.RequestServices,
                    items: context.HttpContext.Items,
                    group: null
                );

                var result = await validator!(sannrContext);
                stopwatch.Stop();

                activity?.SetTag("sannr.validation.is_valid", result.IsValid);
                activity?.SetTag("sannr.validation.duration_ms", stopwatch.Elapsed.TotalMilliseconds);
                
                if (!result.IsValid)
                {
                    activity?.SetTag("sannr.validation.error_count", result.Errors.Count);
                    activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");
                    if (options.EnableEnhancedErrorResponses)
                    {
                        var correlationId = context.HttpContext.Request.Headers["X-Correlation-ID"].ToString()
                            ?? Guid.NewGuid().ToString();

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
    public static RouteHandlerBuilder WithSannrValidation(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<SannrValidationFilter>();
    }

    /// <summary>
    /// Adds Sannr validation to all endpoints in the group.
    /// </summary>
    /// <param name="builder">The route group builder.</param>
    /// <returns>The updated route group builder.</returns>
    public static RouteGroupBuilder WithSannrValidation(this RouteGroupBuilder builder)
    {
        return builder.AddEndpointFilter<SannrValidationFilter>();
    }
}
