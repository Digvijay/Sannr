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
/// Represents a validation error for a specific member.
/// </summary>
/// <param name="MemberName">The name of the member that failed validation.</param>
/// <param name="Message">The validation error message.</param>
/// <param name="Severity">The severity of the validation error.</param>
public record ValidationError(string MemberName, string Message, Severity Severity);


/// <summary>
/// Represents the result of a validation operation, including any validation errors.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation result is valid (no errors of severity Error).
    /// </summary>
    public bool IsValid => !Errors.Any(e => e.Severity == Severity.Error);

    /// <summary>
    /// Gets the list of validation errors.
    /// </summary>
    public List<ValidationError> Errors { get; } = new();

    /// <summary>
    /// Returns a successful <see cref="ValidationResult"/> instance with no errors.
    /// </summary>
    public static ValidationResult Success() => new();

    /// <summary>
    /// Adds a validation error to the result.
    /// </summary>
    /// <param name="member">The name of the member that failed validation.</param>
    /// <param name="message">The validation error message.</param>
    /// <param name="severity">The severity of the validation error.</param>
    public void Add(string member, string message, Severity severity = Severity.Error)
        => Errors.Add(new ValidationError(member, message, severity));

    /// <summary>
    /// Merges errors from another <see cref="ValidationResult"/>, prefixing member names.
    /// </summary>
    /// <param name="other">The other validation result to merge.</param>
    /// <param name="prefix">The prefix to add to each member name.</param>
    public void Merge(ValidationResult other, string prefix)
    {
        foreach (var err in other.Errors)
            Errors.Add(new ValidationError($"{prefix}.{err.MemberName}", err.Message, err.Severity));
    }
}
