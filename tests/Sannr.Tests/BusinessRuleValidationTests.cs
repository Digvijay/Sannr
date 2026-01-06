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
using Xunit;

namespace Sannr.Tests;

/// <summary>
/// Test models for business rule validators.
/// </summary>
public partial class OrderModel
{
    [Required]
    public string? CustomerId { get; set; }

    [FutureDate]
    public DateTime DeliveryDate { get; set; }

    [AllowedValues("USD", "EUR", "GBP")]
    public string? Currency { get; set; }

    [ConditionalRange("Currency", "USD", 1, 1000)]
    public decimal Amount { get; set; }
}

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

public partial class AppointmentModel
{
    [Required]
    public string? CustomerName { get; set; }

    [FutureDate]
    public DateTime AppointmentDate { get; set; }

    [AllowedValues("pending", "confirmed", "cancelled", "completed")]
    public string? Status { get; set; }

    [ConditionalRange("Status", "confirmed", 30, 480)] // 30 minutes to 8 hours
    public int? DurationMinutes { get; set; }
}

/// <summary>
/// Tests for built-in business rule validators.
/// </summary>
public partial class BusinessRuleValidationTests
{
    [Fact]
    public void FutureDateAttribute_ShouldBeDefined()
    {
        // Arrange
        var attribute = new FutureDateAttribute();

        // Act & Assert
        Assert.IsType<FutureDateAttribute>(attribute);
        Assert.IsAssignableFrom<SannrValidationAttribute>(attribute);
    }

    [Fact]
    public void AllowedValuesAttribute_ShouldBeDefined()
    {
        // Arrange
        var attribute = new AllowedValuesAttribute("test1", "test2");

        // Act & Assert
        Assert.IsType<AllowedValuesAttribute>(attribute);
        Assert.IsAssignableFrom<SannrValidationAttribute>(attribute);
        Assert.Equal(new[] { "test1", "test2" }, attribute.Values);
    }

    [Fact]
    public void ConditionalRangeAttribute_ShouldBeDefined()
    {
        // Arrange
        var attribute = new ConditionalRangeAttribute("OtherProperty", "targetValue", 1.0, 100.0);

        // Act & Assert
        Assert.IsType<ConditionalRangeAttribute>(attribute);
        Assert.IsAssignableFrom<SannrValidationAttribute>(attribute);
        Assert.Equal("OtherProperty", attribute.OtherProperty);
        Assert.Equal("targetValue", attribute.TargetValue);
        Assert.Equal(1.0, attribute.Minimum);
        Assert.Equal(100.0, attribute.Maximum);
    }

    [Fact]
    public void OrderModel_ShouldHaveFutureDateAttribute()
    {
        // Arrange
        var deliveryDateProp = typeof(OrderModel).GetProperty("DeliveryDate");

        // Act & Assert
        Assert.NotNull(deliveryDateProp);
        var attributes = deliveryDateProp!.GetCustomAttributes(typeof(FutureDateAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void OrderModel_ShouldHaveAllowedValuesAttribute()
    {
        // Arrange
        var currencyProp = typeof(OrderModel).GetProperty("Currency");

        // Act & Assert
        Assert.NotNull(currencyProp);
        var attributes = currencyProp!.GetCustomAttributes(typeof(AllowedValuesAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void OrderModel_ShouldHaveConditionalRangeAttribute()
    {
        // Arrange
        var amountProp = typeof(OrderModel).GetProperty("Amount");

        // Act & Assert
        Assert.NotNull(amountProp);
        var attributes = amountProp!.GetCustomAttributes(typeof(ConditionalRangeAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void ProductModel_ShouldHaveAllowedValuesAttribute()
    {
        // Arrange
        var statusProp = typeof(ProductModel).GetProperty("Status");

        // Act & Assert
        Assert.NotNull(statusProp);
        var attributes = statusProp!.GetCustomAttributes(typeof(AllowedValuesAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void ProductModel_ShouldHaveFutureDateAttribute()
    {
        // Arrange
        var releaseDateProp = typeof(ProductModel).GetProperty("ReleaseDate");

        // Act & Assert
        Assert.NotNull(releaseDateProp);
        var attributes = releaseDateProp!.GetCustomAttributes(typeof(FutureDateAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void ProductModel_ShouldHaveConditionalRangeAttribute()
    {
        // Arrange
        var priceProp = typeof(ProductModel).GetProperty("Price");

        // Act & Assert
        Assert.NotNull(priceProp);
        var attributes = priceProp!.GetCustomAttributes(typeof(ConditionalRangeAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void AppointmentModel_ShouldHaveFutureDateAttribute()
    {
        // Arrange
        var appointmentDateProp = typeof(AppointmentModel).GetProperty("AppointmentDate");

        // Act & Assert
        Assert.NotNull(appointmentDateProp);
        var attributes = appointmentDateProp!.GetCustomAttributes(typeof(FutureDateAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void AppointmentModel_ShouldHaveAllowedValuesAttribute()
    {
        // Arrange
        var statusProp = typeof(AppointmentModel).GetProperty("Status");

        // Act & Assert
        Assert.NotNull(statusProp);
        var attributes = statusProp!.GetCustomAttributes(typeof(AllowedValuesAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void AppointmentModel_ShouldHaveConditionalRangeAttribute()
    {
        // Arrange
        var durationProp = typeof(AppointmentModel).GetProperty("DurationMinutes");

        // Act & Assert
        Assert.NotNull(durationProp);
        var attributes = durationProp!.GetCustomAttributes(typeof(ConditionalRangeAttribute), false);
        Assert.Single(attributes);
    }

    [Fact]
    public void AllowedValuesAttribute_ShouldStoreValuesCorrectly()
    {
        // Arrange
        var attribute = new AllowedValuesAttribute("USD", "EUR", "GBP", "JPY");

        // Act & Assert
        Assert.Equal(new[] { "USD", "EUR", "GBP", "JPY" }, attribute.Values);
    }

    [Fact]
    public void ConditionalRangeAttribute_ShouldStorePropertiesCorrectly()
    {
        // Arrange
        var attribute = new ConditionalRangeAttribute("Currency", "USD", 10.0, 5000.0);

        // Act & Assert
        Assert.Equal("Currency", attribute.OtherProperty);
        Assert.Equal("USD", attribute.TargetValue);
        Assert.Equal(10.0, attribute.Minimum);
        Assert.Equal(5000.0, attribute.Maximum);
    }

    [Fact]
    public void FutureDateAttribute_ShouldInheritFromSannrValidationAttribute()
    {
        // Arrange
        var attribute = new FutureDateAttribute();

        // Act & Assert
        Assert.IsAssignableFrom<SannrValidationAttribute>(attribute);
    }

    [Fact]
    public void AllowedValuesAttribute_ShouldInheritFromSannrValidationAttribute()
    {
        // Arrange
        var attribute = new AllowedValuesAttribute("test");

        // Act & Assert
        Assert.IsAssignableFrom<SannrValidationAttribute>(attribute);
    }

    [Fact]
    public void ConditionalRangeAttribute_ShouldInheritFromSannrValidationAttribute()
    {
        // Arrange
        var attribute = new ConditionalRangeAttribute("prop", "value", 1, 10);

        // Act & Assert
        Assert.IsAssignableFrom<SannrValidationAttribute>(attribute);
    }
}
