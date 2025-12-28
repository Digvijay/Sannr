using Xunit;
using Sannr;
using System.Threading.Tasks;

namespace Sannr.Tests;

/// <summary>
/// Test model demonstrating various validation attributes supported by Sannr.
/// This model is used across all attribute validation tests to verify the source generator
/// correctly emits validation logic for different attribute types and combinations.
/// </summary>
public class TestModel
{
    /// <summary>Required field with custom error message.</summary>
    [Required(ErrorMessage = "Username is mandatory.")]
    public string? Username { get; set; }

    /// <summary>String with both minimum (3) and maximum (10) length constraints.</summary>
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Must be between 3 and 10 chars.")]
    public string? DisplayName { get; set; }

    /// <summary>Integer range validation (18-99) with custom error message.</summary>
    [Range(18, 99, ErrorMessage = "Age must be valid.")]
    public int Age { get; set; }

    /// <summary>Double range validation with decimal bounds (0.01-1000.00).</summary>
    [Range(0.01, 1000.00)]
    public double Price { get; set; }

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
    public int DisplayedAge { get; set; }
}

/// <summary>
/// Comprehensive test suite for Sannr attribute-based validation.
/// Tests verify that the source generator correctly emits validation logic for all supported
/// validation attributes and their combinations, including error messages, custom messages,
/// and edge cases.
/// </summary>
public class AttributeValidationTests
{
    /// <summary>
    /// Verifies that [Required] attribute correctly validates null and whitespace-only strings.
    /// Tests both null values and strings containing only whitespace characters.
    /// </summary>
    [Fact]
    public async Task Required_Should_Fail_On_Null_Or_Whitespace()
    {
        // Test 1: Null value should fail validation
        var model = new TestModel { Username = null };
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("mandatory"));
        
        // Test 2: Whitespace-only string should also fail validation
        model.Username = "   ";
        res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("mandatory"));
    }

    /// <summary>
    /// Verifies that [StringLength] attribute enforces both minimum and maximum length constraints.
    /// Tests strings that are too short, too long, and within the valid range (3-10 characters).
    /// </summary>
    [Fact]
    public async Task StringLength_Should_Enforce_Bounds()
    {
        // Test 1: String too short (2 chars, min is 3)
        var model = CreateValidModel();
        model.DisplayName = "AB";
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("between 3 and 10") || e.Message.Contains("must be a string with a maximum length of 10"));
        
        // Test 2: String too long (11 chars, max is 10)
        model = CreateValidModel();
        model.DisplayName = "ABCDEFGHIJK";
        res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("between 3 and 10") || e.Message.Contains("must be a string with a maximum length of 10"));
        
        // Test 3: Valid string within bounds (9 chars)
        model = CreateValidModel();
        model.DisplayName = "ValidName";
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [Range] attribute enforces bounds for integer values.
    /// Tests values below minimum, above maximum, and at the minimum boundary (18-99).
    /// Also verifies custom error message is used when specified.
    /// </summary>
    [Fact]
    public async Task Range_Should_Enforce_Bounds()
    {
        // Test 1: Value below minimum (17 < 18)
        var model = CreateValidModel();
        model.Age = 17;
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("Age must be valid"));
        
        // Test 2: Value above maximum (100 > 99)
        model = CreateValidModel();
        model.Age = 100;
        res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("Age must be valid"));
        
        // Test 3: Value at minimum boundary (18 is valid)
        model = CreateValidModel();
        model.Age = 18;
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [Range] attribute correctly handles floating-point (double) values.
    /// Tests decimal boundaries and ensures proper comparison with fractional values (0.01-1000.00).
    /// </summary>
    [Fact]
    public async Task Range_Double_Should_Enforce_Bounds()
    {
        // Test 1: Value below minimum (0.0 < 0.01)
        var model = CreateValidModel();
        model.Price = 0.0;
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("must be between 0.01 and 1000"));
        
        // Test 2: Value at minimum boundary (0.01 is valid)
        model = CreateValidModel();
        model.Price = 0.01;
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [EmailAddress] attribute validates email format using regex pattern.
    /// Tests invalid format (missing @ or domain) and valid email address format.
    /// </summary>
    [Fact]
    public async Task EmailAddress_Should_Validate_Format()
    {
        // Test 1: Invalid email format (no @ or domain)
        var model = CreateValidModel();
        model.ContactEmail = "not-an-email";
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("not a valid e-mail address"));
        
        // Test 2: Valid email format
        model = CreateValidModel();
        model.ContactEmail = "user@example.com";
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [CreditCard] attribute validates credit card number format.
    /// Tests too-short number and valid 16-digit card number with dashes.
    /// Pattern allows 13-19 digits with optional dashes and spaces.
    /// </summary>
    [Fact]
    public async Task CreditCard_Should_Validate_Format()
    {
        // Test 1: Invalid card number (too short)
        var model = CreateValidModel();
        model.PaymentCard = "1234";
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("not a valid credit card number"));
        
        // Test 2: Valid card number with dashes (16 digits)
        model = CreateValidModel();
        model.PaymentCard = "4111-1111-1111-1111";
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [Url] attribute validates URL format requiring http:// or https:// protocol.
    /// Tests unsupported protocol (ftp) and valid HTTPS URL.
    /// </summary>
    [Fact]
    public async Task Url_Should_Validate_Format()
    {
        // Test 1: Invalid protocol (ftp not supported, only http/https)
        var model = CreateValidModel();
        model.PortfolioUrl = "ftp://example.com";
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("not a valid"));
        
        // Test 2: Valid HTTPS URL
        model = CreateValidModel();
        model.PortfolioUrl = "https://example.com";
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [Phone] attribute validates phone number format.
    /// Tests invalid format (letters) and valid phone with country code, parentheses, and dashes.
    /// Pattern allows digits, spaces, dashes, plus signs, and parentheses.
    /// </summary>
    [Fact]
    public async Task Phone_Should_Validate_Format()
    {
        // Test 1: Invalid format (contains letters)
        var model = CreateValidModel();
        model.PhoneNumber = "not-a-phone";
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("not a valid"));
        
        // Test 2: Valid phone number with international format
        model = CreateValidModel();
        model.PhoneNumber = "+1 (555) 123-4567";
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [FileExtensions] attribute validates file extensions against allowed list.
    /// Tests disallowed extension (.exe) and allowed extension (.pdf).
    /// Configured to accept only: .pdf, .docx, .txt
    /// </summary>
    [Fact]
    public async Task FileExtensions_Should_Validate()
    {
        // Test 1: Disallowed extension (.exe not in allowed list)
        var model = CreateValidModel();
        model.ResumeFileName = "resume.exe";
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.Message.Contains("must have one of the following extensions"));
        
        // Test 2: Allowed extension (.pdf is in allowed list)
        model = CreateValidModel();
        model.ResumeFileName = "resume.pdf";
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [RequiredIf] attribute enforces conditional requirement based on another property.
    /// State is required only when Country equals "USA".
    /// Tests when condition is met (USA + null State) and when condition is not met (Canada).
    /// </summary>
    [Fact]
    public async Task RequiredIf_Should_Enforce_Conditional()
    {
        // Test 1: Condition met (Country=USA) but State is null - should fail
        var model = CreateValidModel();
        model.Country = "USA";
        model.State = null;
        var res = await Validate(model);
        Assert.Contains(res.Errors, e => e.MemberName == "State");
        
        // Test 2: Condition not met (Country=Canada) - State not required, should pass
        model = CreateValidModel();
        model.Country = "Canada";
        res = await Validate(model);
        Assert.True(res.IsValid);
    }

    /// <summary>
    /// Verifies that [Sanitize] attribute modifies property values before validation.
    /// Tests Trim=true (removes leading/trailing whitespace) and ToUpper=true (converts to uppercase).
    /// Sanitization happens during validation, modifying the original model.
    /// </summary>
    [Fact]
    public async Task Sanitize_Should_Trim_And_Uppercase()
    {
        // Input has leading/trailing spaces and mixed case
        var model = new TestModel { UserId = "  john.doe  " };
        var res = await Validate(model);
        
        // After validation, value should be trimmed and uppercased
        Assert.Equal("JOHN.DOE", model.UserId);
    }

    /// <summary>
    /// Verifies that [Display] attribute overrides property name in error messages.
    /// DisplayedAge property uses "User's Age" as display name in validation messages.
    /// Tests that error message contains the friendly name instead of property name.
    /// </summary>
    [Fact]
    public async Task Display_Should_Use_Friendly_Name()
    {
        // Set DisplayedAge to invalid value (10 < minimum 18)
        var model = new TestModel { DisplayedAge = 10 };
        var res = await Validate(model);
        
        // Error message should use "User's Age" not "DisplayedAge"
        Assert.Contains(res.Errors, e => e.Message.Contains("User's Age"));
    }

    /// <summary>
    /// Helper method to invoke Sannr validation on a model instance.
    /// Retrieves the generated validator from the registry and executes validation.
    /// </summary>
    /// <param name="model">The model instance to validate.</param>
    /// <returns>ValidationResult containing any errors found.</returns>

    private async Task<ValidationResult> Validate(object model)
    {
        SannrValidatorRegistry.TryGetValidator(model.GetType(), out var val);
        return await val!(new SannrValidationContext(model));
    }

    /// <summary>
    /// Creates a fully valid TestModel instance with all required fields populated.
    /// Used as a baseline in tests to isolate validation of specific properties.
    /// Optional fields are left as null.
    /// </summary>
    /// <returns>A TestModel instance that passes all validation rules.</returns>
    private TestModel CreateValidModel()
    {
        return new TestModel
        {
            Username = "testuser",
            DisplayName = "Valid",
            Age = 25,
            Price = 100.0,
            ContactEmail = null, // Optional
            PaymentCard = null, // Optional
            PortfolioUrl = null, // Optional
            PhoneNumber = null, // Optional
            ResumeFileName = null, // Optional
            Country = null, // Optional
            State = null, // Optional
            UserId = null, // Optional
            DisplayedAge = 25
        };
    }
}
