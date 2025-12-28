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
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sannr.AspNetCore;

/// <summary>
/// Enhanced problem details factory that integrates with Sannr validation results.
/// </summary>
public class SannrProblemDetailsFactory : ProblemDetailsFactory
{
    private readonly SannrValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SannrProblemDetailsFactory"/> class.
    /// </summary>
    /// <param name="options">The Sannr validation options.</param>
    public SannrProblemDetailsFactory(IOptions<SannrValidationOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance that configures defaults based on the specified <see cref="HttpContext"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
    /// <returns>The created <see cref="ProblemDetails"/> instance.</returns>
    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        // Check if we have enhanced validation results
        if (_options.EnableEnhancedErrorResponses &&
            httpContext.Items.TryGetValue("SannrValidationResult", out var validationResult) &&
            validationResult is EnhancedValidationResult enhancedResult)
        {
            var problemDetails = new SannrValidationProblemDetails(
                enhancedResult.Errors,
                enhancedResult.ModelType,
                enhancedResult.CorrelationId,
                enhancedResult.ValidationDurationMs);

            // Set standard problem details properties
            problemDetails.Status = statusCode ?? StatusCodes.Status400BadRequest;
            problemDetails.Title = title ?? "One or more validation errors occurred.";
            problemDetails.Type = type ?? "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            problemDetails.Detail = detail;
            problemDetails.Instance = instance ?? httpContext.Request.Path;

            return problemDetails;
        }

        // Fall back to default
        var defaultProblemDetails = new ProblemDetails
        {
            Status = statusCode ?? StatusCodes.Status500InternalServerError,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };
        return defaultProblemDetails;
    }

    /// <summary>
    /// Creates a <see cref="ValidationProblemDetails"/> instance that configures defaults based on the specified <see cref="HttpContext"/> and <see cref="ModelStateDictionary"/>.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
    /// <param name="modelStateDictionary">The <see cref="ModelStateDictionary"/>.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="title">The problem title.</param>
    /// <param name="type">The problem type.</param>
    /// <param name="detail">The problem detail.</param>
    /// <param name="instance">The problem instance.</param>
    /// <returns>The created <see cref="ValidationProblemDetails"/> instance.</returns>
    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        // Check if we have enhanced validation results
        if (_options.EnableEnhancedErrorResponses &&
            httpContext.Items.TryGetValue("SannrValidationResult", out var validationResult) &&
            validationResult is EnhancedValidationResult enhancedResult)
        {
            var problemDetails = new SannrValidationProblemDetails(
                enhancedResult.Errors,
                enhancedResult.ModelType,
                enhancedResult.CorrelationId,
                enhancedResult.ValidationDurationMs);

            // Set standard problem details properties
            problemDetails.Status = statusCode ?? StatusCodes.Status400BadRequest;
            problemDetails.Title = title ?? "One or more validation errors occurred.";
            problemDetails.Type = type ?? "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            problemDetails.Detail = detail;
            problemDetails.Instance = instance ?? httpContext.Request.Path;

            // Add model state errors as well
            foreach (var entry in modelStateDictionary)
            {
                if (entry.Value?.Errors.Count > 0)
                {
                    var errors = entry.Value.Errors.Select(e => e.ErrorMessage).ToArray();
                    if (problemDetails.Errors.ContainsKey(entry.Key))
                    {
                        // Merge with existing errors
                        var existingErrors = problemDetails.Errors[entry.Key];
                        problemDetails.Errors[entry.Key] = existingErrors.Concat(errors).ToArray();
                    }
                    else
                    {
                        problemDetails.Errors[entry.Key] = errors;
                    }
                }
            }

            return problemDetails;
        }

        // Fall back to default
        var defaultProblemDetails = new ValidationProblemDetails(modelStateDictionary)
        {
            Status = statusCode ?? StatusCodes.Status400BadRequest,
            Title = title ?? "One or more validation errors occurred.",
            Type = type ?? "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Detail = detail,
            Instance = instance ?? httpContext.Request.Path
        };
        return defaultProblemDetails;
    }
}

/// <summary>
/// Extension methods for configuring enhanced error responses.
/// </summary>
public static class SannrProblemDetailsExtensions
{
    /// <summary>
    /// Adds Sannr enhanced problem details factory to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSannrProblemDetails(this IServiceCollection services)
    {
        // Register the enhanced problem details factory
        services.AddSingleton<ProblemDetailsFactory, SannrProblemDetailsFactory>();

        return services;
    }
}