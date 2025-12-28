// ----------------------------------------------------------------------------------
// MIT License
//
// Copyright (c) 2025 Sannr contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System;
using System.Collections.Generic;

namespace Sannr.AspNetCore;

/// <summary>
/// Options for configuring Sannr validation.
/// </summary>
public class SannrValidationOptions
{
    /// <summary>
    /// Gets or sets whether to enable performance metrics collection.
    /// </summary>
    public bool EnableMetrics { get; set; } = false;

    /// <summary>
    /// Gets or sets the prefix for metrics names.
    /// </summary>
    public string MetricsPrefix { get; set; } = "sannr_validation";

    /// <summary>
    /// Gets or sets whether to enable enhanced error responses with correlation IDs and validation rule metadata.
    /// </summary>
    public bool EnableEnhancedErrorResponses { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include validation rule metadata in error responses.
    /// </summary>
    public bool IncludeValidationRuleMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include validation duration in error responses.
    /// </summary>
    public bool IncludeValidationDuration { get; set; } = false;
}

/// <summary>
/// Service for collecting Sannr validation metrics.
/// </summary>
public interface ISannrMetricsCollector
{
    /// <summary>
    /// Records the duration of a validation operation.
    /// </summary>
    /// <param name="modelType">The type of model being validated.</param>
    /// <param name="duration">The duration in milliseconds.</param>
    void RecordValidationDuration(string modelType, double duration);

    /// <summary>
    /// Records validation errors.
    /// </summary>
    /// <param name="modelType">The type of model being validated.</param>
    /// <param name="errorCount">The number of validation errors.</param>
    void RecordValidationErrors(string modelType, long errorCount);
}

/// <summary>
/// Default implementation of metrics collector using System.Diagnostics.Metrics.
/// </summary>
public class SannrMetricsCollector : ISannrMetricsCollector
{
    private readonly Meter _meter;
    private readonly Histogram<double> _validationDuration;
    private readonly Counter<long> _validationErrors;

    public SannrMetricsCollector(SannrValidationOptions options)
    {
        _meter = new Meter(options.MetricsPrefix);
        _validationDuration = _meter.CreateHistogram<double>(
            "validation_duration",
            "ms",
            "Duration of validation operations");
        _validationErrors = _meter.CreateCounter<long>(
            "validation_errors_total",
            description: "Total number of validation errors");
    }

    public void RecordValidationDuration(string modelType, double duration)
    {
        _validationDuration.Record(duration, new KeyValuePair<string, object?>("model_type", modelType));
    }

    public void RecordValidationErrors(string modelType, long errorCount)
    {
        if (errorCount > 0)
        {
            _validationErrors.Add(errorCount, new KeyValuePair<string, object?>("model_type", modelType));
        }
    }
}

/// <summary>
/// No-op implementation of metrics collector for when metrics are disabled.
/// </summary>
public class NoOpMetricsCollector : ISannrMetricsCollector
{
    public void RecordValidationDuration(string modelType, double duration) { }
    public void RecordValidationErrors(string modelType, long errorCount) { }
}

/// <summary>
/// Enhanced validation result stored in HttpContext for problem details generation.
/// </summary>
public class EnhancedValidationResult
{
    /// <summary>
    /// Gets the model type that was validated.
    /// </summary>
    public string ModelType { get; }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Gets the correlation ID.
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    /// Gets the validation duration in milliseconds.
    /// </summary>
    public double? ValidationDurationMs { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhancedValidationResult"/> class.
    /// </summary>
    /// <param name="modelType">The model type.</param>
    /// <param name="errors">The validation errors.</param>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="validationDurationMs">The validation duration.</param>
    public EnhancedValidationResult(string modelType, IReadOnlyList<ValidationError> errors, string correlationId, double? validationDurationMs)
    {
        ModelType = modelType;
        Errors = errors;
        CorrelationId = correlationId;
        ValidationDurationMs = validationDurationMs;
    }
}

/// <summary>
/// Provides an AOT-friendly object model validator for ASP.NET Core that integrates with Sannr validation.
/// </summary>
public class AotObjectModelValidator : IObjectModelValidator
{
    private readonly ISannrMetricsCollector _metricsCollector;
    private readonly SannrValidationOptions _options;

    public AotObjectModelValidator(ISannrMetricsCollector metricsCollector, SannrValidationOptions options)
    {
        _metricsCollector = metricsCollector;
        _options = options;
    }

    /// <summary>
    /// Validates the specified model using Sannr validators and adds errors to the model state.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    /// <param name="validationState">The validation state dictionary.</param>
    /// <param name="prefix">The prefix for model keys.</param>
    /// <param name="model">The model to validate.</param>
    public void Validate(ActionContext actionContext, ValidationStateDictionary? validationState, string prefix, object? model)
    {
        if (model == null) return;

        var modelType = model.GetType().Name;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator))
            {
                var sannrContext = new SannrValidationContext(
                    instance: model,
                    serviceProvider: actionContext.HttpContext?.RequestServices,
                    items: actionContext.HttpContext?.Items,
                    group: null
                );

                var result = validator!(sannrContext).GetAwaiter().GetResult();

                stopwatch.Stop();
                _metricsCollector.RecordValidationDuration(modelType, stopwatch.Elapsed.TotalMilliseconds);
                _metricsCollector.RecordValidationErrors(modelType, result.Errors.Count);

                // Store enhanced validation result if enabled
                if (_options.EnableEnhancedErrorResponses && actionContext.HttpContext != null)
                {
                    var correlationId = actionContext.HttpContext.Request.Headers["X-Correlation-ID"].ToString()
                        ?? Guid.NewGuid().ToString();

                    var enhancedResult = new EnhancedValidationResult(
                        modelType: model.GetType().Name,
                        errors: result.Errors,
                        correlationId: correlationId,
                        validationDurationMs: _options.IncludeValidationDuration ? stopwatch.Elapsed.TotalMilliseconds : null
                    );

                    actionContext.HttpContext.Items["SannrValidationResult"] = enhancedResult;
                }

                foreach (var err in result.Errors)
                {
                    if (err.Severity == Severity.Error)
                    {
                        var key = string.IsNullOrEmpty(prefix) ? err.MemberName : $"{prefix}.{err.MemberName}";
                        actionContext.ModelState.AddModelError(key, err.Message);
                    }
                }
            }
        }
        catch
        {
            stopwatch.Stop();
            _metricsCollector.RecordValidationDuration(modelType, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }
}

/// <summary>
/// Extension methods for registering Sannr validation in ASP.NET Core DI.
/// </summary>
public static class SannrExtensions
{
    /// <summary>
    /// Adds Sannr validation services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSannr(this IServiceCollection services)
    {
        return services.AddSannr(options => { });
    }

    /// <summary>
    /// Adds Sannr validation services to the specified service collection with options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The options configuration action.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSannr(this IServiceCollection services, Action<SannrValidationOptions> configureOptions)
    {
        var options = new SannrValidationOptions();
        configureOptions(options);

        services.AddSingleton(options);
        services.AddSingleton<IOptions<SannrValidationOptions>>(new OptionsWrapper<SannrValidationOptions>(options));
        services.AddSingleton<IObjectModelValidator, AotObjectModelValidator>();

        if (options.EnableMetrics)
        {
            services.AddSingleton<ISannrMetricsCollector>(sp => new SannrMetricsCollector(options));
        }
        else
        {
            services.AddSingleton<ISannrMetricsCollector, NoOpMetricsCollector>();
        }

        if (options.EnableEnhancedErrorResponses)
        {
            services.AddSannrProblemDetails();
        }

        return services;
    }
}
