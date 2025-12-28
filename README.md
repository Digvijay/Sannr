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

## � OpenAPI/Swagger Integration

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
| `[RequiredIf]` | **Conditional:** Required only if another property matches a value. |
| `[Sanitize]` | **Mutation:** Trims, Uppercases, or Lowercases input. |
| `[CustomValidator]` | Points to static sync or async methods. |
| `IValidatableObject` | **Model-level:** Cross-property business rules. |

---

## 🤝 Contributing

Sannr is open-source. We welcome contributions to expand the standard validator set and optimize regex generation patterns.

**License**: MIT
