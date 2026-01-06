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

using System.Linq.Expressions;

namespace Sannr;

/// <summary>
/// Base class for compile-time fluent validation configurations.
/// This provides a FluentValidation-style API that gets analyzed by the source generator
/// to produce optimized validation code at compile time.
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
public abstract class ValidatorConfig<T>
{
    private readonly List<RuleBuilder<T, object>> _ruleBuilders = new();

    /// <summary>
    /// Configures the validation rules for type T.
    /// This method is called by the source generator during compilation.
    /// </summary>
    public abstract void Configure();

    /// <summary>
    /// Starts building a validation rule for the specified property.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property being validated</typeparam>
    /// <param name="expression">Expression selecting the property to validate</param>
    /// <returns>A rule builder for fluent configuration</returns>
    protected IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var builder = new RuleBuilder<T, TProperty>(expression);
        _ruleBuilders.Add((RuleBuilder<T, object>)(object)builder);
        return builder;
    }

    /// <summary>
    /// Gets all configured rule builders for source generator analysis.
    /// </summary>
    internal IReadOnlyList<RuleBuilder<T, object>> RuleBuilders => _ruleBuilders;
}

/// <summary>
/// Interface for building validation rules in a fluent manner.
/// Used by the source generator to analyze rule configurations.
/// </summary>
/// <typeparam name="T">The type being validated</typeparam>
/// <typeparam name="TProperty">The property type being validated</typeparam>
public interface IRuleBuilder<T, TProperty>
{
    /// <summary>
    /// Marks the property as required (not null/empty).
    /// </summary>
    IRuleBuilder<T, TProperty> NotEmpty();

    /// <summary>
    /// Specifies the minimum and maximum length for string properties.
    /// </summary>
    IRuleBuilder<T, TProperty> Length(int min, int max);

    /// <summary>
    /// Validates that the string is a valid email address.
    /// </summary>
    IRuleBuilder<T, TProperty> Email();

    /// <summary>
    /// Validates that the value is within the specified range.
    /// </summary>
    IRuleBuilder<T, TProperty> InclusiveBetween(TProperty min, TProperty max);

    /// <summary>
    /// Adds a custom validation predicate.
    /// </summary>
    IRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate);

    /// <summary>
    /// Adds an async custom validation predicate.
    /// </summary>
    IRuleBuilder<T, TProperty> MustAsync(Func<TProperty, Task<bool>> predicate);

    /// <summary>
    /// Specifies a custom error message for this rule.
    /// </summary>
    IRuleBuilder<T, TProperty> WithMessage(string message);

    /// <summary>
    /// Applies this rule only when the condition is met.
    /// </summary>
    IRuleBuilder<T, TProperty> When(Func<T, bool> condition);

    /// <summary>
    /// Makes this property required only when the condition is met.
    /// </summary>
    IRuleBuilder<T, TProperty> RequiredIf(Func<T, bool> condition);
}

/// <summary>
/// Internal implementation of the rule builder.
/// This class captures rule configurations for source generator analysis.
/// </summary>
internal class RuleBuilder<T, TProperty> : IRuleBuilder<T, TProperty>
{
    public Expression<Func<T, TProperty>> PropertySelector { get; }
    public List<ValidationRule> Rules { get; } = new();

    public RuleBuilder(Expression<Func<T, TProperty>> propertySelector)
    {
        PropertySelector = propertySelector;
    }

    public IRuleBuilder<T, TProperty> NotEmpty() =>
        AddRule(new NotEmptyRule());

    public IRuleBuilder<T, TProperty> Length(int min, int max) =>
        AddRule(new LengthRule(min, max));

    public IRuleBuilder<T, TProperty> Email() =>
        AddRule(new EmailRule());

    public IRuleBuilder<T, TProperty> InclusiveBetween(TProperty min, TProperty max) =>
        AddRule(new RangeRule(min, max));

    public IRuleBuilder<T, TProperty> Must(Func<TProperty, bool> predicate) =>
        AddRule(new PredicateRule(predicate));

    public IRuleBuilder<T, TProperty> MustAsync(Func<TProperty, Task<bool>> predicate) =>
        AddRule(new AsyncPredicateRule(predicate));

    public IRuleBuilder<T, TProperty> WithMessage(string message)
    {
        if (Rules.Count > 0)
            Rules[^1].ErrorMessage = message;
        return this;
    }

    public IRuleBuilder<T, TProperty> When(Func<T, bool> condition) =>
        AddRule(new ConditionalRule(condition));

    public IRuleBuilder<T, TProperty> RequiredIf(Func<T, bool> condition)
    {
        Rules.Add(new RequiredIfRule(condition));
        return NotEmpty();
    }

    private RuleBuilder<T, TProperty> AddRule(ValidationRule rule)
    {
        Rules.Add(rule);
        return this;
    }
}

/// <summary>
/// Base class for validation rules.
/// </summary>
internal abstract class ValidationRule
{
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Rule for non-empty validation.
/// </summary>
internal class NotEmptyRule : ValidationRule { }

/// <summary>
/// Rule for length validation.
/// </summary>
internal class LengthRule : ValidationRule
{
    public int MinLength { get; }
    public int MaxLength { get; }

    public LengthRule(int min, int max)
    {
        MinLength = min;
        MaxLength = max;
    }
}

/// <summary>
/// Rule for email validation.
/// </summary>
internal class EmailRule : ValidationRule { }

/// <summary>
/// Rule for range validation.
/// </summary>
internal class RangeRule : ValidationRule
{
    public object? Min { get; }
    public object? Max { get; }

    public RangeRule(object? min, object? max)
    {
        Min = min;
        Max = max;
    }
}

/// <summary>
/// Rule for custom predicate validation.
/// </summary>
internal class PredicateRule : ValidationRule
{
    public Delegate Predicate { get; }

    public PredicateRule(Delegate predicate)
    {
        Predicate = predicate;
    }
}

/// <summary>
/// Rule for async custom predicate validation.
/// </summary>
internal class AsyncPredicateRule : ValidationRule
{
    public Delegate Predicate { get; }

    public AsyncPredicateRule(Delegate predicate)
    {
        Predicate = predicate;
    }
}

/// <summary>
/// Rule for conditional validation.
/// </summary>
internal class ConditionalRule : ValidationRule
{
    public Delegate Condition { get; }

    public ConditionalRule(Delegate condition)
    {
        Condition = condition;
    }
}

/// <summary>
/// Rule for conditional required validation.
/// </summary>
internal class RequiredIfRule : ValidationRule
{
    public Delegate Condition { get; }

    public RequiredIfRule(Delegate condition)
    {
        Condition = condition;
    }
}
