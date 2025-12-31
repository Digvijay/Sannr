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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Sannr;
using Sannr.AspNetCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Sannr.Tests;

/// <summary>
/// Test models for Minimal API integration testing.
/// </summary>
public class MinimalApiTestModel
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, StringLength(50, MinimumLength = 2)]
    public string? Name { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }
}

public class SimpleTestModel
{
    [Required]
    public string? Value { get; set; }
}

/// <summary>
/// Comprehensive test suite for Minimal API integration with Validated<T> wrapper.
/// Tests verify automatic validation, error handling, and HTTP response generation.
/// </summary>
public class MinimalApiIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public MinimalApiIntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddSannrValidators();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ValidatedT_WithValidModel_ProvidesAccessToValue()
    {
        // Arrange
        var validModel = new MinimalApiTestModel
        {
            Email = "test@example.com",
            Name = "John Doe",
            Age = 30
        };

        // Act
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(validModel, _serviceProvider);

        // Assert
        Assert.True(validated.IsValid);
        Assert.Equal(validModel.Email, validated.Value.Email);
        Assert.Equal(validModel.Name, validated.Value.Name);
        Assert.Equal(validModel.Age, validated.Value.Age);
    }

    [Fact]
    public async Task ValidatedT_WithInvalidModel_IsInvalid()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel
        {
            Email = "invalid-email", // Invalid email
            Name = "", // Required but empty
            Age = 150 // Over maximum
        };

        // Act
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Assert
        Assert.False(validated.IsValid);
        Assert.NotNull(validated.Errors);
        Assert.True(validated.Errors.Count > 0);
    }

    [Fact]
    public async Task ValidatedT_WithNullModel_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Validated<MinimalApiTestModel>.CreateAsync(null!, _serviceProvider));
    }

    [Fact]
    public async Task ValidatedT_WithNullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var model = new MinimalApiTestModel();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Validated<MinimalApiTestModel>.CreateAsync(model, null!));
    }

    [Fact]
    public async Task ValidatedT_ValueProperty_ThrowsWhenInvalid()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel { Email = "invalid" };
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = validated.Value);
    }

    [Fact]
    public async Task ValidatedT_ValueProperty_ReturnsModelWhenValid()
    {
        // Arrange
        var validModel = new MinimalApiTestModel
        {
            Email = "test@example.com",
            Name = "Valid Name",
            Age = 25
        };
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(validModel, _serviceProvider);

        // Act
        var result = validated.Value;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(validModel.Email, result.Email);
    }

    [Fact]
    public async Task ValidatedT_ErrorsProperty_ContainsValidationErrors()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel
        {
            Email = "not-an-email",
            Name = "",
            Age = -5
        };
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Act
        var errors = validated.Errors;

        // Assert
        Assert.NotNull(errors);
        Assert.True(errors.Count >= 2); // At least email and name errors
        Assert.Contains(errors, e => e.MemberName == "Email");
        Assert.Contains(errors, e => e.MemberName == "Name");
    }

    [Fact]
    public async Task ValidatedT_WithModelWithoutValidationAttributes_IsValid()
    {
        // Arrange
        var model = new EmptyModel();

        // Act
        var validated = await Validated<EmptyModel>.CreateAsync(model, _serviceProvider);

        // Assert
        Assert.True(validated.IsValid);
        Assert.NotNull(validated.Value);
        Assert.Empty(validated.Errors);
    }

    [Fact]
    public async Task ValidatedT_WithSimpleValidModel_IsValid()
    {
        // Arrange
        var model = new SimpleTestModel { Value = "test" };

        // Act
        var validated = await Validated<SimpleTestModel>.CreateAsync(model, _serviceProvider);

        // Assert
        Assert.True(validated.IsValid);
        Assert.Equal("test", validated.Value.Value);
    }

    [Fact]
    public async Task ValidatedT_WithSimpleInvalidModel_IsInvalid()
    {
        // Arrange
        var model = new SimpleTestModel { Value = null };

        // Act
        var validated = await Validated<SimpleTestModel>.CreateAsync(model, _serviceProvider);

        // Assert
        Assert.False(validated.IsValid);
        Assert.Contains(validated.Errors, e => e.MemberName == "Value");
    }

    [Fact]
    public async Task ValidatedT_ToValidationProblemDetails_ConvertsCorrectly()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel { Email = "invalid" };
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Act
        var problemDetails = validated.ToValidationProblemDetails();

        // Assert
        Assert.NotNull(problemDetails);
        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("One or more validation errors occurred.", problemDetails.Title);
        Assert.NotNull(problemDetails.Errors);
        Assert.True(problemDetails.Errors.Count > 0);
    }

    [Fact]
    public async Task ValidatedT_ToValidationProblemDetails_WithCustomTitle_Works()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel { Email = "invalid" };
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Act
        var problemDetails = validated.ToValidationProblemDetails("Custom validation failed");

        // Assert
        Assert.Equal("Custom validation failed", problemDetails.Title);
    }

    [Fact]
    public async Task ValidatedT_ToBadRequestResult_CreatesCorrectResponse()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel { Email = "invalid" };
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Act
        var result = validated.ToBadRequestResult();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
    }

    [Fact]
    public async Task ValidatedT_ToBadRequestResult_WithCustomTitle_Works()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel { Email = "invalid" };
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Act
        var result = validated.ToBadRequestResult("Custom error");

        // Assert
        var badRequestResult = (BadRequestObjectResult)result;
        var problemDetails = (ValidationProblemDetails)badRequestResult.Value!;
        Assert.Equal("Custom error", problemDetails.Title);
    }

    [Fact]
    public async Task ValidatedT_IsValid_TrueForValidModel()
    {
        // Arrange
        var validModel = new MinimalApiTestModel
        {
            Email = "test@example.com",
            Name = "Valid Name",
            Age = 30
        };

        // Act
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(validModel, _serviceProvider);

        // Assert
        Assert.True(validated.IsValid);
    }

    [Fact]
    public async Task ValidatedT_IsValid_FalseForInvalidModel()
    {
        // Arrange
        var invalidModel = new MinimalApiTestModel { Email = "invalid" };

        // Act
        var validated = await Validated<MinimalApiTestModel>.CreateAsync(invalidModel, _serviceProvider);

        // Assert
        Assert.False(validated.IsValid);
    }
}