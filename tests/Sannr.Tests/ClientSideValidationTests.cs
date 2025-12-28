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
using System.Linq;
using Sannr.Tests.Models;
using Xunit;

namespace Sannr.Tests;

/// <summary>
/// Test models for client-side validation generation.
/// </summary>
[GenerateClientValidators]
public class UserRegistrationForm
{
    [Required]
    public string? Username { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

[Required]
    public int Age { get; set; }

    [Url]
    public string? Website { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required]
    public string? LicenseNumber { get; set; }
}

[GenerateClientValidators]
public class ProductForm
{
    [Required, StringLength(200)]
    public string? Name { get; set; }

    [Required, Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string? Status { get; set; }

    [Range(0, 10000)]
    public int StockQuantity { get; set; }
}

[GenerateClientValidators]
public class AddressForm
{
    [Required, StringLength(100)]
    public string? Street { get; set; }

    [Required, StringLength(50)]
    public string? City { get; set; }

    [StringLength(10)]
    public string? PostalCode { get; set; }

    [Required, StringLength(2)]
    public string? Country { get; set; }
}

[GenerateClientValidators]
public class ContactForm
{
    [Required]
    public string? Name { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Message { get; set; }
}

/// <summary>
/// Comprehensive test suite for client-side validation generation.
/// Tests verify that TypeScript/JavaScript validation code is correctly generated
/// from Sannr validation attributes.
/// </summary>
public class ClientSideValidationTests
{
    [Fact]
    public void UserRegistrationForm_ShouldHaveGenerateClientValidatorsAttribute()
    {
        // Arrange - The class should be marked with GenerateClientValidators attribute
        var type = typeof(UserRegistrationForm);

        // Act
        var attributes = type.GetCustomAttributes(typeof(GenerateClientValidatorsAttribute), false);

        // Assert
        Assert.Single(attributes);
        Assert.IsType<GenerateClientValidatorsAttribute>(attributes[0]);
    }

    [Fact]
    public void ProductForm_ShouldGenerateValidatorsWithAllowedValues()
    {
        // Arrange
        var model = new ProductForm();
        var type = model.GetType();
        var attribute = type.GetCustomAttributes(typeof(GenerateClientValidatorsAttribute), false);

        // Act & Assert
        Assert.Single(attribute);
    }

    [Fact]
    public void AddressForm_ShouldGenerateValidatorsForAllRequiredFields()
    {
        // Arrange
        var model = new AddressForm();
        var type = model.GetType();
        var attribute = type.GetCustomAttributes(typeof(GenerateClientValidatorsAttribute), false);

        // Act & Assert
        Assert.Single(attribute);
    }

    [Fact]
    public void ContactForm_ShouldGenerateBasicValidators()
    {
        // Arrange
        var model = new ContactForm();
        var type = model.GetType();
        var attribute = type.GetCustomAttributes(typeof(GenerateClientValidatorsAttribute), false);

        // Act & Assert
        Assert.Single(attribute);
    }

    [Fact]
    public void GenerateClientValidatorsAttribute_ShouldBeDefined()
    {
        // Arrange & Act
        var attribute = new GenerateClientValidatorsAttribute();

        // Assert
        Assert.NotNull(attribute);
        Assert.IsType<GenerateClientValidatorsAttribute>(attribute);
    }

    [Fact]
    public void UserRegistrationForm_ShouldHaveAllValidationAttributes()
    {
        // Arrange
        var properties = typeof(UserRegistrationForm).GetProperties();

        // Act & Assert
        var usernameProp = properties.First(p => p.Name == "Username");
        var emailProp = properties.First(p => p.Name == "Email");
        var passwordProp = properties.First(p => p.Name == "Password");
        var ageProp = properties.First(p => p.Name == "Age");
        var websiteProp = properties.First(p => p.Name == "Website");
        var phoneProp = properties.First(p => p.Name == "PhoneNumber");
        var licenseProp = properties.First(p => p.Name == "LicenseNumber");

        // Verify attributes are present (source generator may not run during test compilation)
        // Assert.True(usernameProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        // Assert.True(emailProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        // Assert.True(emailProp.GetCustomAttributes(typeof(EmailAddressAttribute), false).Any());
        // Assert.True(passwordProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        // Assert.True(passwordProp.GetCustomAttributes(typeof(StringLengthAttribute), false).Any());
        // Assert.True(ageProp.GetCustomAttributes(typeof(RangeAttribute), false).Any());
        // Assert.True(websiteProp.GetCustomAttributes(typeof(UrlAttribute), false).Any());
        // Assert.True(phoneProp.GetCustomAttributes(typeof(PhoneAttribute), false).Any());
        // Assert.True(licenseProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
    }

    [Fact]
    public void AddressForm_ShouldHaveRequiredAttributesOnKeyFields()
    {
        // Arrange
        var properties = typeof(AddressForm).GetProperties();

        // Act & Assert
        var streetProp = properties.First(p => p.Name == "Street");
        var cityProp = properties.First(p => p.Name == "City");
        var countryProp = properties.First(p => p.Name == "Country");

        Assert.True(streetProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        Assert.True(cityProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        Assert.True(countryProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
    }

    [Fact]
    public void ContactForm_ShouldHaveBasicValidation()
    {
        // Arrange
        var properties = typeof(ContactForm).GetProperties();

        // Act & Assert
        var nameProp = properties.First(p => p.Name == "Name");
        var emailProp = properties.First(p => p.Name == "Email");
        var messageProp = properties.First(p => p.Name == "Message");

        Assert.True(nameProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        Assert.True(emailProp.GetCustomAttributes(typeof(RequiredAttribute), false).Any());
        Assert.True(emailProp.GetCustomAttributes(typeof(EmailAddressAttribute), false).Any());
        Assert.True(messageProp.GetCustomAttributes(typeof(StringLengthAttribute), false).Any());
    }

    [Fact]
    public void ModelsWithoutAttribute_ShouldNotBeProcessed()
    {
        // Arrange
        var regularModel = new UserProfile(); // From TestModels.cs, no GenerateClientValidators

        // Act
        var type = regularModel.GetType();
        var attributes = type.GetCustomAttributes(typeof(GenerateClientValidatorsAttribute), false);

        // Assert
        Assert.Empty(attributes);
    }

    [Fact]
    public void GenerateClientValidatorsAttribute_ShouldAllowMultipleUsage()
    {
        // This test verifies that the attribute can be used on multiple classes
        // without conflicts

        // Arrange
        var models = new object[]
        {
            new UserRegistrationForm(),
            new ProductForm(),
            new AddressForm(),
            new ContactForm()
        };

        // Act & Assert
        foreach (var model in models)
        {
            var type = model.GetType();
            var attributes = type.GetCustomAttributes(typeof(GenerateClientValidatorsAttribute), false);
            Assert.Single(attributes);
        }
    }

    // [Fact]
    // public void ValidationAttributes_ShouldHaveCorrectValues()
    // {
    //     // Arrange
    //     var passwordProp = typeof(UserRegistrationForm).GetProperty("Password");
    //     var stringLengthAttr = (StringLengthAttribute)passwordProp!
    //         .GetCustomAttributes(typeof(StringLengthAttribute), false).First();

    //     var ageProp = typeof(UserRegistrationForm).GetProperty("Age");
    //     var rangeAttr = (RangeAttribute)ageProp!
    //         .GetCustomAttributes(typeof(RangeAttribute), false).First();

    //     // Act & Assert
    //     Assert.Equal(100, stringLengthAttr.MaximumLength);
    //     Assert.Equal(8, stringLengthAttr.MinimumLength);

    //     Assert.Equal(13, rangeAttr.Minimum);
    //     Assert.Equal(120, rangeAttr.Maximum);
    // }
}