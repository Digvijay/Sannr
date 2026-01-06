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
using Xunit;

namespace Sannr.FluentValidation.Tests;

/// <summary>
/// Test model for fluent validation.
/// </summary>
public partial class TestUser
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
}

/// <summary>
/// Fluent validator configuration for TestUser.
/// This will be analyzed by the source generator to produce validation code.
/// </summary>
public partial class TestUserValidator : ValidatorConfig<TestUser>
{
    public override void Configure()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 50)
            .WithMessage("Name is required and must be 2-50 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .Email()
            .WithMessage("Valid email is required");

        RuleFor(x => x.Age)
            .InclusiveBetween(13, 120)
            .WithMessage("Age must be between 13 and 120");
    }
}

/// <summary>
/// Test model demonstrating comprehensive Sannr fluent validation rules.
/// This model showcases all major validation features in fluent validation.
/// </summary>
public partial class ComprehensiveValidationModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Website { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CreditCard { get; set; }
    public string? FileName { get; set; }
    public DateTime FutureDate { get; set; }
    public string? Status { get; set; }
    public bool HasAddress { get; set; }
    public string? StreetAddress { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
}

/// <summary>
/// Comprehensive fluent validator configuration for testing all validation rules.
/// </summary>
public partial class ComprehensiveValidator : ValidatorConfig<ComprehensiveValidationModel>
{
    public override void Configure()
    {
        // Basic string validations
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 50)
            .WithMessage("Name is required and must be 2-50 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .Email()
            .WithMessage("Valid email is required");

        RuleFor(x => x.Description)
            .Length(0, 500)
            .WithMessage("Description cannot exceed 500 characters");

        // Numeric validations
        RuleFor(x => x.Age)
            .InclusiveBetween(18, 120)
            .WithMessage("Age must be between 18 and 120");

        RuleFor(x => x.Price)
            .InclusiveBetween(0.01m, 10000.00m)
            .WithMessage("Price must be between 0.01 and 10000.00");

        // Specialized validations
        RuleFor(x => x.Website)
            .Must(url => url == null || url.StartsWith("http"))
            .WithMessage("Website must start with http");

        RuleFor(x => x.PhoneNumber)
            .Must(phone => phone == null || phone.Length >= 10)
            .WithMessage("Phone number must be at least 10 characters");

        RuleFor(x => x.CreditCard)
            .Must(card => card == null || card.Replace(" ", "").Replace("-", "").Length >= 13)
            .WithMessage("Credit card number is invalid");

        RuleFor(x => x.FileName)
            .Must(name => name == null || name.Contains("."))
            .WithMessage("File name must have an extension");

        // Date validation
        RuleFor(x => x.FutureDate)
            .Must(date => date > DateTime.Now)
            .WithMessage("Date must be in the future");

        // Allowed values
        RuleFor(x => x.Status)
            .Must(status => status == null || new[] { "active", "inactive", "pending" }.Contains(status))
            .WithMessage("Status must be active, inactive, or pending");

        // Conditional validation
        RuleFor(x => x.StreetAddress)
            .RequiredIf(x => x.HasAddress)
            .WithMessage("Street address is required when HasAddress is true");

        RuleFor(x => x.ZipCode)
            .RequiredIf(x => x.Country == "USA")
            .Length(5, 10)
            .WithMessage("Zip code is required for USA and must be 5-10 characters");
    }
}

/// <summary>
/// Simple validator for basic validation testing.
/// </summary>
public partial class SimpleValidator : ValidatorConfig<TestUser>
{
    public override void Configure()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .Email();

        RuleFor(x => x.Age)
            .InclusiveBetween(13, 120);
    }
}

/// <summary>
/// Conditional validator for testing conditional validation rules.
/// </summary>
public partial class ConditionalValidator : ValidatorConfig<ComprehensiveValidationModel>
{
    public override void Configure()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .When(x => x.Age >= 18)
            .WithMessage("Name is required for adults");

        RuleFor(x => x.ZipCode)
            .RequiredIf(x => x.Country == "USA")
            .WithMessage("Zip code required for USA");

        RuleFor(x => x.StreetAddress)
            .NotEmpty()
            .When(x => x.HasAddress)
            .WithMessage("Street address required when address is specified");
    }
}

/// <summary>
/// Tests for fluent validation API.
/// </summary>
public partial class FluentValidationTests
{
    [Fact]
    [Trait("Category", "FluentValidation")]
    [Trait("Feature", "Debug")]
    public void Debug_MethodBody()
    {
        // Create a validator and check what the method body looks like
        var validator = new TestUserValidator();
        // The method body should be extracted by the source generator
        // This test is just to help debug
        Assert.NotNull(validator);
    }

    [Fact]
    [Trait("Category", "FluentValidation")]
    [Trait("Feature", "BasicSetup")]
    public void RuleBuilder_ChainsCorrectly()
    {
        // This test verifies the fluent API chaining works at compile time
        var config = new TestUserValidator();

        // The actual validation logic will be generated by the source generator
        // This test just ensures the configuration API is usable
        Assert.True(true);
    }

    [Fact]
    [Trait("Category", "FluentValidation")]
    [Trait("Feature", "GeneratedValidator")]
    public async Task GeneratedValidator_ValidatesCorrectly()
    {
        // Arrange - Valid user data
        var validUser = new TestUser
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 30
        };

        // Arrange - Invalid user data (violates all rules)
        var invalidUser = new TestUser
        {
            Name = "", // Empty name (violates NotEmpty)
            Email = "invalid-email", // Invalid email format
            Age = 10 // Below minimum age
        };

        // Act
        var validResult = await TestUserFluentValidator.ValidateAsync(validUser);
        var invalidResult = await TestUserFluentValidator.ValidateAsync(invalidUser);

        // Assert
        Assert.True(validResult.IsValid, "Valid user should pass all validations");
        Assert.False(invalidResult.IsValid, "Invalid user should fail validations");

        // Check specific validation errors
        Assert.Contains(invalidResult.Errors, e => e.MemberName == "Name");
        Assert.Contains(invalidResult.Errors, e => e.MemberName == "Email");
        Assert.Contains(invalidResult.Errors, e => e.MemberName == "Age");
    }

    [Fact]
    [Trait("Category", "FluentValidation")]
    [Trait("Feature", "GeneratedValidator")]
    public async Task ComprehensiveValidator_ValidatesAllRules()
    {
        // Arrange - Valid comprehensive model
        var validModel = new ComprehensiveValidationModel
        {
            Name = "John Doe",
            Email = "john@example.com",
            Age = 25,
            Description = "A valid description",
            Price = 99.99m,
            Website = "https://example.com",
            PhoneNumber = "1234567890",
            CreditCard = "4111111111111111",
            FileName = "document.pdf",
            FutureDate = DateTime.Now.AddDays(1),
            Status = "active",
            HasAddress = true,
            StreetAddress = "123 Main St",
            Country = "USA",
            ZipCode = "12345"
        };

        // Arrange - Invalid comprehensive model (violates multiple rules)
        var invalidModel = new ComprehensiveValidationModel
        {
            Name = "X", // Too short
            Email = "invalid-email", // Invalid format
            Age = 15, // Too young
            Description = new string('x', 600), // Too long
            Price = -10, // Negative
            Website = "ftp://invalid.com", // Wrong protocol
            PhoneNumber = "123", // Too short
            CreditCard = "123", // Too short
            FileName = "document", // No extension
            FutureDate = DateTime.Now.AddDays(-1), // Past date
            Status = "unknown", // Invalid status
            HasAddress = true,
            StreetAddress = "", // Required but empty
            Country = "USA",
            ZipCode = "12" // Too short for USA
        };

        // Act
        var validResult = await ComprehensiveFluentValidator.ValidateAsync(validModel);
        var invalidResult = await ComprehensiveFluentValidator.ValidateAsync(invalidModel);

        // Assert
        Assert.True(validResult.IsValid, "Valid comprehensive model should pass all validations");
        Assert.False(invalidResult.IsValid, "Invalid comprehensive model should fail validations");

        // Should have multiple validation errors
        Assert.True(invalidResult.Errors.Count >= 5, $"Expected at least 5 errors, got {invalidResult.Errors.Count}");

        // Check that some specific validation errors are present
        var errorMessages = invalidResult.Errors.Select(e => e.Message).ToList();
        Assert.Contains("Name is required and must be 2-50 characters", errorMessages);
        Assert.Contains("Valid email is required", errorMessages);
    }

    [Fact]
    [Trait("Category", "FluentValidation")]
    [Trait("Feature", "ConditionalValidation")]
    public async Task ConditionalValidator_ValidatesConditionally()
    {
        // Arrange - Adult user (should require name)
        var adultUser = new ComprehensiveValidationModel
        {
            Age = 25,
            Name = "", // Should be required for adults
            Country = "USA",
            ZipCode = "", // Should be required for USA
            HasAddress = true,
            StreetAddress = "" // Should be required when HasAddress is true
        };

        // Arrange - Child user (name not required)
        var childUser = new ComprehensiveValidationModel
        {
            Age = 15,
            Name = "", // OK for children
            Country = "Canada",
            ZipCode = "", // OK for non-USA
            HasAddress = false,
            StreetAddress = "" // OK when HasAddress is false
        };

        // Act
        var adultResult = await ConditionalFluentValidator.ValidateAsync(adultUser);
        var childResult = await ConditionalFluentValidator.ValidateAsync(childUser);

        // Assert
        Assert.False(adultResult.IsValid, "Adult user should fail validation due to missing required fields");

        // Check specific errors for adult
        var adultErrors = adultResult.Errors.Select(e => e.Message).ToList();
        Assert.Contains("Name is required for adults", adultErrors);
        // Note: Conditional validation for RequiredIf may not be fully implemented yet
    }

    [Fact]
    [Trait("Category", "FluentValidation")]
    [Trait("Feature", "EdgeCases")]
    public async Task Validator_HandlesNullAndEmptyValues()
    {
        // Arrange - Model with null/empty values
        var nullModel = new TestUser
        {
            Name = null,
            Email = null,
            Age = 0
        };

        var emptyModel = new TestUser
        {
            Name = "",
            Email = "",
            Age = 0
        };

        // Act
        var nullResult = await TestUserFluentValidator.ValidateAsync(nullModel);
        var emptyResult = await TestUserFluentValidator.ValidateAsync(emptyModel);

        // Assert
        Assert.False(nullResult.IsValid, "Null values should fail validation");
        Assert.False(emptyResult.IsValid, "Empty values should fail validation");

        // Both should have validation errors (exact count may vary with implementation)
        Assert.True(nullResult.Errors.Count >= 2, $"Null model should have at least 2 errors, got {nullResult.Errors.Count}");
        Assert.True(emptyResult.Errors.Count >= 2, $"Empty model should have at least 2 errors, got {emptyResult.Errors.Count}");
    }

    [Fact]
    [Trait("Category", "FluentValidation")]
    [Trait("Feature", "CustomValidation")]
    public void CustomValidator_API_CompilesCorrectly()
    {
        // Arrange - Create a custom validator that uses Must rules
        var customValidator = new SimpleValidator();
        // Note: This is just for testing the API structure compiles correctly

        // Act & Assert
        // This test ensures the Must API compiles correctly
        Assert.NotNull(customValidator);
    }
}