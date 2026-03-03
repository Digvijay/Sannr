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
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sannr.OpenApi;

/// <summary>
/// AOT-compatible OpenAPI schema filter.
/// Always present in the Sannr package. When the source generator runs (because Sannr
/// model classes with validation attributes are present), it registers per-type schema
/// appliers via <see cref="RegisterApplier"/>.
/// Register with: options.SchemaFilter&lt;SannrGeneratedSchemaFilter&gt;()
/// </summary>
public class SannrGeneratedSchemaFilter : ISchemaFilter
{
    private static readonly Dictionary<string, Action<OpenApiSchema>> _appliers
        = new Dictionary<string, Action<OpenApiSchema>>(StringComparer.Ordinal);

    /// <summary>
    /// Called by source-generated code to register per-type schema appliers.
    /// </summary>
    public static void RegisterApplier(string typeFullName, Action<OpenApiSchema> applier)
    {
        _appliers[typeFullName] = applier;
    }

    /// <summary>
    /// Applies validation constraints to a schema based on the registered type appliers.
    /// </summary>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || context.Type == null)
            return;

        var typeName = context.Type.FullName;
        if (typeName != null && _appliers.TryGetValue(typeName, out var applier))
            applier(schema);
    }
}
