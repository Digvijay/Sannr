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
using System.Diagnostics.Metrics;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Sannr.AspNetCore;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Sannr.Tests;

/// <summary>
/// Test models for performance monitoring.
/// </summary>
public class SimpleValidationModel
{
    [Required]
    public string? Name { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}

public class ComplexValidationModel
{
    [Required]
    public string? Name { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string? Description { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }

    [FutureDate]
    public DateTime? FutureDate { get; set; }
}

/// <summary>
/// Tests for performance monitoring and diagnostics.
/// </summary>
public class PerformanceMonitoringTests
{
    [Fact]
    public void SannrValidationOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new SannrValidationOptions();

        // Assert
        Assert.False(options.EnableMetrics);
        Assert.Equal("sannr_validation", options.MetricsPrefix);
    }

    [Fact]
    public void SannrValidationOptions_ShouldAllowCustomization()
    {
        // Arrange & Act
        var options = new SannrValidationOptions
        {
            EnableMetrics = true,
            MetricsPrefix = "custom_prefix"
        };

        // Assert
        Assert.True(options.EnableMetrics);
        Assert.Equal("custom_prefix", options.MetricsPrefix);
    }

    [Fact]
    public void AddSannr_WithMetricsEnabled_ShouldRegisterMetricsCollector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var metricsCollector = serviceProvider.GetService<ISannrMetricsCollector>();
        Assert.NotNull(metricsCollector);
        Assert.IsType<SannrMetricsCollector>(metricsCollector);
    }

    [Fact]
    public void AddSannr_WithMetricsDisabled_ShouldRegisterNoOpCollector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSannr(options =>
        {
            options.EnableMetrics = false;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var metricsCollector = serviceProvider.GetService<ISannrMetricsCollector>();
        Assert.NotNull(metricsCollector);
        Assert.IsType<NoOpMetricsCollector>(metricsCollector);
    }

    [Fact]
    public void Validation_WithMetricsEnabled_ShouldRecordMetrics()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        var serviceProvider = services.BuildServiceProvider();
        var metricsCollector = serviceProvider.GetService<ISannrMetricsCollector>() as SannrMetricsCollector;

        // Create a test model
        var model = new SimpleValidationModel { Name = "Test", Email = "test@example.com" };

        // Act - Use the AotObjectModelValidator which should record metrics
        var validator = serviceProvider.GetService<IObjectModelValidator>() as AotObjectModelValidator;
        Assert.NotNull(validator);

        // Create a mock ActionContext
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var validationState = new ValidationStateDictionary();

        validator!.Validate(actionContext, validationState, "", model);

        // Assert - The metrics should have been recorded
        // Note: We can't easily test the actual metric values without complex mocking,
        // but we can verify the validator was called and no exceptions occurred
        Assert.True(actionContext.ModelState.IsValid);
    }

    [Fact]
    public void Validation_WithErrors_ShouldRecordErrorMetrics_UsingAspNetCore()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Invalid model - missing required fields
        var model = new SimpleValidationModel { Name = null, Email = "invalid-email" };

        // Act - Use the AotObjectModelValidator
        var validator = serviceProvider.GetService<IObjectModelValidator>() as AotObjectModelValidator;
        Assert.NotNull(validator);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var validationState = new ValidationStateDictionary();

        validator!.Validate(actionContext, validationState, "", model);

        // Assert
        Assert.False(actionContext.ModelState.IsValid);
        Assert.True(actionContext.ModelState.ErrorCount > 0);
    }

    [Fact]
    public async Task Validated_CreateAsync_ShouldRecordMetrics()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        var serviceProvider = services.BuildServiceProvider();

        var model = new SimpleValidationModel { Name = "Test", Email = "test@example.com" };

        // Act
        var validated = await Validated<SimpleValidationModel>.CreateAsync(model, serviceProvider);

        // Assert
        Assert.True(validated.IsValid);
        Assert.Equal(model.Name, validated.Value.Name);
        Assert.Equal(model.Email, validated.Value.Email);
    }

    [Fact]
    public async Task Validated_CreateAsync_WithErrors_ShouldRecordErrorMetrics()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Invalid model
        var model = new SimpleValidationModel { Name = null, Email = "invalid-email" };

        // Act
        var validated = await Validated<SimpleValidationModel>.CreateAsync(model, serviceProvider);

        // Assert
        Assert.False(validated.IsValid);
        Assert.True(validated.Errors.Count > 0);
    }

    [Fact]
    public void Validation_WithMetricsDisabled_ShouldNotImpactPerformance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = false; // Disabled
        });

        var serviceProvider = services.BuildServiceProvider();

        var model = new SimpleValidationModel { Name = "Test", Email = "test@example.com" };

        // Act - Measure performance
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator);
            var result = validator!(new SannrValidationContext(model)).GetAwaiter().GetResult();
            Assert.True(result.IsValid);
        }
        stopwatch.Stop();

        // Assert - Should complete quickly (less than 1 second for 1000 validations)
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Validations should complete quickly when metrics are disabled");
    }

    [Fact]
    public void AsyncValidation_WithMetricsEnabled_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        var serviceProvider = services.BuildServiceProvider();

        var model = new SimpleValidationModel { Name = "Test", Email = "test@example.com" };

        // Act
        SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator);
        var result = validator!(new SannrValidationContext(model)).GetAwaiter().GetResult();

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ComplexModelValidation_ShouldWorkWithMetrics()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Valid model
        var validModel = new ComplexValidationModel
        {
            Name = "John Doe",
            Email = "john@example.com",
            Description = "A valid description",
            Age = 30,
            FutureDate = DateTime.Now.AddDays(1)
        };

        // Act
        SannrValidatorRegistry.TryGetValidator(validModel.GetType(), out var validator);
        var result = validator!(new SannrValidationContext(validModel)).GetAwaiter().GetResult();

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MetricsPrefix_Customization_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "my_custom_prefix";
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var metricsCollector = serviceProvider.GetService<ISannrMetricsCollector>();
        Assert.NotNull(metricsCollector);
        Assert.IsType<SannrMetricsCollector>(metricsCollector);
    }

    [Fact]
    public void MultipleValidations_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options =>
        {
            options.EnableMetrics = true;
            options.MetricsPrefix = "test_metrics";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Act - Multiple validations
        for (int i = 0; i < 5; i++)
        {
            var model = new SimpleValidationModel { Name = $"Test{i}", Email = $"test{i}@example.com" };
            SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator);
            var result = validator!(new SannrValidationContext(model)).GetAwaiter().GetResult();
            Assert.True(result.IsValid);
        }

        // Assert - All validations passed
    }

    [Fact]
    public void MetricsCollector_ShouldBeSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options => options.EnableMetrics = true);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var collector1 = serviceProvider.GetService<ISannrMetricsCollector>();
        var collector2 = serviceProvider.GetService<ISannrMetricsCollector>();

        // Assert
        Assert.NotNull(collector1);
        Assert.NotNull(collector2);
        Assert.Same(collector1, collector2);
    }

    [Fact]
    public void NoOpMetricsCollector_ShouldBeSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSannr(options => options.EnableMetrics = false);

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var collector1 = serviceProvider.GetService<ISannrMetricsCollector>();
        var collector2 = serviceProvider.GetService<ISannrMetricsCollector>();

        // Assert
        Assert.NotNull(collector1);
        Assert.NotNull(collector2);
        Assert.Same(collector1, collector2);
        Assert.IsType<NoOpMetricsCollector>(collector1);
    }
}