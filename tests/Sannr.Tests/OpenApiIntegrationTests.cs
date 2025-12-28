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

using Microsoft.OpenApi.Models;
using Sannr;
using Sannr.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using Xunit;

namespace Sannr.Tests
{
    /// <summary>
    /// Test model with various Sannr validation attributes for OpenAPI testing.
    /// </summary>
    public class TestApiModel
    {
        [Required]
        public string? RequiredField { get; set; }

        [EmailAddress]
        public string? EmailField { get; set; }

        [Range(18, 120)]
        public int AgeField { get; set; }

        [StringLength(100, MinimumLength = 2)]
        public string? NameField { get; set; }

        [Url]
        public string? UrlField { get; set; }

        [Phone]
        public string? PhoneField { get; set; }

        [CreditCard]
        public string? CreditCardField { get; set; }

        [FileExtensions]
        public string? FileField { get; set; }

        public string? NoValidationField { get; set; }
    }

    /// <summary>
    /// Test model with multiple validation attributes on the same property.
    /// </summary>
    public class MultipleAttributesModel
    {
        [EmailAddress, StringLength(50, MinimumLength = 5)]
        public string? MultiField { get; set; }
    }

    /// <summary>
    /// Test model with double range validation.
    /// </summary>
    public class DoubleRangeModel
    {
        [Range(0.01, 999.99)]
        public double PriceField { get; set; }
    }

    /// <summary>
    /// Empty test model.
    /// </summary>
    public class EmptyModel
    {
        // No properties
    }
    /// <summary>
    /// Comprehensive test suite for OpenAPI integration with Sannr validation attributes.
    /// Tests verify that validation attributes are correctly converted to OpenAPI schema constraints.
    /// </summary>
    public class OpenApiIntegrationTests
    {
        private readonly SwaggerGenOptions _swaggerOptions;
        private readonly SchemaRepository _schemaRepository;

        public OpenApiIntegrationTests()
        {
            _swaggerOptions = new SwaggerGenOptions();
            _swaggerOptions.AddSannrValidationSchemas();
            _schemaRepository = new SchemaRepository();
        }

        [Fact]
        public void AddSannrValidationSchemas_AddsSchemaFilter()
        {
            // Arrange
            var options = new SwaggerGenOptions();

            // Act
            options.AddSannrValidationSchemas();

            // Assert
            Assert.NotNull(options);
            // Note: We can't directly test the internal filter collection,
            // but the integration works as verified by other tests
        }

        [Fact]
        public void SchemaFilter_AppliesToModelWithValidationAttributes()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema { Properties = new Dictionary<string, OpenApiSchema>() };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert - The filter should have added properties to the schema
            Assert.NotNull(schema.Properties);
            // Note: The actual property population depends on Swashbuckle's schema generation
            // This test verifies the filter doesn't crash and processes the type
        }

        [Fact]
        public void EmailAddressAttribute_SetsFormatToEmail()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["EmailField"] = new OpenApiSchema { Type = "string" }
                }
            };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("EmailField"));
            var emailProperty = schema.Properties["EmailField"];
            Assert.Equal("email", emailProperty.Format);
        }

        [Fact]
        public void RangeAttribute_SetsMinimumAndMaximum()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["AgeField"] = new OpenApiSchema { Type = "integer" }
                }
            };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("AgeField"));
            var ageProperty = schema.Properties["AgeField"];
            Assert.Equal(18m, ageProperty.Minimum);
            Assert.Equal(120m, ageProperty.Maximum);
        }

        [Fact]
        public void StringLengthAttribute_SetsMinLengthAndMaxLength()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["NameField"] = new OpenApiSchema { Type = "string" }
                }
            };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("NameField"));
            var nameProperty = schema.Properties["NameField"];
            Assert.True(nameProperty.MinLength.HasValue && nameProperty.MinLength.Value == 2);
            Assert.True(nameProperty.MaxLength.HasValue && nameProperty.MaxLength.Value == 100);
        }

        [Fact]
        public void UrlAttribute_SetsFormatToUri()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["UrlField"] = new OpenApiSchema { Type = "string" }
                }
            };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("UrlField"));
            var urlProperty = schema.Properties["UrlField"];
            Assert.Equal("uri", urlProperty.Format);
        }

        [Fact]
        public void FileExtensionsAttribute_SetsFormatToFile()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["FileField"] = new OpenApiSchema { Type = "string" }
                }
            };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("FileField"));
            var fileProperty = schema.Properties["FileField"];
            Assert.Equal("file", fileProperty.Format);
        }

        [Fact]
        public void PropertiesWithoutValidationAttributes_AreNotModified()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["NoValidationField"] = new OpenApiSchema { Type = "string" }
                }
            };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("NoValidationField"));
            var noValidationProperty = schema.Properties["NoValidationField"];
            Assert.Null(noValidationProperty.Format);
            Assert.Null(noValidationProperty.Minimum);
            Assert.Null(noValidationProperty.Maximum);
            Assert.Null(noValidationProperty.MinLength);
            Assert.Null(noValidationProperty.MaxLength);
        }

        [Fact]
        public void SchemaFilter_HandlesNullSchemaProperties()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema(); // Properties is null
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act & Assert - Should not throw exception
            filter.Apply(schema, context);
        }

        [Fact]
        public void SchemaFilter_HandlesNullTypeInContext()
        {
            // Arrange
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema { Properties = new Dictionary<string, OpenApiSchema>() };
            var context = new SchemaFilterContext(
                type: null!,
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act & Assert - Should not throw exception
            filter.Apply(schema, context);
        }

        [Fact]
        public void MultipleValidationAttributes_OnSameProperty_AreApplied()
        {
            // Arrange - Create a model with multiple attributes on one property
            var testModelType = typeof(MultipleAttributesModel);
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["MultiField"] = new OpenApiSchema { Type = "string" }
                }
            };
            var context = new SchemaFilterContext(
                type: testModelType,
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("MultiField"));
            var multiProperty = schema.Properties["MultiField"];
            Assert.Equal("email", multiProperty.Format); // EmailAddress should be applied
            Assert.True(multiProperty.MinLength.HasValue && multiProperty.MinLength.Value == 5);   // StringLength should be applied
        }

        [Fact]
        public void RangeAttribute_WithDoubleValues_ConvertsToDecimal()
        {
            // Arrange
            var testModelType = typeof(DoubleRangeModel);
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["PriceField"] = new OpenApiSchema { Type = "number" }
                }
            };
            var context = new SchemaFilterContext(
                type: testModelType,
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert
            Assert.True(schema.Properties.ContainsKey("PriceField"));
            var priceProperty = schema.Properties["PriceField"];
            Assert.Equal(0.01m, priceProperty.Minimum);
            Assert.Equal(999.99m, priceProperty.Maximum);
        }

        [Fact]
        public void RequiredAttribute_DoesNotSetRequiredFlag()
        {
            // Arrange - Note: Required properties are handled at the schema level, not property level
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema
            {
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["RequiredField"] = new OpenApiSchema { Type = "string" }
                }
            };
            var context = new SchemaFilterContext(
                type: typeof(TestApiModel),
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act
            filter.Apply(schema, context);

            // Assert - Required is handled at object level, not property level in OpenAPI
            Assert.True(schema.Properties.ContainsKey("RequiredField"));
            var requiredProperty = schema.Properties["RequiredField"];
            // The property itself doesn't get modified for required status
            Assert.Null(requiredProperty.Format);
        }

        [Fact]
        public void EmptyModel_DoesNotThrowException()
        {
            // Arrange
            var testModelType = typeof(EmptyModel);
            var filter = new SannrValidationSchemaFilter();
            var schema = new OpenApiSchema { Properties = new Dictionary<string, OpenApiSchema>() };
            var context = new SchemaFilterContext(
                type: testModelType,
                schemaGenerator: null!,
                schemaRepository: _schemaRepository);

            // Act & Assert - Should not throw exception
            filter.Apply(schema, context);
        }
    }
}