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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sannr;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sannr.AspNetCore;

/// <summary>
/// A wrapper for validated models in ASP.NET Core Minimal APIs.
/// Provides automatic validation and convenient methods for handling validation results.
/// </summary>
/// <typeparam name="T">The type of the model to validate.</typeparam>
public class Validated<T> where T : class
{
    private readonly T _value;
    private readonly ValidationResult _validationResult;

    /// <summary>
    /// Gets a value indicating whether the model is valid.
    /// </summary>
    public bool IsValid => _validationResult.IsValid;

    /// <summary>
    /// Gets the validated model value. Throws <see cref="InvalidOperationException"/> if the model is invalid.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing the value of an invalid model.</exception>
    public T Value
    {
        get
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Cannot access the value of an invalid model. Check IsValid first or use TryGetValue.");
            }
            return _value;
        }
    }

    /// <summary>
    /// Gets the collection of validation errors. Empty if the model is valid.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors => _validationResult.Errors;

    private Validated(T value, ValidationResult validationResult)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
        _validationResult = validationResult ?? throw new ArgumentNullException(nameof(validationResult));
    }

    /// <summary>
    /// Creates a new <see cref="Validated{T}"/> instance by validating the provided model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="serviceProvider">The service provider to resolve validation services.</param>
    /// <returns>A task that represents the asynchronous validation operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> or <paramref name="serviceProvider"/> is null.</exception>
    public static async Task<Validated<T>> CreateAsync(T model, IServiceProvider serviceProvider)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        var metricsCollector = serviceProvider.GetService<ISannrMetricsCollector>() ?? new NoOpMetricsCollector();
        var modelType = typeof(T).Name;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (!SannrValidatorRegistry.TryGetValidator(typeof(T), out var validator))
            {
                // If no validator is registered, consider the model valid
                stopwatch.Stop();
                metricsCollector.RecordValidationDuration(modelType, stopwatch.Elapsed.TotalMilliseconds);
                return new Validated<T>(model, ValidationResult.Success());
            }

            var sannrContext = new SannrValidationContext(
                instance: model,
                serviceProvider: serviceProvider,
                items: new System.Collections.Generic.Dictionary<object, object?>(),
                group: null
            );

            var validationResult = await validator!(sannrContext);
            
            stopwatch.Stop();
            metricsCollector.RecordValidationDuration(modelType, stopwatch.Elapsed.TotalMilliseconds);
            metricsCollector.RecordValidationErrors(modelType, validationResult.Errors.Count);
            
            return new Validated<T>(model, validationResult);
        }
        catch
        {
            stopwatch.Stop();
            metricsCollector.RecordValidationDuration(modelType, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Attempts to get the validated value without throwing an exception.
    /// </summary>
    /// <param name="value">The validated value if the model is valid; otherwise, the default value.</param>
    /// <returns><c>true</c> if the model is valid and the value was retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(out T value)
    {
        if (IsValid)
        {
            value = _value;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Converts the validation errors to a <see cref="ValidationProblemDetails"/> object.
    /// </summary>
    /// <param name="title">The title for the problem details. Defaults to "One or more validation errors occurred."</param>
    /// <returns>A <see cref="ValidationProblemDetails"/> object containing the validation errors.</returns>
    public ValidationProblemDetails ToValidationProblemDetails(string? title = null)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var error in Errors)
        {
            var key = error.MemberName ?? string.Empty;
            if (!errors.ContainsKey(key))
            {
                errors[key] = new[] { error.Message };
            }
            else
            {
                var existingErrors = errors[key];
                Array.Resize(ref existingErrors, existingErrors.Length + 1);
                existingErrors[^1] = error.Message;
                errors[key] = existingErrors;
            }
        }

        return new ValidationProblemDetails(errors)
        {
            Title = title ?? "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };
    }

    /// <summary>
    /// Converts the validation errors to a <see cref="BadRequestObjectResult"/> for use in action results.
    /// </summary>
    /// <param name="title">The title for the problem details. Defaults to "One or more validation errors occurred."</param>
    /// <returns>A <see cref="BadRequestObjectResult"/> containing the validation problem details.</returns>
    public BadRequestObjectResult ToBadRequestResult(string? title = null)
    {
        return new BadRequestObjectResult(ToValidationProblemDetails(title));
    }

    /// <summary>
    /// Implicitly converts a <see cref="Validated{T}"/> to its underlying value if valid.
    /// Throws <see cref="InvalidOperationException"/> if the model is invalid.
    /// </summary>
    /// <param name="validated">The validated wrapper.</param>
    public static implicit operator T(Validated<T> validated)
    {
        return validated.Value;
    }
}