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

namespace Sannr;

/// <summary>
/// Base attribute for Sannr validation attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class SannrValidationAttribute : Attribute 
{
    /// <summary>
    /// The error message to use if validation fails.
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// The resource name for the error message.
    /// </summary>
    public string? ErrorMessageResourceName { get; set; }
    /// <summary>
    /// The resource type for the error message.
    /// </summary>
    public Type? ErrorMessageResourceType { get; set; }
    /// <summary>
    /// The severity of the validation error.
    /// </summary>
    public Severity Severity { get; set; } = Severity.Error;
    /// <summary>
    /// The group to which this validation belongs.
    /// </summary>
    public string? Group { get; set; } 
}

/// <summary>
/// Specifies that a property is required.
/// </summary>
public class RequiredAttribute : SannrValidationAttribute { }

/// <summary>
/// Specifies the minimum and maximum length of characters for a string property.
/// </summary>
public class StringLengthAttribute : SannrValidationAttribute 
{
    /// <summary>
    /// Gets the maximum allowable length.
    /// </summary>
    public int MaximumLength { get; }
    /// <summary>
    /// Gets or sets the minimum allowable length.
    /// </summary>
    public int MinimumLength { get; set; }
    /// <summary>
    /// Initializes a new instance of the <see cref="StringLengthAttribute"/> class.
    /// </summary>
    /// <param name="maximumLength">The maximum length.</param>
    public StringLengthAttribute(int maximumLength) => MaximumLength = maximumLength;
}

/// <summary>
/// Specifies the minimum and maximum value for a numeric property.
/// </summary>
public class RangeAttribute : SannrValidationAttribute 
{
    /// <summary>
    /// Gets the minimum allowable value.
    /// </summary>
    public double Minimum { get; }
    /// <summary>
    /// Gets the maximum allowable value.
    /// </summary>
    public double Maximum { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="RangeAttribute"/> class.
    /// </summary>
    /// <param name="minimum">The minimum value.</param>
    /// <param name="maximum">The maximum value.</param>
    public RangeAttribute(double minimum, double maximum) { Minimum = minimum; Maximum = maximum; }
    public RangeAttribute(int minimum, int maximum) { Minimum = minimum; Maximum = maximum; }
}

/// <summary>
/// Specifies that a property must be a valid email address.
/// </summary>
public class EmailAddressAttribute : SannrValidationAttribute { }
/// <summary>
/// Specifies that a property must be a valid credit card number.
/// </summary>
public class CreditCardAttribute : SannrValidationAttribute { }
/// <summary>
/// Specifies that a property must be a valid URL.
/// </summary>
public class UrlAttribute : SannrValidationAttribute { }

public class PhoneAttribute : SannrValidationAttribute { }

public class FileExtensionsAttribute : SannrValidationAttribute
{
    public string? Extensions { get; set; }
}

/// <summary>
/// Specifies that a date property must be in the future.
/// </summary>
public class FutureDateAttribute : SannrValidationAttribute { }

/// <summary>
/// Specifies that a property value must be one of the allowed values.
/// </summary>
public class AllowedValuesAttribute : SannrValidationAttribute
{
    /// <summary>
    /// Gets the allowed values.
    /// </summary>
    public string[] Values { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedValuesAttribute"/> class.
    /// </summary>
    /// <param name="values">The allowed values.</param>
    public AllowedValuesAttribute(params string[] values)
    {
        Values = values ?? Array.Empty<string>();
    }
}

/// <summary>
/// Specifies a conditional range validation based on another property's value.
/// </summary>
public class ConditionalRangeAttribute : SannrValidationAttribute
{
    /// <summary>
    /// Gets the name of the other property to check.
    /// </summary>
    public string OtherProperty { get; }

    /// <summary>
    /// Gets the target value that the other property must have.
    /// </summary>
    public object TargetValue { get; }

    /// <summary>
    /// Gets the minimum allowable value.
    /// </summary>
    public double Minimum { get; }

    /// <summary>
    /// Gets the maximum allowable value.
    /// </summary>
    public double Maximum { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalRangeAttribute"/> class.
    /// </summary>
    /// <param name="otherProperty">The name of the other property.</param>
    /// <param name="targetValue">The target value.</param>
    /// <param name="minimum">The minimum value.</param>
    /// <param name="maximum">The maximum value.</param>
    public ConditionalRangeAttribute(string otherProperty, object targetValue, double minimum, double maximum)
    {
        OtherProperty = otherProperty;
        TargetValue = targetValue;
        Minimum = minimum;
        Maximum = maximum;
    }
}

public class RequiredIfAttribute : SannrValidationAttribute 
{
    public string OtherProperty { get; }
    public object TargetValue { get; }
    public RequiredIfAttribute(string otherProperty, object targetValue) 
    {
        OtherProperty = otherProperty;
        TargetValue = targetValue;
    }
}

public class SanitizeAttribute : Attribute 
{ 
    public bool Trim { get; set; } 
    public bool ToUpper { get; set; }
    public bool ToLower { get; set; }
}

public class DisplayAttribute : Attribute 
{ 
    public string? Name { get; set; } 
}

[AttributeUsage(AttributeTargets.Property)]
public class CustomValidatorAttribute : Attribute 
{
    public Type ValidatorType { get; }
    public string MethodName { get; }
    public bool IsAsync { get; set; }
    public CustomValidatorAttribute(Type validatorType, string methodName = "Check", bool isAsync = false) 
    { 
        ValidatorType = validatorType; 
        MethodName = methodName;
        IsAsync = isAsync;
    }
}

/// <summary>
/// Defines a method that validates an object.
/// </summary>
public interface IValidatableObject
{
    /// <summary>
    /// Validates the object and returns a collection of validation results.
    /// </summary>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A collection of validation results.</returns>
    System.Collections.Generic.IEnumerable<ModelValidationResult> Validate(SannrValidationContext validationContext);
}

/// <summary>
/// Represents the result of validating a single property or model.
/// </summary>
public class ModelValidationResult
{
    /// <summary>
    /// Gets or sets the name of the member that was validated.
    /// </summary>
    public string? MemberName { get; set; }

    /// <summary>
    /// Gets or sets the validation error message.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Gets or sets the severity of the validation error.
    /// </summary>
    public Severity Severity { get; set; } = Severity.Error;
}
