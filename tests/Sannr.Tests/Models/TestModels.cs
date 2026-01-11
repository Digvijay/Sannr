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
using System.Linq;

namespace Sannr.Tests.Models;

/// <summary>
/// Test model for Sannr validation tests.
/// </summary>
public partial class UserProfile
{
    [Required]
    [Sanitize(Trim = true, ToUpper = true)]
    public string? Username { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Years Old")]
    [Range(18, 100)]
    public int Age { get; set; }

    public string? Country { get; set; }

    [RequiredIf("Country", "USA")]
    public string? ZipCode { get; set; }

    [Required(Group = "Reg")]
    public string? ReferralCode { get; set; }
}

/// <summary>
/// Advanced test model for testing regex pre-compilation and performance optimizations.
/// </summary>
public partial class AdvancedValidationModel
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [CreditCard]
    public string? CreditCardNumber { get; set; }

    [Url]
    public string? Website { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [FileExtensions(Extensions = "pdf,doc,docx")]
    public string? DocumentPath { get; set; }

    [Range(1000, 9999)]
    public int SecurityCode { get; set; }

    [CustomValidator(typeof(AdvancedValidationModel), "ValidatePasswordStrength")]
    public string? Password { get; set; }

    [RequiredIf("Country", "US")]
    public string? ZipCode { get; set; }

    public string? Country { get; set; }

    // Custom validator method
    public static ValidationResult ValidatePasswordStrength(string? password, IServiceProvider? serviceProvider)
    {
        var result = new ValidationResult();
        if (string.IsNullOrEmpty(password))
        {
            result.Add("Password", "Password is required", Severity.Error);
            return result;
        }

        if (password.Length < 8)
        {
            result.Add("Password", "Password must be at least 8 characters long", Severity.Error);
        }

        if (!password.Any(char.IsUpper))
        {
            result.Add("Password", "Password must contain at least one uppercase letter", Severity.Warning);
        }

        if (!password.Any(char.IsLower))
        {
            result.Add("Password", "Password must contain at least one lowercase letter", Severity.Warning);
        }

        if (!password.Any(char.IsDigit))
        {
            result.Add("Password", "Password must contain at least one number", Severity.Warning);
        }

        return result;
    }
}

/// <summary>
/// Test model for client-side validation generation.
/// </summary>
[GenerateClientValidators(Language = ClientValidationLanguage.TypeScript)]
public partial class ClientValidationModel
{
    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }

    [RequiredIf("HasAddress", true)]
    public string? StreetAddress { get; set; }

    public bool HasAddress { get; set; }
}
