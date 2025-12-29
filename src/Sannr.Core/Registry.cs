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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
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
    /// Registers a validator for a specific type using a factory function.
    /// This AoT-compatible approach allows explicit registration without reflection.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="validatorFactory">A function that creates the validator.</param>
    public static void Register<T>(Func<SannrValidatorFuncAsync> validatorFactory)
    {
        _validators[typeof(T)] = validatorFactory();
    }

    /// <summary>
    /// Registers a pre-built validator for a specific type.
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    /// <param name="validator">The validator function.</param>
    public static void Register<T>(SannrValidatorFuncAsync validator)
    {
        _validators[typeof(T)] = validator;
    }

    /// <summary>
    /// Tries to get a validator for a given type.
    /// </summary>
    /// <param name="type">The type to look up.</param>
    /// <param name="validator">The found validator, if any.</param>
    /// <returns>True if a validator was found; otherwise, false.</returns>
    public static bool TryGetValidator(Type type, out SannrValidatorFuncAsync? validator) => _validators.TryGetValue(type, out validator);

    /// <summary>
    /// Automatically discovers and registers validators for types with validation attributes.
    /// This method should be called at application startup to populate the registry.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for types with validation attributes. If null, scans the calling assembly.</param>
    public static void AutoRegisterValidators(params Assembly[]? assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && HasValidationAttributes(t))
                .ToArray();

            foreach (var type in types)
            {
                var validator = GenerateValidatorForType(type);
                if (validator != null)
                {
                    Register(type, validator);
                }
            }
        }
    }

    private static bool HasValidationAttributes(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetCustomAttributes(typeof(SannrValidationAttribute), true).Any());
    }

    private static SannrValidatorFuncAsync? GenerateValidatorForType(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttributes(typeof(SannrValidationAttribute), true).Any() ||
                       p.GetCustomAttributes(typeof(SanitizeAttribute), true).Any() ||
                       p.GetCustomAttributes(typeof(CustomValidatorAttribute), true).Any())
            .ToArray();

        if (properties.Length == 0)
            return null;

        // Create a compiled validator function using reflection
        return async (SannrValidationContext context) =>
        {
            var result = ValidationResult.Success();
            var instance = context.ObjectInstance;

            // Handle sanitization first
            foreach (var property in properties)
            {
                var sanitizeAttrs = property.GetCustomAttributes(typeof(SanitizeAttribute), true)
                    .Cast<SanitizeAttribute>()
                    .ToArray();

                if (sanitizeAttrs.Any() && property.PropertyType == typeof(string) && property.CanWrite)
                {
                    var value = property.GetValue(instance) as string;
                    if (value != null)
                    {
                        foreach (var attr in sanitizeAttrs)
                        {
                            if (attr.Trim) value = value.Trim();
                            if (attr.ToUpper) value = value.ToUpper();
                            if (attr.ToLower) value = value.ToLower();
                        }
                        property.SetValue(instance, value);
                    }
                }
            }

            // Handle validation
            foreach (var property in properties)
            {
                var validationAttributes = property.GetCustomAttributes(typeof(SannrValidationAttribute), true)
                    .Cast<SannrValidationAttribute>()
                    .ToArray();

                var customValidators = property.GetCustomAttributes(typeof(CustomValidatorAttribute), true)
                    .Cast<CustomValidatorAttribute>()
                    .ToArray();

                var propertyValue = property.GetValue(instance);
                var propertyName = GetDisplayName(property);

                foreach (var attr in validationAttributes)
                {
                    // Check if validation should run based on group
                    if (!string.IsNullOrEmpty(attr.Group))
                    {
                        if (context.ActiveGroup != attr.Group)
                            continue;
                    }

                    // Perform validation based on attribute type
                    var validationResult = ValidateProperty(instance, property, propertyValue, propertyName, attr, context.ServiceProvider);
                    if (validationResult.Errors.Any())
                    {
                        result.Errors.AddRange(validationResult.Errors);
                    }
                }

                // Handle custom validators
                foreach (var customValidator in customValidators)
                {
                    var validationResult = await ValidateCustom(property, propertyValue, customValidator, context.ServiceProvider);
                    if (validationResult.Errors.Any())
                    {
                        result.Errors.AddRange(validationResult.Errors);
                    }
                }
            }

            return result;
        };
    }

    private static ValidationResult ValidateProperty(object instance, PropertyInfo property, object? propertyValue, string propertyName, SannrValidationAttribute attr, IServiceProvider? serviceProvider)
    {
        var result = new ValidationResult();

        if (attr is RequiredAttribute)
        {
            if (property.PropertyType == typeof(string))
            {
                if (string.IsNullOrEmpty(propertyValue as string))
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, $"{propertyName} is required."), attr.Severity);
                }
            }
            else if (propertyValue == null)
            {
                result.Add(propertyName, GetErrorMessage(attr, propertyName, $"{propertyName} is required."), attr.Severity);
            }
        }
        else if (attr is StringLengthAttribute stringLengthAttr)
        {
            if (property.PropertyType == typeof(string) && propertyValue is string str)
            {
                if (str.Length > stringLengthAttr.MaximumLength)
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, $"The field {propertyName} must be a string with a maximum length of {stringLengthAttr.MaximumLength}."), attr.Severity);
                }
                if (stringLengthAttr.MinimumLength > 0 && str.Length < stringLengthAttr.MinimumLength)
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, $"The field {propertyName} must be a string with a minimum length of {stringLengthAttr.MinimumLength}."), attr.Severity);
                }
            }
        }
        else if (attr is RangeAttribute rangeAttr)
        {
            if (propertyValue != null && (property.PropertyType == typeof(int) || property.PropertyType == typeof(double) || property.PropertyType == typeof(decimal)))
            {
                var value = Convert.ToDouble(propertyValue);
                if (value < rangeAttr.Minimum || value > rangeAttr.Maximum)
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, $"The field {propertyName} must be between {rangeAttr.Minimum} and {rangeAttr.Maximum}."), attr.Severity);
                }
            }
        }
        else if (attr is EmailAddressAttribute)
        {
            if (propertyValue is string str && !string.IsNullOrEmpty(str))
            {
                if (!Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, "Invalid email address"), attr.Severity);
                }
            }
        }
        else if (attr is UrlAttribute)
        {
            if (propertyValue is string str && !string.IsNullOrEmpty(str))
            {
                if (!Regex.IsMatch(str, @"^https?://[^\s/$.?#].[^\s]*$"))
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, "Invalid URL"), attr.Severity);
                }
            }
        }
        else if (attr is CreditCardAttribute)
        {
            if (propertyValue is string str && !string.IsNullOrEmpty(str))
            {
                if (!Regex.IsMatch(str, @"^\d{13,19}$"))
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, "Invalid credit card number"), attr.Severity);
                }
            }
        }
        else if (attr is PhoneAttribute)
        {
            if (propertyValue is string str && !string.IsNullOrEmpty(str))
            {
                if (!Regex.IsMatch(str, @"^[\+]?[1-9][\d]{0,15}$"))
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, "Invalid phone number"), attr.Severity);
                }
            }
        }
        else if (attr is FileExtensionsAttribute fileExtAttr)
        {
            if (propertyValue is string str && !string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(fileExtAttr.Extensions))
            {
                var extensions = fileExtAttr.Extensions.Split(',').Select(e => e.Trim().ToLower()).ToArray();
                var valid = extensions.Any(ext => str.EndsWith("." + ext, StringComparison.OrdinalIgnoreCase));
                if (!valid)
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, $"Invalid file extension. Allowed extensions: {fileExtAttr.Extensions}"), attr.Severity);
                }
            }
        }
        else if (attr is FutureDateAttribute)
        {
            if (propertyValue is DateTime dt && dt <= DateTime.Now)
            {
                result.Add(propertyName, GetErrorMessage(attr, propertyName, $"The field {propertyName} must be a future date."), attr.Severity);
            }
            else if (propertyValue != null && propertyValue.GetType() == typeof(DateTime?))
            {
                var dtNullable = (DateTime?)propertyValue;
                if (dtNullable.HasValue && dtNullable.Value <= DateTime.Now)
                {
                    result.Add(propertyName, GetErrorMessage(attr, propertyName, $"The field {propertyName} must be a future date."), attr.Severity);
                }
            }
        }
        else if (attr is AllowedValuesAttribute allowedValuesAttr)
        {
            if (propertyValue != null && !allowedValuesAttr.Values.Contains(propertyValue.ToString()))
            {
                result.Add(propertyName, GetErrorMessage(attr, propertyName, $"The field {propertyName} must be one of the allowed values."), attr.Severity);
            }
        }
        else if (attr is RequiredIfAttribute requiredIfAttr)
        {
            var otherProperty = property.DeclaringType?.GetProperty(requiredIfAttr.OtherProperty);
            if (otherProperty != null)
            {
                var otherValue = otherProperty.GetValue(instance);
                if (Equals(otherValue, requiredIfAttr.TargetValue))
                {
                    var isNullOrEmpty = property.PropertyType == typeof(string)
                        ? string.IsNullOrEmpty(propertyValue as string)
                        : propertyValue == null;

                    if (isNullOrEmpty)
                    {
                        result.Add(propertyName, GetErrorMessage(attr, propertyName, $"{propertyName} is required when {requiredIfAttr.OtherProperty} is {requiredIfAttr.TargetValue}."), attr.Severity);
                    }
                }
            }
        }
        else if (attr is ConditionalRangeAttribute conditionalRangeAttr)
        {
            var otherProperty = property.DeclaringType?.GetProperty(conditionalRangeAttr.OtherProperty);
            if (otherProperty != null)
            {
                var otherValue = otherProperty.GetValue(instance);
                if (Equals(otherValue, conditionalRangeAttr.TargetValue) && propertyValue != null)
                {
                    var value = Convert.ToDouble(propertyValue);
                    if (value < conditionalRangeAttr.Minimum || value > conditionalRangeAttr.Maximum)
                    {
                        result.Add(propertyName, GetErrorMessage(attr, propertyName, $"The field {propertyName} must be between {conditionalRangeAttr.Minimum} and {conditionalRangeAttr.Maximum} when {conditionalRangeAttr.OtherProperty} is {conditionalRangeAttr.TargetValue}."), attr.Severity);
                    }
                }
            }
        }

        return result;
    }

    private static async Task<ValidationResult> ValidateCustom(PropertyInfo property, object? propertyValue, CustomValidatorAttribute attr, IServiceProvider? serviceProvider)
    {
        var result = new ValidationResult();

        try
        {
            var validatorMethod = attr.ValidatorType.GetMethod(attr.MethodName, BindingFlags.Public | BindingFlags.Static);
            if (validatorMethod == null)
                return result;

            object? customResult;
            if (attr.IsAsync)
            {
                var task = (Task)validatorMethod.Invoke(null, new[] { propertyValue, serviceProvider })!;
                await task;
                var resultProperty = task.GetType().GetProperty("Result");
                customResult = resultProperty?.GetValue(task);
            }
            else
            {
                customResult = validatorMethod.Invoke(null, new[] { propertyValue, serviceProvider });
            }

            if (customResult is ValidationResult customValidationResult)
            {
                result.Errors.AddRange(customValidationResult.Errors);
            }
        }
        catch
        {
            // If custom validation fails, ignore it
        }

        return result;
    }

    private static string GetDisplayName(PropertyInfo property)
    {
        var displayAttr = property.GetCustomAttribute(typeof(DisplayAttribute), true) as DisplayAttribute;
        return displayAttr?.Name ?? property.Name;
    }

    private static string GetErrorMessage(SannrValidationAttribute attr, string propertyName, string defaultMessage)
    {
        if (!string.IsNullOrEmpty(attr.ErrorMessage))
            return attr.ErrorMessage;
        if (!string.IsNullOrEmpty(attr.ErrorMessageResourceName) && attr.ErrorMessageResourceType != null)
        {
            var property = attr.ErrorMessageResourceType.GetProperty(attr.ErrorMessageResourceName, BindingFlags.Public | BindingFlags.Static);
            if (property != null && property.PropertyType == typeof(string))
            {
                return (string?)property.GetValue(null) ?? defaultMessage;
            }
        }
        return defaultMessage;
    }
}
