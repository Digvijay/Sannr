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
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sannr.OpenApi;

/// <summary>
/// Documentation and helpers for OpenAPI generation.
/// </summary>
public static class SannrOpenApiExtensions
{
    /// <summary>
    /// NOTE: To use Sannr's compile-time OpenAPI integration (AoT compatible), add this in your Program.cs:
    /// 
    /// <code>
    /// builder.Services.AddSwaggerGen(options => {
    ///     options.SchemaFilter&lt;Sannr.OpenApi.SannrGeneratedSchemaFilter&gt;();
    /// });
    /// </code>
    /// 
    /// The SannrGeneratedSchemaFilter is automatically generated at compile-time for all your validated models.
    /// </summary>
    [Obsolete("Use options.SchemaFilter<Sannr.OpenApi.SannrGeneratedSchemaFilter>() directly in your AddSwaggerGen configuration.")]
    public static void AddSannrValidationSchemas(this SwaggerGenOptions options)
    {
        // This is a documented-only extension to guide users to the generated filter.
        throw new NotSupportedException(
            "Use options.SchemaFilter<Sannr.OpenApi.SannrGeneratedSchemaFilter>() directly in your configuration.");
    }
}
