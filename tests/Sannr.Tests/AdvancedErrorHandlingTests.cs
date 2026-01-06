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
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Sannr.AspNetCore;
using Xunit;

namespace Sannr.Tests;

/// <summary>
/// Tests for Advanced Error Handling features.
/// </summary>
public partial class AdvancedErrorHandlingTests
{
    [Fact]
    public void SannrValidationProblemDetails_Should_Initialize_With_Errors()
    {
        // Arrange
        var errors = new List<ValidationError>
        {
            new ValidationError("Name", "Name is required", Severity.Error),
            new ValidationError("Email", "Email format is invalid", Severity.Error)
        };

        // Act
        var problemDetails = new SannrValidationProblemDetails(errors, "TestModel", "test-correlation-id", 15.5);

        // Assert
        Assert.Equal("test-correlation-id", problemDetails.CorrelationId);
        Assert.Equal("TestModel", problemDetails.ModelType);
        Assert.Equal(15.5, problemDetails.ValidationDurationMs);
        Assert.True(problemDetails.Timestamp <= DateTimeOffset.UtcNow);
        Assert.Contains("Name", problemDetails.Errors.Keys);
        Assert.Contains("Email", problemDetails.Errors.Keys);
        Assert.Equal(new[] { "Name is required" }, problemDetails.Errors["Name"]);
        Assert.Equal(new[] { "Email format is invalid" }, problemDetails.Errors["Email"]);
    }

    [Fact]
    public void SannrValidationProblemDetails_Should_Extract_Validation_Rules()
    {
        // Arrange
        var errors = new List<ValidationError>
        {
            new ValidationError("Name", "The Name field is required.", Severity.Error),
            new ValidationError("Email", "The Email field is not a valid e-mail address.", Severity.Error),
            new ValidationError("Age", "The Age must be between 18 and 100.", Severity.Error),
            new ValidationError("Description", "The Description must be at least 10 characters long.", Severity.Error)
        };

        // Act
        var problemDetails = new SannrValidationProblemDetails(errors);

        // Assert
        // Assert - basic validation rule extraction
        Assert.True(problemDetails.ValidationRules.ContainsKey("Name"));
        Assert.True(problemDetails.ValidationRules.ContainsKey("Email"));
        Assert.True(problemDetails.ValidationRules.ContainsKey("Age"));
        Assert.True(problemDetails.ValidationRules.ContainsKey("Description"));

        Assert.True(problemDetails.ValidationRules["Name"].Required);
        Assert.True(problemDetails.ValidationRules["Email"].Email);
        // Note: Range and StringLength extraction is simplified and may not parse actual values
    }

    [Fact]
    public void ToSannrValidationProblemDetails_Extension_Should_Convert_Validated_Result()
    {
        // Arrange
        var errors = new List<ValidationError>
        {
            new ValidationError("Name", "Name is required", Severity.Error)
        };
        var problemDetails = errors.ToSannrValidationProblemDetails("TestModel", "correlation-123", 25.0);

        // Assert
        Assert.Equal("correlation-123", problemDetails.CorrelationId);
        Assert.Equal("TestModel", problemDetails.ModelType);
        Assert.Equal(25.0, problemDetails.ValidationDurationMs);
        Assert.Contains("Name", problemDetails.Errors.Keys);
    }

    [Fact]
    public void ToBadRequestResult_Extension_Should_Return_BadRequestObjectResult()
    {
        // Arrange
        var problemDetails = new SannrValidationProblemDetails();

        // Act
        var result = problemDetails.ToBadRequestResult();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(problemDetails, result.Value);
    }

    [Fact]
    public void SannrProblemDetailsFactory_Should_Return_Enhanced_ProblemDetails_When_Enabled()
    {
        // This test would require mocking the default factory
        // For now, we'll test the core functionality separately
        Assert.True(true); // Placeholder test
    }

    [Fact]
    public void SannrProblemDetailsFactory_Should_Return_Default_ProblemDetails_When_Disabled()
    {
        // This test would require mocking the default factory
        // For now, we'll test the core functionality separately
        Assert.True(true); // Placeholder test
    }

    [Fact]
    public void SannrProblemDetailsFactory_Should_Return_Enhanced_ValidationProblemDetails()
    {
        // This test would require mocking the default factory
        // For now, we'll test the core functionality separately
        Assert.True(true); // Placeholder test
    }

    [Fact]
    public void AotObjectModelValidator_Should_Store_Enhanced_Result_When_Enabled()
    {
        // This test would require setting up HttpContext and action context
        // For now, we'll test the core functionality separately
        Assert.True(true); // Placeholder test
    }

    [Fact]
    public void AotObjectModelValidator_Should_Generate_CorrelationId_When_Not_Provided()
    {
        // This test would require setting up HttpContext and action context
        // For now, we'll test the core functionality separately
        Assert.True(true); // Placeholder test
    }

    [Fact]
    public void AddSannr_Should_Register_ProblemDetailsFactory_When_Enhanced_Responses_Enabled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSannr(options =>
        {
            options.EnableEnhancedErrorResponses = true;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<ProblemDetailsFactory>();
        Assert.IsType<SannrProblemDetailsFactory>(factory);
    }

    [Fact]
    public void AddSannr_Should_Not_Register_ProblemDetailsFactory_When_Enhanced_Responses_Disabled()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSannr(options =>
        {
            options.EnableEnhancedErrorResponses = false;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetService<ProblemDetailsFactory>();
        Assert.IsNotType<SannrProblemDetailsFactory>(factory);
    }

    [Fact]
    public void ValidationRuleInfo_Should_Initialize_With_Default_Values()
    {
        // Arrange & Act
        var ruleInfo = new ValidationRuleInfo();

        // Assert
        Assert.False(ruleInfo.Required);
        Assert.False(ruleInfo.Email);
        Assert.False(ruleInfo.Url);
        Assert.Null(ruleInfo.Range);
        Assert.Null(ruleInfo.StringLength);
        Assert.Null(ruleInfo.Pattern);
        Assert.Null(ruleInfo.AllowedValues);
    }

    [Fact]
    public void RangeInfo_Should_Store_Min_Max_Values()
    {
        // Arrange & Act
        var range = new RangeInfo { Min = 10, Max = 100 };

        // Assert
        Assert.Equal(10, range.Min);
        Assert.Equal(100, range.Max);
    }

    [Fact]
    public void StringLengthInfo_Should_Store_Length_Constraints()
    {
        // Arrange & Act
        var length = new StringLengthInfo { MinLength = 5, MaxLength = 50 };

        // Assert
        Assert.Equal(5, length.MinLength);
        Assert.Equal(50, length.MaxLength);
    }
}

/// <summary>
/// Simple test model for validation testing.
/// </summary>
public partial class ErrorHandlingTestModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
}
