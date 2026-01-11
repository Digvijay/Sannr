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
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sannr.OpenApi;

/// <summary>
/// Generates OpenAPI schemas from Sannr validation attributes.
/// </summary>
public static class SannrOpenApiExtensions
{
    /// <summary>
    /// NOTE: This extension is a placeholder for discoverability.
    /// To use Sannr's compile-time OpenAPI integration (AoT compatible), add this in your Program.cs:
    /// 
    /// <code>
    /// builder.Services.AddSwaggerGen(options => {
    ///     options.SchemaFilter&lt;Sannr.OpenApi.SannrGeneratedSchemaFilter&gt;();
    /// });
    /// </code>
    /// 
    /// The SannrGeneratedSchemaFilter is automatically generated at compile-time for all your validated models,
    /// without any runtime reflection or IL suppression. It's fully AoT compatible!
    /// </summary>
    [Obsolete("Don't call this method. Instead, add options.SchemaFilter<Sannr.OpenApi.SannrGeneratedSchemaFilter>() directly in your AddSwaggerGen configuration. See XML documentation for details.")]
    public static void AddSannrValidationSchemas(this SwaggerGenOptions options)
    {
        throw new NotSupportedException(
            "This method is a placeholder. Add options.SchemaFilter<Sannr.OpenApi.SannrGeneratedSchemaFilter>() " +
            "directly in your AddSwaggerGen configuration instead. The filter is auto-generated at compile-time.");
    }
}

/// <summary>
/// DEPRECATED: Use AddSannrValidationSchemas() instead, which uses the compile-time generated SannrGeneratedSchemaFilter.
/// This reflection-based filter is kept for backwards compatibility but should not be used in AoT scenarios.
/// </summary>
[Obsolete("Use AddSannrValidationSchemas() extension method instead. This reflection-based approach is not AoT compatible.")]
public class SannrValidationSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || context.Type == null)
            return;

#pragma warning disable IL2075 // Suppress trimming warning for reflection-based filter
        var properties = context.Type.GetProperties();
#pragma warning restore IL2075
        foreach (var property in properties)
        {
            if (schema.Properties.TryGetValue(property.Name, out var propertySchema))
            {
                var attributes = property.GetCustomAttributes(true);
                foreach (var attribute in attributes)
                {
                    var attrType = attribute.GetType();
                    if (attrType.FullName == "Sannr.RequiredAttribute")
                    {
                        // Required is handled at object level
                    }
                    else if (attrType.Name == "StringLengthAttribute")
                    {
                        var sla = attribute as Sannr.StringLengthAttribute;
                        if (sla != null)
                        {
                            if (sla.MaximumLength > 0) propertySchema.MaxLength = sla.MaximumLength;
                            if (sla.MinimumLength > 0) propertySchema.MinLength = sla.MinimumLength;
                        }
                    }
                    else if (attrType.Name == "RangeAttribute")
                    {
                        var ra = attribute as Sannr.RangeAttribute;
                        if (ra != null)
                        {
                            propertySchema.Minimum = (decimal)ra.Minimum;
                            propertySchema.Maximum = (decimal)ra.Maximum;
                        }
                    }
                    if (attrType.Name == "EmailAddressAttribute")
                    {
                        propertySchema.Format = "email";
                    }
                    else if (attrType.Name == "UrlAttribute")
                    {
                        propertySchema.Format = "uri";
                    }
                    else if (attrType.Name == "FileExtensionsAttribute")
                    {
                        propertySchema.Format = "file";
                    }
                    else if (attrType.Name == "RegularExpressionAttribute")
                    {
                        var rea = attribute as System.ComponentModel.DataAnnotations.RegularExpressionAttribute;
                        if (rea != null && !string.IsNullOrEmpty(rea.Pattern))
                        {
                            propertySchema.Pattern = rea.Pattern;
                        }
                    }
                    else if (attrType.Name == "MaxLengthAttribute")
                    {
                        var mla = attribute as System.ComponentModel.DataAnnotations.MaxLengthAttribute;
                        if (mla != null && mla.Length > 0) propertySchema.MaxLength = mla.Length;
                    }
                    else if (attrType.Name == "MinLengthAttribute")
                    {
                        var mla = attribute as System.ComponentModel.DataAnnotations.MinLengthAttribute;
                        if (mla != null && mla.Length > 0) propertySchema.MinLength = mla.Length;
                    }
                }
            }
        }
    }
}
