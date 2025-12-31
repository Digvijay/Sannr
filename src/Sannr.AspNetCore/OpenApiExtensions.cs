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
        // TODO: Add the schema filter once the correct API is determined
        // For now, users should manually add: options.SchemaFilter<SannrValidationSchemaFilter>();
        // or options.Filters.Add(new SchemaFilterDescriptor(typeof(SannrValidationSchemaFilter), null));
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