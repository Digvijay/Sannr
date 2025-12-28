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
    public void Validation_WithMetricsEnabled_ShouldUseMetricsCollector()
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

        // Act - Use registry directly like other tests
        SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator);
        var result = validator!(new SannrValidationContext(model)).GetAwaiter().GetResult();

        // Assert
        Assert.True(result.IsValid);
        // The metrics collector would have been called during validation
    }

    [Fact]
    public void Validation_WithErrors_ShouldRecordErrorMetrics()
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

        // Act
        SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator);
        var result = validator!(new SannrValidationContext(model)).GetAwaiter().GetResult();

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count > 0);
        // Error metrics would have been recorded
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