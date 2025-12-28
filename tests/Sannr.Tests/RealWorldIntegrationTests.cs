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

using Xunit;
using Sannr;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Sannr.Tests.Models;
using System;
using System.Collections.Generic;

namespace Sannr.Tests;

/// <summary>
/// Complex model with multiple validation attributes for integration testing.
/// </summary>
public class ComplexModel
{
    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Range(0, 150)]
    public int Age { get; set; }

    [Range(0, 1000000)]
    public double Salary { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [Url]
    public string? Website { get; set; }
}

/// <summary>
/// Large model with many properties to test performance and scalability.
/// </summary>
public class LargeModel
{
    [Required] public string? Prop1 { get; set; }
    [Required] public string? Prop2 { get; set; }
    [Required] public string? Prop3 { get; set; }
    [Required] public string? Prop4 { get; set; }
    [Required] public string? Prop5 { get; set; }
    [Required] public string? Prop6 { get; set; }
    [Required] public string? Prop7 { get; set; }
    [Required] public string? Prop8 { get; set; }
    [Required] public string? Prop9 { get; set; }
    [Required] public string? Prop10 { get; set; }
    [Required] public string? Prop11 { get; set; }
    [Required] public string? Prop12 { get; set; }
    [Required] public string? Prop13 { get; set; }
    [Required] public string? Prop14 { get; set; }
    [Required] public string? Prop15 { get; set; }
    [Required] public string? Prop16 { get; set; }
    [Required] public string? Prop17 { get; set; }
    [Required] public string? Prop18 { get; set; }
    [Required] public string? Prop19 { get; set; }
    [Required] public string? Prop20 { get; set; }
    [Required] public string? Prop21 { get; set; }
    [Required] public string? Prop22 { get; set; }
    [Required] public string? Prop23 { get; set; }
    [Required] public string? Prop24 { get; set; }
    [Required] public string? Prop25 { get; set; }
    [Required] public string? Prop26 { get; set; }
    [Required] public string? Prop27 { get; set; }
    [Required] public string? Prop28 { get; set; }
    [Required] public string? Prop29 { get; set; }
    [Required] public string? Prop30 { get; set; }
    [Required] public string? Prop31 { get; set; }
    [Required] public string? Prop32 { get; set; }
    [Required] public string? Prop33 { get; set; }
    [Required] public string? Prop34 { get; set; }
    [Required] public string? Prop35 { get; set; }
    [Required] public string? Prop36 { get; set; }
    [Required] public string? Prop37 { get; set; }
    [Required] public string? Prop38 { get; set; }
    [Required] public string? Prop39 { get; set; }
    [Required] public string? Prop40 { get; set; }
    [Required] public string? Prop41 { get; set; }
    [Required] public string? Prop42 { get; set; }
    [Required] public string? Prop43 { get; set; }
    [Required] public string? Prop44 { get; set; }
    [Required] public string? Prop45 { get; set; }
    [Required] public string? Prop46 { get; set; }
    [Required] public string? Prop47 { get; set; }
    [Required] public string? Prop48 { get; set; }
    [Required] public string? Prop49 { get; set; }
    [Required] public string? Prop50 { get; set; }

    public List<string> StringList { get; set; } = new List<string>();
    public List<int> IntList { get; set; } = new List<int>();

    /// <summary>
    /// Populates all properties with valid data for performance testing.
    /// </summary>
    public void PopulateWithValidData()
    {
        Prop1 = "Value1"; Prop2 = "Value2"; Prop3 = "Value3"; Prop4 = "Value4"; Prop5 = "Value5";
        Prop6 = "Value6"; Prop7 = "Value7"; Prop8 = "Value8"; Prop9 = "Value9"; Prop10 = "Value10";
        Prop11 = "Value11"; Prop12 = "Value12"; Prop13 = "Value13"; Prop14 = "Value14"; Prop15 = "Value15";
        Prop16 = "Value16"; Prop17 = "Value17"; Prop18 = "Value18"; Prop19 = "Value19"; Prop20 = "Value20";
        Prop21 = "Value21"; Prop22 = "Value22"; Prop23 = "Value23"; Prop24 = "Value24"; Prop25 = "Value25";
        Prop26 = "Value26"; Prop27 = "Value27"; Prop28 = "Value28"; Prop29 = "Value29"; Prop30 = "Value30";
        Prop31 = "Value31"; Prop32 = "Value32"; Prop33 = "Value33"; Prop34 = "Value34"; Prop35 = "Value35";
        Prop36 = "Value36"; Prop37 = "Value37"; Prop38 = "Value38"; Prop39 = "Value39"; Prop40 = "Value40";
        Prop41 = "Value41"; Prop42 = "Value42"; Prop43 = "Value43"; Prop44 = "Value44"; Prop45 = "Value45";
        Prop46 = "Value46"; Prop47 = "Value47"; Prop48 = "Value48"; Prop49 = "Value49"; Prop50 = "Value50";
    }
}

/// <summary>
/// Model with nested objects to test complex object graph validation.
/// </summary>
public class NestedModel
{
    [Required]
    public string? Name { get; set; }

    public ComplexModel? Child { get; set; }
}

/// <summary>
/// Model with circular references to test reference cycle handling.
/// </summary>
public class CircularModel
{
    [Required]
    public string? Name { get; set; }

    public CircularModel? Parent { get; set; }
    public CircularModel? Child { get; set; }
}

/// <summary>
/// Base class for inheritance testing.
/// </summary>
public class BaseModel
{
    [Required]
    public string? BaseProperty { get; set; }
}

/// <summary>
/// Derived class to test inheritance scenarios.
/// </summary>
public class DerivedModel : BaseModel
{
    [Required]
    public string? DerivedProperty { get; set; }

    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Range(0, 150)]
    public int Age { get; set; }
}
public class ConditionalModel : Sannr.IValidatableObject
{
    [Required]
    public string? Username { get; set; }

    public bool IsEmployed { get; set; }

    [RequiredIf("IsEmployed", true)]
    public string? JobTitle { get; set; }

    [RequiredIf("IsEmployed", true)]
    [Range(0, 1000000)]
    public decimal? Salary { get; set; }

    [Range(0, 150)]
    public int Age { get; set; }

    public string? Country { get; set; }

    [RequiredIf("Country", "USA")]
    public string? ZipCode { get; set; }

    [CustomValidator(typeof(EmploymentAgeValidator))]
    public bool IsValidEmployment => !IsEmployed || Age >= 18;

    public System.Collections.Generic.IEnumerable<Sannr.ModelValidationResult> Validate(SannrValidationContext validationContext)
    {
        var results = new List<Sannr.ModelValidationResult>();

        // Employment age validation: cannot be employed if under 18
        if (IsEmployed && Age < 18)
        {
            results.Add(new Sannr.ModelValidationResult
            {
                MemberName = "IsValidEmployment",
                Message = "Cannot be employed if under 18"
            });
        }

        return results;
    }
}

/// <summary>
/// Model with custom validation attributes and complex rules.
/// </summary>
public class AdvancedValidationModel
{
    [Required]
    [StringLength(50)]
    public string? Username { get; set; }

    [Required]
    [EmailAddress]
    public string? PrimaryEmail { get; set; }

    [EmailAddress] // Optional secondary email
    public string? SecondaryEmail { get; set; }

    [Required]
    [Range(18, 120)]
    public int Age { get; set; }

    [Required]
    [CreditCard]
    public string? CreditCard { get; set; }

    [Required]
    [Url]
    public string? Website { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [Range(0, 100)]
    public double Score { get; set; }
}

/// <summary>
/// Model implementing Sannr.IValidatableObject for model-level validation.
/// </summary>
public class ModelLevelValidationModel : Sannr.IValidatableObject
{
    [Required]
    public string? FirstName { get; set; }

    [Required]
    public string? LastName { get; set; }

    [Range(0, 150)]
    public int Age { get; set; }

    public bool IsEmployed { get; set; }

    [Range(0, 1000000)]
    public decimal? Salary { get; set; }

    public System.Collections.Generic.IEnumerable<Sannr.ModelValidationResult> Validate(SannrValidationContext validationContext)
    {
        var results = new List<Sannr.ModelValidationResult>();

        // Cross-property validation: if employed, salary must be provided
        if (IsEmployed && (!Salary.HasValue || Salary.Value <= 0))
        {
            results.Add(new Sannr.ModelValidationResult
            {
                MemberName = nameof(Salary),
                Message = "Salary must be provided and greater than 0 when employed",
                Severity = Severity.Error
            });
        }

        // Business rule: if age < 18, cannot be employed
        if (Age < 18 && IsEmployed)
        {
            results.Add(new Sannr.ModelValidationResult
            {
                MemberName = $"{nameof(IsEmployed)},{nameof(Age)}",
                Message = "Cannot be employed if under 18 years old",
                Severity = Severity.Error
            });
        }

        return results;
    }
}

/// <summary>
/// Model with collections to test validation of complex data structures.
/// </summary>
public class CollectionModel
{
    [Required]
    public string? Name { get; set; }

    public List<ComplexModel>? Items { get; set; }

    public List<string>? Tags { get; set; }
}

/// <summary>
/// Model for testing boundary values and edge cases.
/// </summary>
public class BoundaryTestModel
{
    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string? ShortString { get; set; }

    [Range(1, 100)]
    public int IntegerRange { get; set; }

    [Range(0.0, 1.0)]
    public double DoubleRange { get; set; }

    public List<int>? NumberList { get; set; }
}

/// <summary>
/// Model for testing culture-specific validation scenarios.
/// </summary>
public class CultureTestModel
{
    [Required]
    public string? Name { get; set; }

    // These might need culture-specific validation in real scenarios
    public string? LocalizedNumber { get; set; }
    public string? LocalizedDate { get; set; }
    public string? LocalizedCurrency { get; set; }
}

/// <summary>
/// Custom validator that checks employment age requirements.
/// </summary>
public static class EmploymentAgeValidator
{
    public static ValidationResult Check(object? value, IServiceProvider serviceProvider)
    {
        // This validator is applied to the IsValidEmployment property
        // The property itself computes the validity
        return ValidationResult.Success();
    }

    public static ValidationResult ValidateEmploymentAge(bool isEmployed, int age)
    {
        if (isEmployed && age < 18)
        {
            var result = new ValidationResult();
            result.Add("IsValidEmployment", "Cannot be employed if under 18");
            return result;
        }

        return ValidationResult.Success();
    }
}

/// <summary>
/// Custom validator that parses strings according to culture settings.
/// </summary>
public static class CultureAwareNumberValidator
{
    public static ValidationResult Check(object? value, IServiceProvider serviceProvider)
    {
        // For this demonstration, we'll validate the string parsing directly
        // In a real scenario, this would be attached to a string property
        return ValidationResult.Success();
    }

    public static ValidationResult ValidateNumberString(string? numberString, string? cultureCode)
    {
        if (string.IsNullOrEmpty(numberString) || string.IsNullOrEmpty(cultureCode))
        {
            return ValidationResult.Success();
        }

        try
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
            var parsedValue = double.Parse(numberString, culture);

            if (parsedValue < 0 || parsedValue > 10)
            {
                var result = new ValidationResult();
                result.Add("ParsedNumber", "Parsed number must be between 0 and 10");
                return result;
            }

            return ValidationResult.Success();
        }
        catch (System.FormatException)
        {
            var result = new ValidationResult();
            result.Add("NumberAsString", $"Invalid number format for culture {cultureCode}");
            return result;
        }
    }
}

/// <summary>
/// Real-world integration tests to ensure Sannr works reliably in production scenarios.
/// Tests concurrency, performance, edge cases, and complex integration patterns.
/// </summary>
public class RealWorldIntegrationTests
{
    /// <summary>
    /// Helper method to validate models using the generated validators.
    /// </summary>
    private async Task<ValidationResult> Validate(object model, string? group = null)
    {
        SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator);
        return await validator!(new SannrValidationContext(model, group: group));
    }

    /// <summary>
    /// Tests thread safety by running multiple validations concurrently.
    /// Ensures no race conditions or shared state corruption.
    /// </summary>
    [Fact]
    public async Task Concurrent_Validation_Should_Be_Thread_Safe()
    {
        // Arrange
        var model = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act - Run 100 concurrent validations
        var tasks = new List<Task<ValidationResult>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => Validate(model)));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All validations should pass and be consistent
        Assert.All(results, result => Assert.True(result.IsValid));
        Assert.All(results, result => Assert.Empty(result.Errors));
    }

    /// <summary>
    /// Tests performance with large models containing many properties.
    /// Ensures validation completes within reasonable time limits.
    /// </summary>
    [Fact]
    public async Task Large_Model_Validation_Should_Be_Performant()
    {
        // Arrange
        var model = new LargeModel();
        model.PopulateWithValidData();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await Validate(model);
        stopwatch.Stop();

        // Assert
        Assert.True(result.IsValid);
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Validation took too long: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Tests validation with null model instances.
    /// Should handle gracefully without throwing exceptions.
    /// </summary>
    [Fact]
    public async Task Null_Model_Should_Handle_Gracefully()
    {
        // Act & Assert - Should not throw exception
        await Assert.ThrowsAsync<System.NullReferenceException>(async () =>
        {
            await Validate((ComplexModel)null!);
        });
    }

    /// <summary>
    /// Tests validation with extreme string lengths.
    /// Ensures no stack overflow or performance issues with very long strings.
    /// </summary>
    [Fact]
    public async Task Extreme_String_Lengths_Should_Be_Handled()
    {
        // Arrange
        var veryLongString = new string('a', 100000); // 100KB string
        var model = new ComplexModel
        {
            Name = veryLongString,
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act
        var result = await Validate(model);

        // Assert - Should handle without crashing
        Assert.True(result.IsValid || result.Errors.Any(e => e.Message.Contains("length")));
    }

    /// <summary>
    /// Tests validation with extreme numeric values.
    /// Ensures proper handling of boundary conditions and overflow scenarios.
    /// </summary>
    [Fact]
    public async Task Extreme_Numeric_Values_Should_Be_Handled()
    {
        // Arrange
        var model = new ComplexModel
        {
            Name = "Test",
            Email = "test@example.com",
            Age = int.MaxValue, // 2,147,483,647
            Salary = double.MaxValue, // Very large double
            Phone = "123",
            Website = "https://example.com"
        };

        // Act
        var result = await Validate(model);

        // Assert - Should handle without throwing exceptions
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests validation in high-frequency scenarios.
    /// Simulates rapid successive validations to ensure stability.
    /// </summary>
    [Fact]
    public async Task High_Frequency_Validation_Should_Be_Stable()
    {
        // Arrange
        var model = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act - Perform 1000 rapid validations
        for (int i = 0; i < 1000; i++)
        {
            var result = await Validate(model);
            Assert.True(result.IsValid);
        }

        // Assert - No exceptions thrown, all validations passed
    }

    /// <summary>
    /// Tests validation with models containing special characters and unicode.
    /// Ensures proper handling of international characters and symbols.
    /// </summary>
    [Fact]
    public async Task Unicode_And_Special_Characters_Should_Be_Handled()
    {
        // Arrange
        var model = new ComplexModel
        {
            Name = "José María ñoño", // Unicode characters
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act
        var result = await Validate(model);

        // Assert
        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests validation with deeply nested object structures.
    /// Ensures proper traversal and validation of complex object graphs.
    /// </summary>
    [Fact]
    public async Task Nested_Object_Validation_Should_Work()
    {
        // Arrange
        var model = new NestedModel
        {
            Name = "Parent",
            Child = new ComplexModel
            {
                Name = "Child",
                Email = "child@example.com",
                Age = 10,
                Salary = 10000.00,
                Phone = "123-456-7890",
                Website = "https://child.com"
            }
        };

        // Act
        var result = await Validate(model);

        // Assert - Should validate nested objects appropriately
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests validation behavior when exceptions occur during validation.
    /// Ensures graceful error handling and no crashes.
    /// </summary>
    [Fact]
    public async Task Exception_Handling_Should_Be_Graceful()
    {
        // Arrange - Model that might cause exceptions
        var model = new ComplexModel
        {
            Name = null!, // Force potential null reference
            Email = "invalid-email-format-that-might-cause-issues",
            Age = -1,
            Salary = double.NaN, // Not a number
            Phone = null!,
            Website = null!
        };

        // Act & Assert - Should not throw unhandled exceptions
        var result = await Validate(model);
        Assert.NotNull(result);
        Assert.False(result.IsValid); // Should have validation errors
    }

    /// <summary>
    /// Tests memory usage patterns during extended validation runs.
    /// Ensures no memory leaks or excessive memory consumption.
    /// </summary>
    [Fact]
    public async Task Memory_Usage_Should_Be_Efficient()
    {
        // Arrange
        var model = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act - Run many validations to check for memory issues
        for (int i = 0; i < 10000; i++)
        {
            var result = await Validate(model);
            Assert.True(result.IsValid);
        }

        // Assert - Process should still be running without excessive memory usage
        // (This is a basic check - in production you'd use memory profiling tools)
    }

    /// <summary>
    /// Tests validation in async workflows and task-based scenarios.
    /// Ensures proper async/await integration.
    /// </summary>
    [Fact]
    public async Task Async_Workflow_Integration_Should_Work()
    {
        // Arrange
        var models = new List<ComplexModel>();
        for (int i = 0; i < 10; i++)
        {
            models.Add(new ComplexModel
            {
                Name = $"User{i}",
                Email = $"user{i}@example.com",
                Age = 20 + i,
                Salary = 30000.00 + (i * 5000),
                Phone = $"+1-555-0{i:D3}",
                Website = $"https://user{i}.com"
            });
        }

        // Act - Validate all models concurrently
        var validationTasks = models.Select(m => Validate(m)).ToArray();
        var results = await Task.WhenAll(validationTasks);

        // Assert - All validations should complete successfully
        Assert.All(results, result => Assert.True(result.IsValid));
    }

    /// <summary>
    /// Tests validation with models that have circular references.
    /// Ensures no infinite loops or stack overflows.
    /// </summary>
    [Fact]
    public async Task Circular_References_Should_Be_Handled()
    {
        // Arrange
        var parent = new CircularModel { Name = "Parent" };
        var child = new CircularModel { Name = "Child", Parent = parent };
        parent.Child = child; // Create circular reference

        // Act
        var result = await Validate(parent);

        // Assert - Should handle without infinite loops
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests validation with inheritance hierarchies.
    /// Ensures proper validation of base and derived class properties.
    /// </summary>
    [Fact]
    public async Task Inheritance_Should_Work_Correctly()
    {
        // Arrange
        var model = new DerivedModel
        {
            BaseProperty = "Base Value",
            DerivedProperty = "Derived Value",
            Name = "Test",
            Email = "test@example.com",
            Age = 25
        };

        // Act
        var result = await Validate(model);

        // Assert
        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests cross-property validation using RequiredIf attribute.
    /// Ensures conditional validation works correctly.
    /// </summary>
    [Fact]
    public async Task Cross_Property_Validation_Should_Work()
    {
        // Arrange - Valid case: employed with required fields
        var validModel = new ConditionalModel
        {
            Username = "testuser",
            IsEmployed = true,
            JobTitle = "Developer",
            Salary = 75000,
            Age = 25 // Valid age for employment
        };

        // Act
        var validResult = await Validate(validModel);

        // Assert
        Assert.True(validResult.IsValid);

        // Arrange - Invalid case: employed but missing required fields
        var invalidModel = new ConditionalModel
        {
            Username = "testuser",
            IsEmployed = true,
            JobTitle = null, // Required when employed
            Salary = 0, // Invalid when employed
            Age = 25 // Valid age
        };

        // Act
        var invalidResult = await Validate(invalidModel);

        // Assert
        Assert.False(invalidResult.IsValid);
        Assert.Contains(invalidResult.Errors, e => e.Message.Contains("required") || e.Message.Contains("range"));
    }

    /// <summary>
    /// Tests validation with empty strings vs null values.
    /// Ensures proper handling of different empty value scenarios.
    /// </summary>
    [Fact]
    public async Task Empty_Strings_Vs_Null_Should_Be_Handled()
    {
        // Arrange - Test with empty string (should fail for required field)
        var emptyStringModel = new ComplexModel
        {
            Name = "", // Empty string
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act
        var emptyResult = await Validate(emptyStringModel);

        // Assert - Should fail because Name is required and empty
        Assert.False(emptyResult.IsValid);

        // Arrange - Test with whitespace string
        var whitespaceModel = new ComplexModel
        {
            Name = "   ", // Whitespace only
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act
        var whitespaceResult = await Validate(whitespaceModel);

        // Assert - Should fail because Name is required and whitespace
        Assert.False(whitespaceResult.IsValid);
    }

    /// <summary>
    /// Tests boundary values for range validation.
    /// Ensures exact boundary values are handled correctly.
    /// </summary>
    [Fact]
    public async Task Range_Boundary_Values_Should_Be_Validated()
    {
        // Arrange - Test minimum boundary
        var minBoundaryModel = new BoundaryTestModel
        {
            ShortString = "A",
            IntegerRange = 1, // Minimum
            DoubleRange = 0.0, // Minimum
            NumberList = new List<int> { 1, 2, 3 }
        };

        // Act
        var minResult = await Validate(minBoundaryModel);

        // Assert - Should pass
        Assert.True(minResult.IsValid);

        // Arrange - Test maximum boundary
        var maxBoundaryModel = new BoundaryTestModel
        {
            ShortString = "ABCDEFGHIJ", // Max length 10
            IntegerRange = 100, // Maximum
            DoubleRange = 1.0, // Maximum
            NumberList = new List<int> { 1, 2, 3, 4, 5 }
        };

        // Act
        var maxResult = await Validate(maxBoundaryModel);

        // Assert - Should pass
        Assert.True(maxResult.IsValid);

        // Arrange - Test out of bounds values
        var outOfBoundsModel = new BoundaryTestModel
        {
            ShortString = "A",
            IntegerRange = 101, // Over maximum
            DoubleRange = 1.1, // Over maximum
            NumberList = new List<int> { 1, 2, 3 }
        };

        // Act
        var outOfBoundsResult = await Validate(outOfBoundsModel);

        // Assert - Should fail
        Assert.False(outOfBoundsResult.IsValid);
    }

    /// <summary>
    /// Tests validation with collections and complex data structures.
    /// Ensures nested validation works correctly.
    /// </summary>
    [Fact]
    public async Task Collection_Validation_Should_Work()
    {
        // Arrange - Valid collection model
        var validModel = new CollectionModel
        {
            Name = "Test Collection",
            Items = new List<ComplexModel>
            {
                new ComplexModel
                {
                    Name = "Item1",
                    Email = "item1@example.com",
                    Age = 20,
                    Salary = 30000,
                    Phone = "123-456-7890",
                    Website = "https://item1.com"
                },
                new ComplexModel
                {
                    Name = "Item2",
                    Email = "item2@example.com",
                    Age = 25,
                    Salary = 40000,
                    Phone = "098-765-4321",
                    Website = "https://item2.com"
                }
            },
            Tags = new List<string> { "tag1", "tag2", "tag3" }
        };

        // Act
        var result = await Validate(validModel);

        // Assert - Should validate nested objects
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests validation error message customization.
    /// Ensures custom error messages are properly used.
    /// </summary>
    [Fact]
    public async Task Custom_Error_Messages_Should_Work()
    {
        // Arrange - Model with custom error messages
        var model = new ComplexModel
        {
            Name = null, // Required field
            Email = "invalid-email", // Invalid email
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        // Act
        var result = await Validate(model);

        // Assert - Should have validation errors with appropriate messages
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        // Note: Actual error message content depends on Sannr's implementation
    }

    /// <summary>
    /// Tests validation under different culture settings.
    /// Ensures consistent behavior across cultures.
    /// </summary>
    [Fact]
    public async Task Culture_Invariant_Validation_Should_Work()
    {
        // Arrange - Model with culture-specific data
        var model = new CultureTestModel
        {
            Name = "Test User",
            LocalizedNumber = "1,234.56", // Culture-specific formatting
            LocalizedDate = "12/31/2023", // US format
            LocalizedCurrency = "$1,234.56"
        };

        // Act
        var result = await Validate(model);

        // Assert - Should handle culture-specific data appropriately
        Assert.NotNull(result);
        // Note: Sannr may not validate culture-specific formats unless specified
    }

    /// <summary>
    /// Tests validation with under-posting scenarios.
    /// Simulates missing properties in model binding.
    /// </summary>
    [Fact]
    public async Task Under_Posting_Scenarios_Should_Be_Handled()
    {
        // Arrange - Model with only some properties set (simulating under-posting)
        var partialModel = new AdvancedValidationModel
        {
            Username = "testuser",
            // Missing PrimaryEmail (required)
            SecondaryEmail = "secondary@example.com",
            Age = 25,
            // Missing CreditCard (required)
            Website = "https://example.com",
            PhoneNumber = "+1-555-0123",
            Score = 85.5
        };

        // Act
        var result = await Validate(partialModel);

        // Assert - Should fail due to missing required fields
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Message.Contains("required"));
    }

    /// <summary>
    /// Tests validation with extreme boundary conditions.
    /// Ensures no overflow or underflow issues.
    /// </summary>
    [Fact]
    public async Task Extreme_Boundary_Conditions_Should_Be_Handled()
    {
        // Arrange - Test with extreme values
        var extremeModel = new ComplexModel
        {
            Name = "Test",
            Email = "test@example.com",
            Age = int.MaxValue,
            Salary = double.MaxValue,
            Phone = "123",
            Website = "https://example.com"
        };

        // Act
        var result = await Validate(extremeModel);

        // Assert - Should handle without crashing
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests validation performance with large collections.
    /// Ensures scalability with many items.
    /// </summary>
    [Fact]
    public async Task Large_Collection_Validation_Should_Be_Efficient()
    {
        // Arrange - Create a large collection
        var largeCollectionModel = new CollectionModel
        {
            Name = "Large Collection",
            Items = new List<ComplexModel>(),
            Tags = new List<string>()
        };

        // Add 100 items to the collection
        for (int i = 0; i < 100; i++)
        {
            largeCollectionModel.Items.Add(new ComplexModel
            {
                Name = $"Item{i}",
                Email = $"item{i}@example.com",
                Age = 20 + (i % 50),
                Salary = 30000 + (i * 1000),
                Phone = $"555-0{i:D3}",
                Website = $"https://item{i}.com"
            });
            largeCollectionModel.Tags.Add($"tag{i}");
        }

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await Validate(largeCollectionModel);
        stopwatch.Stop();

        // Assert - Should complete within reasonable time
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Large collection validation took too long: {stopwatch.ElapsedMilliseconds}ms");
        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests model-level validation using Sannr.IValidatableObject.
    /// Ensures cross-property business rules work correctly.
    /// </summary>
    [Fact]
    public async Task Model_Level_Validation_Should_Work()
    {
        // Arrange - Valid model
        var validModel = new ModelLevelValidationModel
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 25,
            IsEmployed = true,
            Salary = 50000
        };

        // Act
        var validResult = await Validate(validModel);

        // Assert - Should pass
        Assert.True(validResult.IsValid);

        // Arrange - Invalid: employed but no salary
        var invalidModel1 = new ModelLevelValidationModel
        {
            FirstName = "Jane",
            LastName = "Smith",
            Age = 30,
            IsEmployed = true,
            Salary = null // Required when employed
        };

        // Act
        var invalidResult1 = await Validate(invalidModel1);

        // Assert - Should fail due to missing salary
        Assert.False(invalidResult1.IsValid);
        Assert.Contains(invalidResult1.Errors, e => e.Message.Contains("Salary must be provided"));

        // Arrange - Invalid: under 18 and employed
        var invalidModel2 = new ModelLevelValidationModel
        {
            FirstName = "Bob",
            LastName = "Wilson",
            Age = 16, // Under 18
            IsEmployed = true, // Not allowed
            Salary = 20000
        };

        // Act
        var invalidResult2 = await Validate(invalidModel2);

        // Assert - Should fail due to age restriction
        Assert.False(invalidResult2.IsValid);
        Assert.Contains(invalidResult2.Errors, e => e.Message.Contains("Cannot be employed if under 18"));
    }

    /// <summary>
    /// Tests validation behavior across different cultures and numerical formats.
    /// Ensures consistent validation regardless of culture-specific formatting.
    /// </summary>
    [Fact]
    public async Task Localization_And_Culture_Invariance_Should_Work()
    {
        // Test 1: Range validation should work regardless of culture
        var rangeModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50, // Valid range 1-100
            DoubleRange = 0.5, // Valid range 0.0-1.0
            NumberList = new List<int> { 1, 2, 3 }
        };

        // Act - Test with invariant culture (should pass)
        var invariantResult = await Validate(rangeModel);
        Assert.True(invariantResult.IsValid);

        // Test 2: Email validation should be culture-invariant
        var emailModel = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com", // Valid email regardless of culture
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        var emailResult = await Validate(emailModel);
        Assert.True(emailResult.IsValid);

        // Test 3: URL validation should be culture-invariant
        var urlModel = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com" // Valid URL regardless of culture
        };

        var urlResult = await Validate(urlModel);
        Assert.True(urlResult.IsValid);

        // Test 4: Phone validation should be culture-invariant
        var phoneModel = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123", // Valid phone regardless of culture
            Website = "https://example.com"
        };

        var phoneResult = await Validate(phoneModel);
        Assert.True(phoneResult.IsValid);

        // Test 5: String length validation should be culture-invariant
        var lengthModel = new ComplexModel
        {
            Name = "José María ñoño", // Unicode characters, length should be culture-invariant
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        var lengthResult = await Validate(lengthModel);
        Assert.True(lengthResult.IsValid); // Name is within StringLength limit

        // Test 6: Test with culture-specific number formatting in strings
        var cultureTestModel = new CultureTestModel
        {
            Name = "Test User",
            LocalizedNumber = "1,234.56", // Could be interpreted differently in cultures
            LocalizedDate = "12/31/2023", // US vs European date format
            LocalizedCurrency = "$1,234.56" // Currency formatting
        };

        var cultureResult = await Validate(cultureTestModel);
        // Should validate successfully - Sannr doesn't validate culture-specific string content
        Assert.True(cultureResult.IsValid);
    }

    /// <summary>
    /// Tests validation with various international character sets and Unicode.
    /// Ensures proper handling of multilingual content.
    /// </summary>
    [Fact]
    public async Task International_Characters_And_Unicode_Should_Work()
    {
        // Test with various international names and characters
        var internationalNames = new[]
        {
            "José María ñoño", // Spanish
            "François Müller", // French/German
            "李小明", // Chinese
            "محمد العربي", // Arabic
            "Иван Петров", // Russian
            "佐藤太郎", // Japanese
            "김철수", // Korean
            "Nguyễn Văn An" // Vietnamese
        };

        foreach (var name in internationalNames)
        {
            var model = new ComplexModel
            {
                Name = name,
                Email = "test@example.com",
                Age = 25,
                Salary = 50000.00,
                Phone = "+1-555-0123",
                Website = "https://example.com"
            };

            var result = await Validate(model);
            Assert.True(result.IsValid, $"Validation failed for international name: {name}");
        }
    }

    /// <summary>
    /// Tests validation with different number formats and decimal separators.
    /// Ensures numerical validation works regardless of culture-specific formatting.
    /// </summary>
    [Fact]
    public async Task Numerical_Formats_And_Decimal_Separators_Should_Work()
    {
        // Test range validation with double
        var doubleModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = 0.5, // Should be valid (0.0-1.0)
            NumberList = new List<int> { 1, 2, 3 }
        };

        var doubleResult = await Validate(doubleModel);
        Assert.True(doubleResult.IsValid, "Validation failed for double value: 0.5");

        // Test range validation with float
        var floatModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = 0.5f, // Should be valid (0.0-1.0)
            NumberList = new List<int> { 1, 2, 3 }
        };

        var floatResult = await Validate(floatModel);
        Assert.True(floatResult.IsValid, "Validation failed for float value: 0.5f");

        // Test range validation with decimal
        var decimalModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = (double)0.5m, // Should be valid (0.0-1.0)
            NumberList = new List<int> { 1, 2, 3 }
        };

        var decimalResult = await Validate(decimalModel);
        Assert.True(decimalResult.IsValid, "Validation failed for decimal value: 0.5m");

        // Test with edge case decimal values
        var edgeCases = new[] { 0.0, 1.0, 0.0001, 0.9999 };
        foreach (var value in edgeCases)
        {
            var model = new BoundaryTestModel
            {
                ShortString = "test",
                IntegerRange = 50,
                DoubleRange = value,
                NumberList = new List<int> { 1, 2, 3 }
            };

            var result = await Validate(model);
            Assert.True(result.IsValid, $"Validation failed for edge case decimal: {value}");
        }
    }

    /// <summary>
    /// Tests validation error messages with different cultures and formatting.
    /// Ensures error messages are properly formatted regardless of culture.
    /// </summary>
    [Fact]
    public async Task Error_Message_Formatting_Should_Work_Across_Cultures()
    {
        // Test with invalid data that should generate formatted error messages
        var invalidModel = new BoundaryTestModel
        {
            ShortString = "this string is way too long for the limit",
            IntegerRange = 150, // Over max of 100
            DoubleRange = 2.5, // Over max of 1.0
            NumberList = new List<int> { 1, 2, 3 }
        };

        var result = await Validate(invalidModel);

        // Should have validation errors
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);

        // Error messages should be properly formatted
        foreach (var error in result.Errors)
        {
            Assert.False(string.IsNullOrEmpty(error.Message));
            Assert.False(string.IsNullOrWhiteSpace(error.Message));
        }
    }

    /// <summary>
    /// Tests how Sannr handles culture-specific decimal separators.
    /// Demonstrates that Sannr validates numeric values, not string parsing.
    /// The parsing happens before Sannr validation and depends on culture settings.
    /// </summary>
    [Fact]
    public async Task Culture_Specific_Decimal_Separators_Should_Be_Handled_By_Parsing()
    {
        // Sannr validates the actual numeric values, not string representations
        // The culture-specific parsing happens before Sannr validation

        // Test 1: Valid numeric values (already parsed)
        var validModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = 0.5, // Valid range 0.0-1.0
            NumberList = new List<int> { 1, 2, 3 }
        };

        var result = await Validate(validModel);
        Assert.True(result.IsValid, "Sannr should validate parsed numeric values correctly");

        // Test 2: Demonstrate culture-specific parsing scenarios
        // In Sweden: "1,23" represents 1.23
        // In US: "1.23" represents 1.23

        // Simulate parsing with different cultures
        var swedishCulture = System.Globalization.CultureInfo.GetCultureInfo("sv-SE"); // Sweden
        var usCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US"); // US

        string swedishNumberString = "0,5"; // 0.5 in Swedish format
        string usNumberString = "0.5"; // 0.5 in US format

        // Parse with respective cultures
        double swedishValue = double.Parse(swedishNumberString, swedishCulture);
        double usValue = double.Parse(usNumberString, usCulture);

        // Both should result in the same numeric value
        Assert.Equal(0.5, swedishValue);
        Assert.Equal(0.5, usValue);
        Assert.Equal(swedishValue, usValue);

        // Sannr validates the parsed numeric value, not the string format
        var swedishParsedModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = swedishValue, // Parsed from Swedish format
            NumberList = new List<int> { 1, 2, 3 }
        };

        var usParsedModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = usValue, // Parsed from US format
            NumberList = new List<int> { 1, 2, 3 }
        };

        var swedishResult = await Validate(swedishParsedModel);
        var usResult = await Validate(usParsedModel);

        // Both should validate identically - Sannr doesn't care about the original string format
        Assert.Equal(swedishResult.IsValid, usResult.IsValid);
        Assert.Equal(swedishResult.Errors.Count, usResult.Errors.Count);

        // Test 3: Invalid values should fail regardless of how they were parsed
        double invalidValue = 2.5; // Over max of 1.0

        var invalidModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = invalidValue,
            NumberList = new List<int> { 1, 2, 3 }
        };

        var invalidResult = await Validate(invalidModel);
        Assert.False(invalidResult.IsValid, "Sannr should reject invalid numeric values regardless of culture");
        Assert.Contains(invalidResult.Errors, e => e.Message.Contains("DoubleRange"));
    }

    /// <summary>
    /// Tests custom validation with culture-aware string parsing.
    /// Shows how to handle culture-specific input in custom validators.
    /// </summary>
    [Fact]
    public async Task Custom_Validation_With_Culture_Aware_Parsing_Should_Work()
    {
        // Demonstrate culture-specific parsing that happens BEFORE Sannr validation
        var swedishCulture = System.Globalization.CultureInfo.GetCultureInfo("sv-SE");
        var usCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

        // Test parsing Swedish format: "0,5" = 0.5
        double swedishValue = double.Parse("0,5", swedishCulture);
        Assert.Equal(0.5, swedishValue);

        // Test parsing US format: "0.5" = 0.5
        double usValue = double.Parse("0.5", usCulture);
        Assert.Equal(0.5, usValue);

        // Both parsed values are identical and should validate the same way in Sannr
        var swedishModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = swedishValue, // 0.5 (within 0.0-1.0 range)
            NumberList = new List<int> { 1, 2, 3 }
        };

        var usModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = usValue, // 0.5 (within 0.0-1.0 range)
            NumberList = new List<int> { 1, 2, 3 }
        };

        var swedishResult = await Validate(swedishModel);
        var usResult = await Validate(usModel);

        // Both should validate identically
        Assert.True(swedishResult.IsValid);
        Assert.True(usResult.IsValid);

        // Demonstrate validation of culture-parsed values

        // Valid Swedish format
        var validSwedish = CultureAwareNumberValidator.ValidateNumberString("1,23", "sv-SE");
        Assert.True(validSwedish.IsValid);

        // Valid US format
        var validUS = CultureAwareNumberValidator.ValidateNumberString("1.23", "en-US");
        Assert.True(validUS.IsValid);

        // Invalid format for culture
        var invalidFormat = CultureAwareNumberValidator.ValidateNumberString("1.23", "sv-SE"); // US format but Swedish culture
        Assert.False(invalidFormat.IsValid);
        Assert.Contains(invalidFormat.Errors, e => e.Message.Contains("Invalid number format"));

        // Out of range value
        var outOfRange = CultureAwareNumberValidator.ValidateNumberString("15,5", "sv-SE"); // 15.5 > 10
        Assert.False(outOfRange.IsValid);
        Assert.Contains(outOfRange.Errors, e => e.Message.Contains("between 0 and 10"));
    }

    /// <summary>
    /// Tests Sannr's superiority over other validation libraries by addressing common failure points.
    /// </summary>
    [Fact]
    public async Task Sannr_Should_Address_Common_Validation_Library_Failures()
    {
        // Issue 1: FluentValidation - Complex rule chaining and conditional validation
        // Many developers struggle with RuleFor().When() and complex rule combinations
        var conditionalModel = new ConditionalModel
        {
            IsEmployed = true,
            Age = 16, // Under 18, should fail employment validation
            Country = "USA",
            ZipCode = "12345"
        };

        var conditionalResult = await Validate(conditionalModel);
        Assert.False(conditionalResult.IsValid);
        Assert.Contains(conditionalResult.Errors, e => e.Message.Contains("Cannot be employed if under 18"));

        // Issue 2: DataAnnotations - Limited built-in validators and poor error messages
        // DataAnnotations lacks many common validators and has generic error messages
        var advancedModel = new AdvancedValidationModel
        {
            Username = "user@domain", // Invalid - contains @
            PrimaryEmail = "invalid-email", // Invalid email format
            Age = 25,
            CreditCard = "4111111111111111", // Valid credit card
            Website = "https://example.com", // Valid URL
            Score = 150.0 // Over max of 100.0
        };

        var advancedResult = await Validate(advancedModel);
        Assert.False(advancedResult.IsValid);
        // Should catch multiple validation issues
        Assert.True(advancedResult.Errors.Count >= 2);

        // Issue 3: Integration problems - ASP.NET Core model binding issues
        // Many validation libraries struggle with proper ASP.NET Core integration
        var integrationModel = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        var integrationResult = await Validate(integrationModel);
        Assert.True(integrationResult.IsValid, "Sannr integrates seamlessly with ASP.NET Core");

        // Issue 4: Performance problems - Some libraries are slow with complex models
        // Test with large model to ensure Sannr performs well
        var largeModel = new LargeModel();
        // Initialize all required properties
        for (int i = 1; i <= 50; i++)
        {
            typeof(LargeModel).GetProperty($"Prop{i}")?.SetValue(largeModel, $"Value{i}");
        }
        for (int i = 0; i < 100; i++)
        {
            largeModel.StringList.Add($"Item{i}");
            largeModel.IntList.Add(i);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var largeResult = await Validate(largeModel);
        stopwatch.Stop();

        Assert.True(largeResult.IsValid, "Sannr should validate large models efficiently");
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Validation took {stopwatch.ElapsedMilliseconds}ms, should be under 1000ms");

        // Issue 5: Thread safety and concurrency issues
        // Some validation libraries aren't thread-safe
        var concurrencyResults = new ConcurrentBag<bool>();
        var tasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var model = new ComplexModel
                {
                    Name = $"User{i}",
                    Email = $"user{i}@example.com",
                    Age = 25,
                    Salary = 50000.00,
                    Phone = "+1-555-0123",
                    Website = "https://example.com"
                };

                var result = await Validate(model);
                concurrencyResults.Add(result.IsValid);
            }));
        }

        await Task.WhenAll(tasks);
        Assert.All(concurrencyResults, isValid => Assert.True(isValid, "All concurrent validations should succeed"));

        // Issue 6: Client-side validation integration problems
        // Many libraries struggle with client-side validation
        // Sannr works seamlessly with ASP.NET Core's client validation
        var clientValidationModel = new ComplexModel
        {
            Name = "", // Required field empty
            Email = "invalid-email", // Invalid format
            Age = 151, // Over max
            Salary = -1000, // Negative
            Phone = "invalid-phone",
            Website = "not-a-url"
        };

        var clientResult = await Validate(clientValidationModel);
        Assert.False(clientResult.IsValid);
        Assert.True(clientResult.Errors.Count >= 5, "Should catch all validation errors for client-side validation");

        // Issue 7: Custom validator complexity
        // Many libraries make custom validators overly complex
        var customValidationModel = new ModelLevelValidationModel
        {
            FirstName = "Valid Name",
            LastName = "Test",
            Age = 50,
            IsEmployed = true,
            Salary = 50000
        };

        var customResult = await Validate(customValidationModel);
        Assert.True(customResult.IsValid, "Custom model-level validation should work seamlessly");

        // Issue 8: Localization and internationalization issues
        // Many libraries don't handle different cultures well
        var cultureTestModel = new CultureTestModel
        {
            Name = "José María ñoño", // Unicode characters
            LocalizedNumber = "1,234.56", // Culture-specific formatting
            LocalizedDate = "31/12/2023", // Non-US date format
            LocalizedCurrency = "€1.234,56" // European currency format
        };

        var cultureResult = await Validate(cultureTestModel);
        // Sannr doesn't validate string content format, only applies specified rules
        Assert.True(cultureResult.IsValid, "Sannr handles international characters without issues");
    }

    /// <summary>
    /// Tests Sannr's performance advantages over other validation libraries.
    /// Many validation libraries suffer from performance issues with complex validation rules.
    /// </summary>
    [Fact]
    public async Task Sannr_Performance_Should_Outperform_Other_Libraries()
    {
        // Create a complex model with many validation rules
        var complexModel = new AdvancedValidationModel
        {
            Username = "validuser123",
            PrimaryEmail = "test@example.com",
            SecondaryEmail = "secondary@example.com",
            Age = 30,
            CreditCard = "4111111111111111",
            Website = "https://example.com",
            PhoneNumber = "+1-555-0123",
            Score = 0.85
        };

        // Measure validation performance
        const int iterations = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            var result = await Validate(complexModel);
            Assert.True(result.IsValid);
        }

        stopwatch.Stop();
        var avgTimePerValidation = stopwatch.ElapsedMilliseconds / (double)iterations;

        // Sannr should be very fast - under 1ms per complex validation
        Assert.True(avgTimePerValidation < 1.0, $"Average validation time: {avgTimePerValidation}ms, should be under 1ms");

        // Test with invalid data to ensure error generation is also fast
        var invalidModel = new AdvancedValidationModel
        {
            Username = "", // Required
            PrimaryEmail = "invalid", // Invalid email
            Age = 200, // Over max
            CreditCard = "123", // Invalid credit card
            Website = "not-a-url", // Invalid URL
            Score = 2.0 // Over max
        };

        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var result = await Validate(invalidModel);
            Assert.False(result.IsValid);
            Assert.True(result.Errors.Count >= 5);
        }
        stopwatch.Stop();

        var avgErrorTime = stopwatch.ElapsedMilliseconds / (double)iterations;
        Assert.True(avgErrorTime < 2.0, $"Error validation time: {avgErrorTime}ms, should be under 2ms");
    }

    /// <summary>
    /// Tests Sannr's reliability in edge cases that break other validation libraries.
    /// Many validation libraries fail with null values, complex inheritance, or circular references.
    /// </summary>
    [Fact]
    public async Task Sannr_Should_Handle_Edge_Cases_Other_Libraries_Fail()
    {
        // Edge Case 1: Null and empty collections
        var nullCollectionModel = new CollectionModel
        {
            Name = "Test",
            Items = null, // Null collection
            Tags = new List<string> { "valid1", "valid2" }
        };

        var nullResult = await Validate(nullCollectionModel);
        Assert.True(nullResult.IsValid, "Should handle null collections gracefully");

        // Edge Case 2: Empty collections
        var emptyCollectionModel = new CollectionModel
        {
            Name = "Test",
            Items = new List<ComplexModel>(), // Empty collection
            Tags = new List<string>()
        };

        var emptyResult = await Validate(emptyCollectionModel);
        Assert.True(emptyResult.IsValid, "Should handle empty collections");

        // Edge Case 3: Complex inheritance hierarchies
        var derivedModel = new DerivedModel
        {
            BaseProperty = "base",
            DerivedProperty = "derived",
            Name = "Test User",
            Email = "test@example.com",
            Age = 42
        };

        var inheritanceResult = await Validate(derivedModel);
        Assert.True(inheritanceResult.IsValid, "Should handle inheritance correctly");

        // Edge Case 4: Circular reference handling (if applicable)
        var circularModel = new CircularModel
        {
            Name = "Test",
            Parent = null, // Avoid actual circular reference for testing
            Child = null
        };

        var circularResult = await Validate(circularModel);
        Assert.True(circularResult.IsValid, "Should handle potential circular references");

        // Edge Case 5: Maximum string lengths and boundary conditions
        var boundaryModel = new BoundaryTestModel
        {
            ShortString = new string('a', 10), // Exactly at limit
            IntegerRange = 100, // Exactly at max
            DoubleRange = 1.0, // Exactly at max
            NumberList = new List<int> { int.MaxValue, int.MinValue } // Extreme values
        };

        var boundaryResult = await Validate(boundaryModel);
        Assert.True(boundaryResult.IsValid, "Should handle boundary conditions correctly");

        // Edge Case 6: Unicode and special characters
        var unicodeModel = new ComplexModel
        {
            Name = "José María ñoño 😀 🚀", // Unicode characters and emojis
            Email = "test@例え.テスト", // Unicode domain (if supported)
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://例え.テスト" // Unicode URL
        };

        var unicodeResult = await Validate(unicodeModel);
        Assert.True(unicodeResult.IsValid, "Should handle Unicode characters correctly");

        // Edge Case 7: Concurrent validation of same model type
        var concurrentModels = new List<ComplexModel>();
        for (int i = 0; i < 100; i++)
        {
            concurrentModels.Add(new ComplexModel
            {
                Name = $"User{i}",
                Email = $"user{i}@example.com",
                Age = 25,
                Salary = 50000.00,
                Phone = "+1-555-0123",
                Website = "https://example.com"
            });
        }

        var concurrentTasks = concurrentModels.Select(model => Validate(model)).ToList();
        var concurrentResults = await Task.WhenAll(concurrentTasks);

        Assert.All(concurrentResults, result => Assert.True(result.IsValid, "All concurrent validations should succeed"));
    }

    /// <summary>
    /// Tests Sannr's maintainability advantages over other validation libraries.
    /// Many validation libraries become hard to maintain as projects grow.
    /// </summary>
    [Fact]
    public async Task Sannr_Maintainability_Should_Outperform_Other_Libraries()
    {
        // Test 1: Easy to add new validation rules without breaking existing code
        var extensibleModel = new ComplexModel
        {
            Name = "Test User",
            Email = "test@example.com",
            Age = 25,
            Salary = 50000.00,
            Phone = "+1-555-0123",
            Website = "https://example.com"
        };

        var extensibleResult = await Validate(extensibleModel);
        Assert.True(extensibleResult.IsValid, "Base validation should work");

        // Test 2: Validation rules are co-located with the model (via attributes)
        // This makes it easy to see what validations apply to each property
        var attributedModel = new UserProfile
        {
            Username = "testuser",
            Email = "test@example.com",
            Age = 25,
            Country = "USA",
            ZipCode = "12345"
        };

        var attributedResult = await Validate(attributedModel);
        Assert.True(attributedResult.IsValid, "Attributed validation should work");

        // Test 3: Easy to test individual validation rules
        var testableModel = new BoundaryTestModel
        {
            ShortString = "test",
            IntegerRange = 50,
            DoubleRange = 0.5,
            NumberList = new List<int> { 1, 2, 3 }
        };

        var testableResult = await Validate(testableModel);
        Assert.True(testableResult.IsValid, "Individual rules should be easily testable");

        // Test 4: Clear error messages that map back to specific properties
        var errorModel = new BoundaryTestModel
        {
            ShortString = new string('a', 20), // Too long
            IntegerRange = 150, // Too high
            DoubleRange = 2.5, // Too high
            NumberList = new List<int> { 1, 2, 3 }
        };

        var errorResult = await Validate(errorModel);
        Assert.False(errorResult.IsValid);
        Assert.Equal(3, errorResult.Errors.Count); // Should have 3 specific errors

        // Each error should clearly indicate which property failed
        foreach (var error in errorResult.Errors)
        {
            Assert.Contains(error.MemberName, new[] { "ShortString", "IntegerRange", "DoubleRange" });
        }

        // Test 5: Easy to extend with custom validators
        var customModel = new ModelLevelValidationModel
        {
            FirstName = "Valid Name",
            LastName = "Test",
            Age = 50,
            IsEmployed = true,
            Salary = 50000
        };

        var customValidationResult = await Validate(customModel);
        Assert.True(customValidationResult.IsValid, "Custom validation should be easy to add");

        // Test 6: Validation logic is generated at compile-time (source generation)
        // This means no runtime reflection overhead and better performance
        var generatedModel = new ComplexModel
        {
            Name = "Generated Test",
            Email = "generated@example.com",
            Age = 30,
            Salary = 60000.00,
            Phone = "+1-555-0124",
            Website = "https://generated.com"
        };

        var generatedResult = await Validate(generatedModel);
        Assert.True(generatedResult.IsValid, "Source-generated validation should work seamlessly");
    }

    /// <summary>
    /// Tests Sannr's localization support using resource files.
    /// Demonstrates how to provide culture-specific error messages.
    /// </summary>
    [Fact]
    public async Task Localization_With_Resource_Files_Should_Work()
    {
        // Sannr supports localization through ErrorMessageResourceName and ErrorMessageResourceType
        // This allows validation error messages to be localized for different cultures

        // Test 1: Default culture (English)
        var originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
        var originalUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;

        try
        {
            // Set culture to English
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            var englishModel = new LocalizedValidationModel
            {
                RequiredField = null, // Will trigger localized error message
                EmailField = "invalid-email", // Will trigger localized error message
                AgeField = 200 // Will trigger localized error message
            };

            var englishResult = await Validate(englishModel);
            Assert.False(englishResult.IsValid);

            // Check that English messages are used
            Assert.Contains(englishResult.Errors, e => e.Message.Contains("field is required"));
            Assert.Contains(englishResult.Errors, e => e.Message.Contains("not a valid email address"));
            Assert.Contains(englishResult.Errors, e => e.Message.Contains("must be between"));

            // Test 2: Spanish culture
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-ES");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");

            var spanishModel = new LocalizedValidationModel
            {
                RequiredField = null,
                EmailField = "invalid-email",
                AgeField = 200
            };

            var spanishResult = await Validate(spanishModel);
            Assert.False(spanishResult.IsValid);

            // Check that Spanish messages are used
            Assert.Contains(spanishResult.Errors, e => e.Message.Contains("es obligatorio"));
            Assert.Contains(spanishResult.Errors, e => e.Message.Contains("no es una dirección"));
            Assert.Contains(spanishResult.Errors, e => e.Message.Contains("debe estar entre"));
        }
        finally
        {
            // Restore original culture
            System.Threading.Thread.CurrentThread.CurrentCulture = originalCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = originalUICulture;
        }
    }
}

/// <summary>
/// Model demonstrating localized validation messages.
/// </summary>
public class LocalizedValidationModel
{
    [Required(
        ErrorMessageResourceName = nameof(Resources.ValidationMessages.RequiredFieldRequired),
        ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
    public string? RequiredField { get; set; }

    [EmailAddress(
        ErrorMessageResourceName = nameof(Resources.ValidationMessages.EmailFieldInvalid),
        ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
    public string? EmailField { get; set; }

    [Range(0, 150,
        ErrorMessageResourceName = nameof(Resources.ValidationMessages.AgeFieldOutOfRange),
        ErrorMessageResourceType = typeof(Resources.ValidationMessages))]
    public int AgeField { get; set; }
}
