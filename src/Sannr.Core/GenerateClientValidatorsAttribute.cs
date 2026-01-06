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

namespace Sannr;

/// <summary>
/// Attribute to mark classes for client-side validation code generation.
/// When applied to a class, the Sannr source generator will create TypeScript/JavaScript
/// validation rules that mirror the server-side Sannr validation attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class GenerateClientValidatorsAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the output language for client-side validation code.
    /// Defaults to TypeScript.
    /// </summary>
    public ClientValidationLanguage Language { get; set; } = ClientValidationLanguage.TypeScript;

    /// <summary>
    /// Gets or sets the output directory for generated client validation files.
    /// If not specified, files will be generated in the project root under 'wwwroot/js/validation'.
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Gets or sets whether to generate validation functions in addition to rule objects.
    /// Defaults to true.
    /// </summary>
    public bool GenerateValidationFunctions { get; set; } = true;

    /// <summary>
    /// Gets or sets the namespace for generated TypeScript code.
    /// If not specified, uses the class namespace.
    /// </summary>
    public string? Namespace { get; set; }
}

/// <summary>
/// Specifies the output language for client-side validation code generation.
/// </summary>
public enum ClientValidationLanguage
{
    /// <summary>
    /// Generate TypeScript validation code (.ts files).
    /// </summary>
    TypeScript,

    /// <summary>
    /// Generate JavaScript validation code (.js files).
    /// </summary>
    JavaScript,

    /// <summary>
    /// Generate JSON validation schema.
    /// </summary>
    Json
}
