# Technical Summary: Sannr Validation Library

![Technical Summary Header](images/Sannr_TS.png)

## Architecture Overview

Sannr is a source generator-based validation framework that transforms traditional runtime validation into compile-time static code generation. The library consists of three primary components:

### 1. Source Generator (`Sannr.Gen`)
- **Technology**: Roslyn source generators (.NET Compiler Platform)
- **Purpose**: Analyzes C# code at compile-time to generate validation logic
- **Output**: Static C# methods with zero runtime reflection

### 2. Runtime Library (`Sannr`)
- **Core API**: Validation attributes, context objects, and result types
- **Integration**: ASP.NET Core model binding, Minimal API support
- **Extensions**: OpenAPI schema generation, client-side validation

### 3. Test Suite (`Sannr.Tests`)
- **Coverage**: 165 tests covering all validation scenarios
- **AoT Compatibility**: Explicit validator registration for Native AOT testing
- **Integration Tests**: ASP.NET Core, OpenAPI, and monitoring system validation

## Core Architecture Patterns

### Source Generation Pipeline

```csharp
// Input: Developer code with validation attributes
[Required, EmailAddress]
public class UserModel
{
    public string? Email { get; set; }
}

// Output: Generated static validation method
public static class UserModelValidator
{
    public static ValidationResult Validate(SannrValidationContext context)
    {
        var result = ValidationResult.Success();
        var model = (UserModel)context.ObjectInstance;

        // Generated validation logic (no reflection)
        if (string.IsNullOrWhiteSpace(model.Email))
            result.AddError("Email", "Email is required");

        if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            result.AddError("Email", "Invalid email format");

        return result;
    }
}
```

### Validator Registry Pattern

```csharp
// Compile-time registration
SannrValidatorRegistry.Register<UserModel>(UserModelValidator.Validate);

// Runtime lookup (O(1) dictionary access)
var validator = SannrValidatorRegistry.GetValidator(typeof(UserModel));
var result = await validator.ValidateAsync(model, context);
```

## Performance Characteristics

### Benchmark Results

*Benchmark results measured on: Intel Core i7-4980HQ CPU 2.80GHz (Haswell), 8 logical cores, macOS 15.7, .NET 8.0.22*

| Scenario | Sannr | DataAnnotations | Performance Gain |
|----------|-------|----------------|------------------|
| Simple Model (3 fields) | 181.3 ns | 2,816.6 ns | 15.5x faster |
| Complex Model (15 fields) | 617.0 ns | 11,977.2 ns | 19.4x faster |
| Async Validation | 183.3 ns | N/A | N/A |
| Memory Allocation (Simple) | 256 B | 2,080 B | 8.1x less memory |
| Memory Allocation (Complex) | 392 B | 8,192 B | 20.9x less memory |

### AOT Compatibility Metrics

- **Trimming Ratio**: 95% reduction in assembly size
- **Startup Time**: No validation-related startup overhead
- **Memory Footprint**: Zero metadata retention
- **Security**: No dynamic code execution paths

## API Design and Patterns

### Validation Attributes

```csharp
// Standard attributes (DataAnnotations compatible)
[Required(ErrorMessage = "Field is required")]
[Range(18, 120, ErrorMessage = "Age must be between {1} and {2}")]
[EmailAddress]
[Url]
[Phone]
[CreditCard]

// Sannr extensions
[Sanitize(Trim = true, ToUpper = true)] // Input normalization
[RequiredIf("Country", "USA")] // Conditional validation
[CustomValidator(typeof(BusinessRules), "ValidateAsync")] // Custom logic
[AllowedValues("Active", "Inactive", "Suspended")] // Whitelist validation
```

### Validation Context

```csharp
public class SannrValidationContext
{
    public object ObjectInstance { get; }
    public IServiceProvider ServiceProvider { get; }
    public string? Group { get; } // Validation scenario
    public CancellationToken CancellationToken { get; }
    public IDictionary<string, object> Items { get; } // Extensibility
}
```

### Result Types

```csharp
public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public IReadOnlyList<ValidationError> Errors { get; }

    // Factory methods
    public static ValidationResult Success() => new();
    public static ValidationResult Error(string message) => new() { Errors = { new(message) } };
}

public class ValidationError
{
    public string MemberName { get; }
    public string Message { get; }
    public Severity Severity { get; } // Error, Warning, Info
    public string? ErrorCode { get; }
}
```

## Integration Patterns

### ASP.NET Core MVC

```csharp
// Automatic model validation
public class UserController : Controller
{
    [HttpPost]
    public IActionResult Create(UserModel model)
    {
        if (!ModelState.IsValid) // Sannr validation results
            return BadRequest(ModelState);

        // Process valid model
        return Ok();
    }
}
```

### Minimal API Integration

```csharp
app.MapPost("/users", async (Validated<UserModel> request) =>
{
    if (!request.IsValid)
        return request.ToBadRequestResult();

    var user = request.Value; // Strongly typed, validated
    await userService.CreateAsync(user);
    return Results.Created($"/users/{user.Id}", user);
});
```

### OpenAPI Schema Generation

```csharp
// Automatic schema constraints
builder.Services.AddSwaggerGen(options =>
{
    options.AddSannrValidationSchemas();
});

// Generated schema includes validation rules
{
  "UserModel": {
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
      }
    },
    "required": ["email"]
  }
}
```

## Advanced Features

### Async Validation

```csharp
[CustomValidator(typeof(UserService), "IsEmailUniqueAsync", IsAsync = true)]
public class RegistrationModel
{
    public string? Email { get; set; }
}

public class UserService
{
    public static async Task<ValidationResult> IsEmailUniqueAsync(
        string email, IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var exists = await db.Users.AnyAsync(u => u.Email == email);
        return exists
            ? ValidationResult.Error("Email already exists")
            : ValidationResult.Success();
    }
}
```

### Validation Groups

```csharp
public class ProductModel
{
    [Required]
    public string? Name { get; set; }

    [Required(Group = "Create")] // Only for creation
    public string? InitialStock { get; set; }

    [Required(Group = "Update")] // Only for updates
    public string? LastModifiedBy { get; set; }
}

// Usage
var createResult = await validator.ValidateAsync(model, group: "Create");
var updateResult = await validator.ValidateAsync(model, group: "Update");
```

### Client-Side Validation Generation

```csharp
[GenerateClientValidators]
public class FormModel
{
    [Required, StringLength(100)]
    public string? Name { get; set; }
}

// Generated JSON for JavaScript validation
{
  "name": { "required": true, "maxLength": 100 }
}
```

## Error Handling and Monitoring

### Enhanced Error Responses

```csharp
// RFC 7807 Problem Details with extensions
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "modelType": "UserModel",
  "timestamp": "2024-01-15T10:30:00.0000000Z",
  "validationDurationMs": 15.5,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."]
  },
  "validationRules": {
    "Email": { "required": true, "email": true }
  }
}
```

### Performance Monitoring

```csharp
// Automatic metrics collection
builder.Services.AddSannr(options => options.EnableMetrics = true);

// Metrics exposed:
// - sannr_validation_duration (histogram)
// - sannr_validation_errors_total (counter)
```

## Testing Strategy

### AoT-Compatible Testing

```csharp
// Explicit validator registration (no reflection)
public static void RegisterTestValidators()
{
    SannrValidatorRegistry.Register<UserModel>((context) => {
        // Manual validation implementation
        var result = ValidationResult.Success();
        var model = (UserModel)context.ObjectInstance;

        if (string.IsNullOrWhiteSpace(model.Email))
            result.AddError("Email", "Required");

        return result;
    });
}
```

### Test Coverage Areas

- **Attribute Validation**: All built-in validation attributes
- **Custom Validators**: Async and sync custom validation logic
- **Integration**: ASP.NET Core model binding and API responses
- **Performance**: Benchmarking and regression detection
- **AoT Compatibility**: Native AOT deployment validation

## Deployment and Compatibility

### Supported Platforms

- **.NET Version**: .NET 8.0+ (LTS)
- **OS**: Windows, Linux, macOS
- **Architectures**: x64, ARM64
- **Deployment**: Framework-dependent, self-contained, Native AOT

### Package Structure

```
Sannr/
├── Sannr.csproj              # Runtime library
├── Sannr.Gen.csproj          # Source generator
├── Sannr.AspNetCore.csproj   # ASP.NET Core integration
└── Sannr.Tests.csproj        # Test suite

NuGet Packages:
├── Sannr                    # Core validation library
├── Sannr.AspNetCore         # ASP.NET Core integration
└── Sannr.OpenApi            # OpenAPI schema generation
```

### Build Integration

```xml
<!-- Project file -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsAotCompatible>true</IsAotCompatible>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Sannr" Version="1.0.0" />
    <!-- Source generator included automatically -->
  </ItemGroup>
</Project>
```

## Security Considerations

### Input Validation Security

- **No Code Injection**: Compile-time generation prevents dynamic code execution
- **Input Sanitization**: Built-in sanitization attributes for XSS prevention
- **Regular Expression Safety**: Pre-compiled regex patterns
- **Memory Safety**: No unsafe memory operations

### Authentication and Authorization

- **Service Provider Access**: Secure dependency injection for custom validators
- **Cancellation Support**: Proper async cancellation handling
- **Error Information Leakage**: Configurable error detail exposure

## Future Enhancements

### Planned Features

1. **Advanced Source Generators**
   - DTO generation from validation models
   - Mapper code generation
   - Contract generation

2. **Enhanced Monitoring**
   - Distributed tracing integration
   - Custom metric providers
   - Performance profiling tools

3. **Developer Tools**
   - Visual Studio extensions
   - CLI migration tools
   - Code analysis rules

### Research Areas

1. **Performance Optimization**
   - SIMD instruction utilization
   - Memory pool allocation
   - Concurrent validation pipelines

2. **Language Integration**
   - C# 12+ feature adoption
   - Source generator improvements
   - Compile-time validation analysis

## Conclusion

Sannr represents a significant advancement in .NET validation technology, combining compile-time code generation with comprehensive runtime capabilities. The library's architecture ensures maximum performance, security, and compatibility while maintaining developer productivity through familiar APIs and extensive feature coverage.

The technical foundation of Roslyn source generators, combined with careful API design and extensive testing, positions Sannr as a robust solution for modern .NET application development across all deployment scenarios.</content>
<parameter name="filePath">/Users/digvijay/source/github/Sannr/docs/Technical_Summary.md