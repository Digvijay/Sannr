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
/// Provides context for Sannr validation, including the object instance, DI provider, and items.
/// </summary>
public class SannrValidationContext
{
    /// <summary>
    /// Gets the object instance being validated.
    /// </summary>
    public object ObjectInstance { get; }
    /// <summary>
    /// Gets the service provider for dependency injection.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; }
    /// <summary>
    /// Gets the dictionary of contextual items.
    /// </summary>
    public IDictionary<object, object?> Items { get; }
    /// <summary>
    /// Gets the active validation group, if any.
    /// </summary>
    public string? ActiveGroup { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SannrValidationContext"/> class.
    /// </summary>
    /// <param name="instance">The object instance being validated.</param>
    /// <param name="serviceProvider">The service provider for DI.</param>
    /// <param name="items">The contextual items dictionary.</param>
    /// <param name="group">The active validation group.</param>
    public SannrValidationContext(object instance, IServiceProvider? serviceProvider = null, IDictionary<object, object?>? items = null, string? group = null)
    {
        ObjectInstance = instance;
        ServiceProvider = serviceProvider;
        Items = items ?? new Dictionary<object, object?>();
        ActiveGroup = group;
    }
}
