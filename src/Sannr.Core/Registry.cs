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
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Sannr;

/// <summary>
/// Represents a Sannr validator function delegate.
/// </summary>
/// <param name="context">The validation context.</param>
/// <returns>A task that returns a <see cref="ValidationResult"/>.</returns>
public delegate Task<ValidationResult> SannrValidatorFuncAsync(SannrValidationContext context);

/// <summary>
/// Registry for Sannr validators, allowing registration and lookup by type.
/// </summary>
public static class SannrValidatorRegistry
{
    private static readonly ConcurrentDictionary<Type, SannrValidatorFuncAsync> _validators = new();
    /// <summary>
    /// Registers a validator for a given type.
    /// </summary>
    /// <param name="type">The type to register the validator for.</param>
    /// <param name="validator">The validator function.</param>
    public static void Register(Type type, SannrValidatorFuncAsync validator) => _validators[type] = validator;
    /// <summary>
    /// Tries to get a validator for a given type.
    /// </summary>
    /// <param name="type">The type to look up.</param>
    /// <param name="validator">The found validator, if any.</param>
    /// <returns>True if a validator was found; otherwise, false.</returns>
    public static bool TryGetValidator(Type type, out SannrValidatorFuncAsync? validator) => _validators.TryGetValue(type, out validator);
}
