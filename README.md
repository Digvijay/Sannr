# Sannr

[![NuGet](https://img.shields.io/nuget/v/Sannr.svg)](https://www.nuget.org/packages/Sannr)
[![Build Status](https://github.com/microsoft/sannr/actions/workflows/build.yml/badge.svg)](https://github.com/microsoft/sannr/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Native AOT](https://img.shields.io/badge/Native%20AOT-Compatible-green)](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
[![.NET 8 LTS](https://img.shields.io/badge/.NET-8.0%20LTS-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

**The Enterprise-Grade, AOT-First Validation Engine for .NET.**

Sannr is a high-performance validation library designed to replace `System.ComponentModel.DataAnnotations` in modern cloud-native applications. By utilizing **Roslyn Source Generators**, Sannr moves validation logic from runtime reflection to compile-time C#, resulting in zero startup overhead and complete compatibility with **Native AOT** trimming.



It extends the standard validation paradigm with enterprise requirements: **Async Validation**, **Conditional Logic**, **Auto-Sanitization**, and **Validation Groups**, while maintaining a familiar API.

---

## 🚀 Why Sannr?

Standard validation libraries rely on Reflection, which is slow, memory-intensive, and hostile to the IL Trimmer. Sannr generates highly optimized static code that looks exactly like code you would write by hand.

| Feature | System.ComponentModel.DataAnnotations | **Sannr** |
| :--- | :--- | :--- |
| **Runtime Mechanism** | Reflection (Slow) | **Static C# (Instant)** |
| **Native AOT** | ⚠️ Requires warnings/trimming | **✅ 100% Trimming Safe** |
| **Async Support** | ❌ Synchronous Only | **✅ Native `Task<T>`** |
| **Dependency Injection** | ❌ Service Locator Anti-Pattern | **✅ `IServiceProvider` Support** |
| **Conditional Logic** | ❌ Custom implementation required | **✅ `[RequiredIf]` Built-in** |
| **Sanitization** | ❌ Manual code in Controllers | **✅ `[Sanitize]` Built-in** |
| **OpenAPI Integration** | ❌ Manual schema definitions | **✅ Auto-generated schemas** |
| **Minimal API Support** | ❌ Manual validation boilerplate | **✅ `Validated<T>` wrapper** |
| **Client-Side Validation** | ❌ Manual JavaScript/TypeScript | **✅ Auto-generated JSON rules** |
| **Model-Level Validation** | ✅ `IValidatableObject` | **✅ `Sannr.IValidatableObject`** |

---

## 📦 Installation

```bash
dotnet add package Sannr
```

*Note: Sannr includes both the runtime library and the source generator analyzer.*

---

## ⚡ Quick Start

### 1. Register the Service
In your ASP.NET Core `Program.cs`, add Sannr. This automatically replaces the default `IObjectModelValidator`.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Registers the Sannr AOT Validator Adapter
builder.Services.AddSannr(); 

builder.Services.AddControllers();
var app = builder.Build();
```

### 2. Define Your Model
Sannr uses standard attributes familiar to .NET developers, plus powerful extensions.

```csharp
using Sannr; // Replaces System.ComponentModel.DataAnnotations

public class UserProfile
{
    // Auto-Sanitization: Trims whitespace and Uppercases input before validation
    [Sanitize(Trim = true, ToUpper = true)]
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    // Conditional Validation: ZipCode is only required if Country is USA
    public string Country { get; set; }

    [RequiredIf(nameof(Country), "USA")]
    public string ZipCode { get; set; }
}
```

That's it. No manual validator registration is required. Sannr uses **Module Initializers** to automatically register your models at startup.

---

## 🛡️ Enterprise Capabilities

### 1. Asynchronous & Database Validation
Validating data against external sources (Databases, APIs) requires async. Sannr handles this natively.

```csharp
public class UserRegistration
{
    [CustomValidator(typeof(UserRules), nameof(UserRules.IsUniqueAsync), IsAsync = true)]
    public string Email { get; set; }
}

public static class UserRules
{
    // Services are injected automatically from the HttpContext
    public static async Task<ValidationResult> IsUniqueAsync(string email, IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        bool exists = await db.Users.AnyAsync(u => u.Email == email);
        
        return exists 
            ? ValidationResult.Error("Email is already taken.") 
            : ValidationResult.Success();
    }
}
```

### 2. Validation Groups (Scenarios)
Apply rules only in specific contexts (e.g., Registration vs. Update) without creating duplicate DTOs.

```csharp
public class Product
{
    [Required] // Always required
    public string Name { get; set; }

    // Only required during "Creation"
    [Required(Group = "Creation")]
    public string InitialStock { get; set; }
}

// Usage in Code:
validator.ValidateAsync(model, group: "Creation");
```

### 3. Severity Levels
Not all failures should block a request. Sannr supports `Info`, `Warning`, and `Error`.

```csharp
[StringLength(20, MinimumLength = 12, Severity = Severity.Warning, ErrorMessage = "Password is weak.")]
public string Password { get; set; }
```

### 4. Localization & Formatting
Sannr fully supports `.resx` localization resources and string formatting, checked at compile-time.

```csharp
[Display(Name = "User Age")]
[Range(18, 100, ErrorMessage = "The field {0} must be between {1} and {2}.")]
public int Age { get; set; }
// Error Output: "The field User Age must be between 18 and 100."
```

### 5. Model-Level Validation
Sannr provides `IValidatableObject` for cross-property business rules that can't be expressed with attributes.

```csharp
public class EmploymentModel : Sannr.IValidatableObject
{
    [Required]
    public string? Name { get; set; }
    
    public bool IsEmployed { get; set; }
    
    [Range(0, 1000000)]
    public decimal? Salary { get; set; }

    public IEnumerable<Sannr.ModelValidationResult> Validate(SannrValidationContext context)
    {
        if (IsEmployed && (!Salary.HasValue || Salary.Value <= 0))
        {
            yield return new Sannr.ModelValidationResult
            {
                MemberName = nameof(Salary),
                Message = "Salary required when employed"
            };
        }
    }
}
```

---

## 🔍 Advanced Error Handling

Sannr provides structured, enterprise-grade error responses with correlation IDs, validation rule metadata, and RFC 7807 Problem Details compliance.

### Configuration

```csharp
builder.Services.AddSannr(options =>
{
    options.EnableEnhancedErrorResponses = true;
    options.IncludeValidationRuleMetadata = true;
    options.IncludeValidationDuration = false;
});
```

### Enhanced Error Responses

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "modelType": "UserRegistration",
    "timestamp": "2024-01-15T10:30:00.0000000Z",
    "validationDurationMs": 15.5,
    "errors": {
        "Email": ["The Email field is required."],
        "Age": ["The Age must be between 18 and 120."]
    },
    "validationRules": {
        "Email": { "required": true, "email": true },
        "Age": { "range": { "min": 18, "max": 120 } }
    }
}
```

**Features:**
- **Correlation IDs**: Automatic generation or extraction from `X-Correlation-ID` header
- **Validation Rule Metadata**: Structured information about applied validation rules
- **Model Type Information**: Identifies which model type was validated
- **Timestamps**: When validation occurred
- **Performance Metrics**: Optional validation duration tracking
- **Problem Details**: RFC 7807 compliant error responses

---

## 📋 OpenAPI/Swagger Integration

Sannr automatically generates OpenAPI schema constraints from your validation attributes, ensuring your API documentation stays in sync with your validation rules.

### Setup

```csharp
// In Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.AddSannrValidationSchemas(); // ✨ One line to enable!
});
```

### Automatic Schema Generation

```csharp
public class CreateUserRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Url]
    public string? Website { get; set; }
}
```

**Generated OpenAPI Schema:**
```json
{
  "CreateUserRequest": {
    "type": "object",
    "properties": {
      "email": {
        "type": "string",
        "format": "email"
      },
      "age": {
        "type": "integer",
        "minimum": 18,
        "maximum": 120
      },
      "name": {
        "type": "string",
        "minLength": 2,
        "maxLength": 100
      },
      "website": {
        "type": "string",
        "format": "uri"
      }
    }
  }
}
```

### Supported Mappings

| Sannr Attribute | OpenAPI Schema |
| :--- | :--- |
| `[EmailAddress]` | `"format": "email"` |
| `[Url]` | `"format": "uri"` |
| `[Range(min, max)]` | `"minimum": min, "maximum": max` |
| `[StringLength(max, min)]` | `"minLength": min, "maxLength": max` |
| `[FileExtensions]` | `"format": "file"` |

**Benefits:**
- ✅ **Single Source of Truth**: Validation rules automatically become API documentation
- ✅ **Always Up-to-Date**: Schema updates when validation attributes change
- ✅ **No Boilerplate**: Eliminates manual schema definitions
- ✅ **Type Safety**: Compile-time validation of attribute usage

📖 **[Complete OpenAPI Integration Guide](docs/OPENAPI_INTEGRATION.md)**

---
## ⚡ Minimal API Integration

Sannr provides seamless integration with ASP.NET Core Minimal APIs through the `Validated<T>` wrapper, which automatically handles validation and returns appropriate HTTP responses.

**Before (Manual Validation):**
```csharp
app.MapPost("/users", async (CreateUserRequest request) =>
{
    if (!ModelState.IsValid)
    {
        return Results.ValidationProblem(ModelState);
    }
    // Process request...
});
```

**After (with Sannr):**
```csharp
app.MapPost("/users", async (Validated<CreateUserRequest> request) =>
{
    if (!request.IsValid)
    {
        return request.ToBadRequestResult();
    }
    // Process request...
});
```

**Key Benefits:**
- ✅ **Clean Code**: Eliminates boilerplate validation logic
- ✅ **Type Safety**: Strongly-typed access to validated data
- ✅ **Consistent Errors**: Standardized validation error responses
- ✅ **OpenAPI Integration**: Automatic schema generation with validation constraints

📖 **[Complete Minimal API Integration Guide](docs/MINIMAL_API_INTEGRATION.md)**

---
## 🌐 Client-Side Validation

Sannr automatically generates JSON validation rules from your server-side attributes, enabling seamless client-side validation in JavaScript/TypeScript applications.

### Setup

Mark your model classes with the `[GenerateClientValidators]` attribute:

```csharp
using Sannr;

[GenerateClientValidators]
public class UserRegistrationForm
{
    [Required]
    public string? Username { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    [Range(13, 120)]
    public int Age { get; set; }
}
```

### Generated Output

Sannr generates a static class with JSON validation rules:

```csharp
public static class UserRegistrationFormValidators
{
    public const string ValidationRulesJson = @"{
  ""username"": { ""required"": true },
  ""email"": { ""required"": true, ""email"": true },
  ""password"": { ""required"": true, ""minLength"": 8, ""maxLength"": 100 },
  ""age"": { ""min"": 13, ""max"": 120 }
}";
}
```

### Client-Side Usage

Use the generated JSON in your JavaScript/TypeScript validation:

```javascript
// Fetch validation rules from your API
const rules = JSON.parse(UserRegistrationFormValidators.ValidationRulesJson);

// Apply to your form validation library
const validator = new FormValidator(rules);
```

### Supported Mappings

| Sannr Attribute | Generated JSON Rule |
| :--- | :--- |
| `[Required]` | `"required": true` |
| `[EmailAddress]` | `"email": true` |
| `[StringLength(max, min)]` | `"minLength": min, "maxLength": max` |
| `[Range(min, max)]` | `"min": min, "max": max` |
| `[RegularExpression(pattern)]` | `"pattern": "pattern"` |
| `[Url]` | `"url": true` |
| `[Phone]` | `"phone": true` |
| `[FutureDate]` | `"futureDate": true` |
| `[AllowedValues]` | `"allowedValues": ["value1", "value2"]` |
| `[ConditionalRange]` | `"minRange": min, "maxRange": max, "conditionProperty": "prop", "conditionValue": value` |

**Benefits:**
- ✅ **Single Source of Truth**: Client and server validation rules stay synchronized
- ✅ **Type Safety**: Compile-time generation prevents typos
- ✅ **Framework Agnostic**: Works with any JavaScript validation library
- ✅ **Zero Runtime Cost**: Validation rules generated at compile-time

📖 **[Complete Client-Side Validation Guide](docs/CLIENT_SIDE_VALIDATION.md)**

---## 🏢 Built-in Business Rule Validators

Sannr includes common business validation patterns out-of-the-box, eliminating the need for custom validators in most enterprise scenarios.

### Future Date Validation

Ensures a date property is in the future (useful for delivery dates, appointments, etc.).

```csharp
public class Order
{
    [Required]
    public string CustomerId { get; set; }

    [FutureDate]
    public DateTime DeliveryDate { get; set; }
}
```

### Allowed Values Validation

Restricts a property to a predefined set of values (enums without the enum overhead).

```csharp
public class Product
{
    [Required]
    public string Name { get; set; }

    [AllowedValues("active", "inactive", "discontinued")]
    public string Status { get; set; }
}
```

### Conditional Range Validation

Applies numeric range validation only when another property matches a specific value.

```csharp
public class LoanApplication
{
    [Required]
    public string LoanType { get; set; } // "personal", "business", "mortgage"

    [ConditionalRange("LoanType", "personal", 1000, 50000)]
    [ConditionalRange("LoanType", "business", 5000, 1000000)]
    [ConditionalRange("LoanType", "mortgage", 50000, 5000000)]
    public decimal Amount { get; set; }
}
```

### Client-Side Support

All business rule validators generate appropriate client-side JSON rules:

```json
{
  "deliveryDate": { "futureDate": true },
  "status": { "allowedValues": ["active", "inactive", "discontinued"] },
  "amount": { 
    "minRange": 1000, 
    "maxRange": 50000, 
    "conditionProperty": "loanType", 
    "conditionValue": "personal" 
  }
}
```

**Benefits:**
- ✅ **Common Patterns**: Covers 80% of business validation scenarios
- ✅ **Type Safe**: Compile-time validation of attribute usage
- ✅ **Client-Side Ready**: Automatic JSON rule generation
- ✅ **Performance**: Zero runtime overhead with AOT compilation

📖 **[Complete Business Rule Validators Guide](docs/BUSINESS_RULE_VALIDATORS.md)**

---
## 📊 Performance Monitoring & Diagnostics

Sannr provides built-in performance monitoring and diagnostics to help you track validation performance in production environments.

### Setup

Enable metrics collection in your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Enable performance monitoring
builder.Services.AddSannr(options =>
{
    options.EnableMetrics = true;
    options.MetricsPrefix = "myapp_validation"; // Optional: customize metric names
});

builder.Services.AddControllers();
var app = builder.Build();
```

### Available Metrics

Sannr automatically collects the following metrics using `System.Diagnostics.Metrics`:

#### Validation Duration
- **Name**: `{prefix}_validation_duration`
- **Type**: Histogram (milliseconds)
- **Description**: Duration of validation operations
- **Tags**: `model_type` (the name of the validated model class)

#### Validation Errors
- **Name**: `{prefix}_validation_errors_total`
- **Type**: Counter
- **Description**: Total number of validation errors encountered
- **Tags**: `model_type` (the name of the validated model class)

### Integration with Monitoring Systems

#### Prometheus (via OpenTelemetry)

```csharp
// Add OpenTelemetry metrics export
builder.Services.AddOpenTelemetry()
    .WithMetrics(builder => builder
        .AddMeter("myapp_validation") // Matches your MetricsPrefix
        .AddPrometheusExporter());
```

#### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
{
    module.IncludeDiagnosticSourceActivities.Add("System.Diagnostics.Metrics");
});
```

### Performance Characteristics

- **Zero Overhead When Disabled**: Metrics collection is completely bypassed when `EnableMetrics = false`
- **Minimal Overhead When Enabled**: Uses efficient `System.Diagnostics.Metrics` APIs
- **AOT Compatible**: No dynamic code generation or reflection
- **Thread Safe**: All metric recording is thread-safe

### Best Practices

1. **Use Descriptive Prefixes**: Choose metric prefixes that match your application naming conventions
2. **Monitor in Production**: Enable metrics in production to track performance trends
3. **Set Up Alerts**: Alert on significant increases in validation duration or error rates
4. **Profile Performance**: Use metrics to identify models with complex validation logic
5. **Resource Monitoring**: Correlate validation metrics with CPU/memory usage

📖 **[Complete Performance Monitoring Guide](docs/PERFORMANCE_MONITORING.md)**

---
## �🔧 Architecture

When you compile your project, Sannr generates a static class for every model marked with validation attributes.

**Input Code:**
```csharp
public class Login { [Required] public string User { get; set; } }
```

**Generated AOT Code (Simplified):**
```csharp
public static class LoginValidator 
{
    public static async Task<ValidationResult> ValidateAsync(SannrValidationContext ctx) 
    {
        var model = (Login)ctx.ObjectInstance;
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(model.User)) {
             result.Add("User", "User is required.", Severity.Error);
        }
        return result;
    }
}
```

This ensures **zero allocations** for metadata lookups and **maximum throughput**.

---

## 📋 Supported Attributes

| Attribute | Function |
| :--- | :--- |
| `[Required]` | Checks for null or whitespace. |
| `[StringLength]` | Validates min/max string length. |
| `[Range]` | Validates numeric ranges. |
| `[EmailAddress]` | Validates email format (Regex compiled). |
| `[CreditCard]` | Validates credit card format (Luhn check planned). |
| `[Url]`, `[Phone]` | Validates URL and Phone formats. |
| `[FileExtensions]` | Validates file extensions (e.g., `.png,.jpg`). |
| `[FutureDate]` | **Business Rule:** Ensures date is in the future. |
| `[AllowedValues]` | **Business Rule:** Restricts to predefined values. |
| `[ConditionalRange]` | **Business Rule:** Range validation based on another property. |
| `[RequiredIf]` | **Conditional:** Required only if another property matches a value. |
| `[Sanitize]` | **Mutation:** Trims, Uppercases, or Lowercases input. |
| `[CustomValidator]` | Points to static sync or async methods. |
| `IValidatableObject` | **Model-level:** Cross-property business rules. |

---

## 🤝 Contributing

Sannr is open-source. We welcome contributions to expand the standard validator set and optimize regex generation patterns.

**License**: MIT
