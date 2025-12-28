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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Sannr.AspNetCore;

/// <summary>
/// Provides an AOT-friendly object model validator for ASP.NET Core that integrates with Sannr validation.
/// </summary>
public class AotObjectModelValidator : IObjectModelValidator
{
    /// <summary>
    /// Validates the specified model using Sannr validators and adds errors to the model state.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    /// <param name="validationState">The validation state dictionary.</param>
    /// <param name="prefix">The prefix for model keys.</param>
    /// <param name="model">The model to validate.</param>
    public void Validate(ActionContext actionContext, ValidationStateDictionary? validationState, string prefix, object? model)
    {
        if (model == null) return;

        if (SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator))
        {
            var sannrContext = new SannrValidationContext(
                instance: model,
                serviceProvider: actionContext.HttpContext?.RequestServices,
                items: actionContext.HttpContext?.Items,
                group: null
            );

            var result = validator!(sannrContext).GetAwaiter().GetResult();

            foreach (var err in result.Errors)
            {
                if (err.Severity == Severity.Error)
                {
                    var key = string.IsNullOrEmpty(prefix) ? err.MemberName : $"{prefix}.{err.MemberName}";
                    actionContext.ModelState.AddModelError(key, err.Message);
                }
            }
        }
    }
}

/// <summary>
/// Extension methods for registering Sannr validation in ASP.NET Core DI.
/// </summary>
public static class SannrExtensions
{
    /// <summary>
    /// Adds Sannr validation services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSannr(this IServiceCollection services)
    {
        services.AddSingleton<IObjectModelValidator, AotObjectModelValidator>();
        return services;
    }
}
