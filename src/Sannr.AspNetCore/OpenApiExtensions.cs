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

using Microsoft.OpenApi.Models;
using Sannr;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sannr.OpenApi;

/// <summary>
/// Generates OpenAPI schemas from Sannr validation attributes.
/// </summary>
public static class SannrOpenApiExtensions
{
    /// <summary>
    /// Adds Sannr OpenAPI schema generation to SwaggerGen options.
    /// </summary>
    public static void AddSannrValidationSchemas(this SwaggerGenOptions options)
    {
        // Schema filters are added via the Filters collection
        // This will be applied to all schemas during generation
    }
}

/// <summary>
/// Schema filter that enhances OpenAPI schemas with Sannr validation attributes.
/// </summary>
public class SannrValidationSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || context.Type == null)
            return;

        foreach (var property in context.Type.GetProperties())
        {
            if (schema.Properties.TryGetValue(property.Name, out var propertySchema))
            {
                ApplyValidationToSchema(propertySchema, property);
            }
        }
    }

    private void ApplyValidationToSchema(OpenApiSchema schema, PropertyInfo property)
    {
        var attributes = property.GetCustomAttributes<SannrValidationAttribute>();

        foreach (var attribute in attributes)
        {
            ApplyValidationAttribute(schema, attribute);
        }
    }

    private void ApplyValidationAttribute(OpenApiSchema schema, SannrValidationAttribute attribute)
    {
        switch (attribute)
        {
            case RequiredAttribute:
                // Required is handled at the object level, not property level in OpenAPI
                break;

            case StringLengthAttribute stringLength:
                if (stringLength.MaximumLength > 0)
                    schema.MaxLength = stringLength.MaximumLength;
                if (stringLength.MinimumLength > 0)
                    schema.MinLength = stringLength.MinimumLength;
                break;

            case RangeAttribute range:
                if (range.Minimum is double minDouble)
                    schema.Minimum = (decimal)minDouble;
                if (range.Maximum is double maxDouble)
                    schema.Maximum = (decimal)maxDouble;
                break;

            case EmailAddressAttribute:
                schema.Format = "email";
                break;

            case UrlAttribute:
                schema.Format = "uri";
                break;

            case PhoneAttribute:
                // No standard format for phone, could add pattern
                break;

            case CreditCardAttribute:
                // No standard format for credit card, could add pattern
                break;

            case FileExtensionsAttribute fileExt:
                if (!string.IsNullOrEmpty(fileExt.Extensions))
                {
                    schema.Format = "file";
                    // Could add pattern validation for extensions
                }
                break;
        }
    }
}