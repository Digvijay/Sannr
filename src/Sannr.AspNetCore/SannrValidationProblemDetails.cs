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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Sannr.AspNetCore;

/// <summary>
/// Enhanced validation problem details with correlation IDs and validation rule metadata.
/// </summary>
public class SannrValidationProblemDetails : ValidationProblemDetails
{
    /// <summary>
    /// Gets or sets the correlation ID for tracking the request.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the validation rules that were applied.
    /// </summary>
    public Dictionary<string, ValidationRuleInfo> ValidationRules { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when the validation occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the model type that was validated.
    /// </summary>
    public string? ModelType { get; set; }

    /// <summary>
    /// Gets or sets the validation duration in milliseconds.
    /// </summary>
    public double? ValidationDurationMs { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SannrValidationProblemDetails"/> class.
    /// </summary>
    public SannrValidationProblemDetails()
    {
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        Title = "One or more validation errors occurred.";
        Status = StatusCodes.Status400BadRequest;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SannrValidationProblemDetails"/> class with validation errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <param name="modelType">The model type that was validated.</param>
    /// <param name="correlationId">The correlation ID for the request.</param>
    /// <param name="validationDurationMs">The validation duration in milliseconds.</param>
    public SannrValidationProblemDetails(
        IReadOnlyList<ValidationError> errors,
        string? modelType = null,
        string? correlationId = null,
        double? validationDurationMs = null)
        : this()
    {
        ModelType = modelType;
        CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        ValidationDurationMs = validationDurationMs;

        // Group errors by property
        foreach (var errorGroup in errors.GroupBy(e => e.MemberName ?? string.Empty))
        {
            Errors[errorGroup.Key] = errorGroup.Select(e => e.Message).ToArray();
        }

        // Extract validation rules from errors
        ValidationRules = ExtractValidationRules(errors);
    }

    private static Dictionary<string, ValidationRuleInfo> ExtractValidationRules(IReadOnlyList<ValidationError> errors)
    {
        var rules = new Dictionary<string, ValidationRuleInfo>();

        foreach (var error in errors)
        {
            var propertyName = error.MemberName ?? string.Empty;
            if (!rules.ContainsKey(propertyName))
            {
                rules[propertyName] = new ValidationRuleInfo();
            }

            var ruleInfo = rules[propertyName];

            // Extract rule information from error metadata
            // This would be enhanced based on the specific validation attributes used
            if (error.Message.Contains("required") || error.Message.Contains("Required"))
            {
                ruleInfo.Required = true;
            }

            if (error.Message.Contains("email") || error.Message.Contains("Email"))
            {
                ruleInfo.Email = true;
            }

            if (error.Message.Contains("between") || error.Message.Contains("range"))
            {
                // Try to extract range information from the message
                // This is a simplified implementation
                ruleInfo.Range = new RangeInfo { Min = 0, Max = int.MaxValue };
            }

            if (error.Message.Contains("length") || error.Message.Contains("Length"))
            {
                ruleInfo.StringLength = new StringLengthInfo { MinLength = 0, MaxLength = int.MaxValue };
            }
        }

        return rules;
    }
}

/// <summary>
/// Information about a validation rule.
/// </summary>
public class ValidationRuleInfo
{
    /// <summary>
    /// Gets or sets whether the property is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// Gets or sets whether the property must be a valid email.
    /// </summary>
    public bool Email { get; set; }

    /// <summary>
    /// Gets or sets whether the property must be a valid URL.
    /// </summary>
    public bool Url { get; set; }

    /// <summary>
    /// Gets or sets the range constraints.
    /// </summary>
    public RangeInfo? Range { get; set; }

    /// <summary>
    /// Gets or sets the string length constraints.
    /// </summary>
    public StringLengthInfo? StringLength { get; set; }

    /// <summary>
    /// Gets or sets the regular expression pattern.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Gets or sets the allowed values.
    /// </summary>
    public string[]? AllowedValues { get; set; }
}

/// <summary>
/// Information about range constraints.
/// </summary>
public class RangeInfo
{
    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public double Max { get; set; }
}

/// <summary>
/// Information about string length constraints.
/// </summary>
public class StringLengthInfo
{
    /// <summary>
    /// Gets or sets the minimum length.
    /// </summary>
    public int MinLength { get; set; }

    /// <summary>
    /// Gets or sets the maximum length.
    /// </summary>
    public int MaxLength { get; set; }
}

/// <summary>
/// Extension methods for creating enhanced validation problem details.
/// </summary>
public static class SannrValidationProblemDetailsExtensions
{
    /// <summary>
    /// Converts validation errors to enhanced problem details.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <param name="modelType">The model type that was validated.</param>
    /// <param name="correlationId">The correlation ID for the request.</param>
    /// <param name="validationDurationMs">The validation duration in milliseconds.</param>
    /// <returns>The enhanced problem details.</returns>
    public static SannrValidationProblemDetails ToSannrValidationProblemDetails(
        this IReadOnlyList<ValidationError> errors,
        string? modelType = null,
        string? correlationId = null,
        double? validationDurationMs = null)
    {
        return new SannrValidationProblemDetails(errors, modelType, correlationId, validationDurationMs);
    }

    /// <summary>
    /// Converts a Validated wrapper to enhanced problem details.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    /// <param name="validated">The validated wrapper.</param>
    /// <param name="correlationId">The correlation ID for the request.</param>
    /// <param name="validationDurationMs">The validation duration in milliseconds.</param>
    /// <returns>The enhanced problem details.</returns>
    public static SannrValidationProblemDetails ToSannrValidationProblemDetails<T>(
        this Validated<T> validated,
        string? correlationId = null,
        double? validationDurationMs = null) where T : class
    {
        if (validated.IsValid)
        {
            throw new InvalidOperationException("Cannot create problem details for valid model.");
        }

        return new SannrValidationProblemDetails(
            validated.Errors,
            typeof(T).Name,
            correlationId,
            validationDurationMs);
    }

    /// <summary>
    /// Converts enhanced problem details to a BadRequestObjectResult.
    /// </summary>
    /// <param name="problemDetails">The problem details.</param>
    /// <returns>The bad request result.</returns>
    public static BadRequestObjectResult ToBadRequestResult(this SannrValidationProblemDetails problemDetails)
    {
        return new BadRequestObjectResult(problemDetails);
    }
}
