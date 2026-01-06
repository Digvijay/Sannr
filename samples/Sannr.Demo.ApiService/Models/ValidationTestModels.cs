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

using Sannr;

namespace Sannr.Demo.ApiService.Models;

/// <summary>
/// Test model demonstrating comprehensive Sannr validation attributes.
/// This model showcases all major validation features in a single endpoint.
/// </summary>
public partial class ValidationTestModel
{
    /// <summary>Required field with custom error message.</summary>
    [Required(ErrorMessage = "Username is mandatory.")]
    public string? Username { get; set; }

    /// <summary>String with both minimum (3) and maximum (10) length constraints.</summary>
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Must be between 3 and 10 chars.")]
    public string? DisplayName { get; set; }

    /// <summary>Integer range validation (18-99) with custom error message.</summary>
    [Range(18, 99, ErrorMessage = "Age must be valid.")]
    public int? Age { get; set; }

    /// <summary>Double range validation with decimal bounds (0.01-1000.00).</summary>
    [Range(0.01, 1000.00)]
    public double? Price { get; set; }

    /// <summary>Email format validation using regex pattern.</summary>
    [EmailAddress]
    public string? ContactEmail { get; set; }

    /// <summary>Credit card number format validation (13-19 digits with optional dashes/spaces).</summary>
    [CreditCard]
    public string? PaymentCard { get; set; }

    /// <summary>URL validation requiring http:// or https:// protocol.</summary>
    [Url]
    public string? PortfolioUrl { get; set; }

    /// <summary>Phone number validation allowing digits, spaces, dashes, plus signs, and parentheses.</summary>
    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>File extension validation - only allows .pdf, .docx, or .txt extensions.</summary>
    [FileExtensions(Extensions = "pdf,docx,txt")]
    public string? ResumeFileName { get; set; }

    /// <summary>No validation - used as condition for RequiredIf on State property.</summary>
    public string? Country { get; set; }

    /// <summary>Conditionally required when Country equals "USA".</summary>
    [RequiredIf(nameof(Country), "USA")]
    public string? State { get; set; }

    /// <summary>Sanitization attribute - trims whitespace and converts to uppercase.</summary>
    [Sanitize(Trim = true, ToUpper = true)]
    public string? UserId { get; set; }

    /// <summary>Display name override for error messages, combined with range validation.</summary>
    [Display(Name = "User's Age")]
    [Range(18, 99)]
    public int? DisplayedAge { get; set; }
}

/// <summary>
/// User profile model with sanitization and conditional validation.
/// </summary>
public partial class UserProfileModel
{
    [Required]
    [Sanitize(Trim = true, ToUpper = true)]
    public string? Username { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "Years Old")]
    [Range(18, 100)]
    public int? Age { get; set; }

    public string? Country { get; set; }

    [RequiredIf("Country", "USA")]
    public string? ZipCode { get; set; }

    [Required(Group = "Reg")]
    public string? ReferralCode { get; set; }
}

/// <summary>
/// Advanced validation model with custom validators and complex rules.
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
    public int? SecurityCode { get; set; }

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
            result.Add("", "Password is required", Severity.Error);
            return result;
        }

        if (password.Length < 8)
        {
            result.Add("", "Password must be at least 8 characters long", Severity.Error);
        }

        if (!password.Any(char.IsUpper))
        {
            result.Add("", "Password must contain at least one uppercase letter", Severity.Warning);
        }

        if (!password.Any(char.IsLower))
        {
            result.Add("", "Password must contain at least one lowercase letter", Severity.Warning);
        }

        if (!password.Any(char.IsDigit))
        {
            result.Add("", "Password must contain at least one number", Severity.Warning);
        }

        return result;
    }
}

/// <summary>
/// Order model demonstrating business rule validations.
/// </summary>
public partial class OrderModel
{
    [Required]
    public string? CustomerId { get; set; }

    [FutureDate]
    public DateTime? DeliveryDate { get; set; }

    [AllowedValues("USD", "EUR", "GBP")]
    public string? Currency { get; set; }

    [ConditionalRange("Currency", "USD", 1, 1000)]
    public decimal? Amount { get; set; }
}

/// <summary>
/// Product model with business rules.
/// </summary>
public partial class ProductModel
{
    [Required]
    public string? Name { get; set; }

    [AllowedValues("active", "inactive", "discontinued", "pending")]
    public string? Status { get; set; }

    [FutureDate]
    public DateTime? ReleaseDate { get; set; }

    [ConditionalRange("Status", "active", 0.01, 999999.99)]
    public decimal? Price { get; set; }
}

/// <summary>
/// Appointment model with time-based validations.
/// </summary>
public partial class AppointmentModel
{
    [Required]
    public string? CustomerName { get; set; }

    [FutureDate]
    public DateTime? AppointmentDate { get; set; }

    [AllowedValues("pending", "confirmed", "cancelled", "completed")]
    public string? Status { get; set; }

    [ConditionalRange("Status", "confirmed", 30, 480)] // 30 minutes to 8 hours
    public int? DurationMinutes { get; set; }
}

/// <summary>
/// Client-side validation model for frontend integration.
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
    public int? Age { get; set; }

    [RequiredIf("HasAddress", true)]
    public string? StreetAddress { get; set; }

    public bool HasAddress { get; set; }
}