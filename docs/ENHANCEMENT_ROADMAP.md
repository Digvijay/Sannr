# Sannr Enhancement Roadmap

## 🚀 High-Impact Enhancements

### 1. **OpenAPI/Swagger Integration** ✅ IMPLEMENTED
Generate OpenAPI schemas directly from Sannr validation attributes, eliminating the need for separate validation documentation.

```csharp
// Setup in Program.cs
builder.Services.AddSwaggerGen(options =>
{
    options.AddSannrValidationSchemas(); // Automatically generates schemas from Sannr attributes
});

// Model with Sannr validation attributes
public class CreateUserRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Url]
    public string Website { get; set; }
}

// Generated OpenAPI schema automatically includes:
// - email format validation
// - age range constraints (18-120)
// - name length constraints (2-100 characters)
// - website URL format validation
```

**Benefits:**
- Single source of truth for validation rules
- Automatic API documentation updates
- Reduced boilerplate code
- Consistent validation across runtime and documentation

### 2. **Minimal API Integration**
Enhanced support for .NET Minimal APIs with automatic validation and error handling.

```csharp
// Current approach
app.MapPost("/users", async (CreateUserRequest request, SannrValidator<CreateUserRequest> validator) =>
{
    var result = await validator.ValidateAsync(request);
    if (!result.IsValid)
        return Results.ValidationProblem(result.ToProblemDetails());

    // Handle valid request
});

// Future: Automatic validation
app.MapPost("/users", async (Validated<CreateUserRequest> request) =>
{
    // request.Value is guaranteed to be valid
    // Automatic 400 responses for invalid data
});
```

### 3. **Client-Side Validation**
Generate TypeScript/JavaScript validation code for Blazor and SPA applications.

```csharp
// Generate client-side validators
[GenerateClientValidators]
public class UserForm
{
    [Required, StringLength(100)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }
}

// Generated TypeScript
export const userFormValidators = {
    name: { required: true, maxLength: 100 },
    email: { email: true }
};
```

### 4. **Built-in Business Rule Validators**
Common business validation patterns as built-in attributes.

```csharp
public class OrderModel
{
    [Required]
    public string CustomerId { get; set; }

    [FutureDate] // New built-in validator
    public DateTime DeliveryDate { get; set; }

    [AllowedValues("USD", "EUR", "GBP")] // New built-in validator
    public string Currency { get; set; }

    [ConditionalRange("Currency", "USD", 1, 1000)] // New conditional validator
    public decimal Amount { get; set; }
}
```

### 5. **Performance Monitoring & Diagnostics**
Built-in metrics and performance monitoring for validation operations.

```csharp
// Automatic metrics collection
services.AddSannrValidation(options =>
{
    options.EnableMetrics = true;
    options.MetricsPrefix = "sannr_validation";
});

// Metrics available:
// - sannr_validation_duration
// - sannr_validation_errors_total
// - sannr_validation_cache_hit_ratio
```

### 6. **Visual Studio Integration**
Enhanced IDE experience with better IntelliSense and code analysis.

- **Code Snippets**: Quick insertion of common validation patterns
- **Live Analysis**: Real-time validation rule checking
- **Refactoring Support**: Safe renaming of validation properties
- **Code Generation**: Generate validators from existing models

### 7. **Advanced Error Handling**
Structured error responses with problem details and correlation IDs.

```csharp
// Enhanced error responses
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "00-1234567890abcdef1234567890abcdef-1234567890abcdef-00",
    "errors": {
        "email": ["The email field is required."],
        "age": ["Age must be between 18 and 120."]
    },
    "validationRules": {
        "email": { "required": true, "email": true },
        "age": { "range": { "min": 18, "max": 120 } }
    }
}
```

### 8. **Testing Utilities**
Comprehensive testing helpers for validation scenarios.

```csharp
// Enhanced testing support
[Theory]
[ValidModelData(typeof(UserModel))] // Generates valid test data
public void ValidUserModel_PassesValidation(UserModel model)
{
    var result = model.Validate();
    Assert.True(result.IsValid);
}

[Theory]
[InvalidModelData(typeof(UserModel), "Email")] // Generates invalid test data
public void InvalidEmail_FailsValidation(UserModel model)
{
    var result = model.Validate();
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "Email");
}
```

### 9. **Migration Tools**
Easy migration from other validation libraries.

```bash
# CLI tool for migration
dotnet sannr migrate --from fluent-validation --project MyProject.csproj
dotnet sannr migrate --from data-annotations --output-dir Migrations/
```

### 10. **Advanced Source Generators**
Generate related code artifacts from validation models.

```csharp
// Generate DTOs, mappers, and contracts
[GenerateDto]
[GenerateMapper]
[GenerateOpenApiContract]
public class UserModel
{
    [Required, StringLength(100)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }
}

// Auto-generates:
// - UserModelDto
// - UserModelMapper
// - OpenAPI schema
```

## 🎯 Implementation Priority

### Phase 1 (High Impact, Low Effort)
1. **OpenAPI/Swagger Integration** - Immediate value for API developers
2. **Minimal API Integration** - Essential for modern .NET apps
3. **Built-in Business Rule Validators** - Addresses common validation needs

### Phase 2 (Medium Impact, Medium Effort)
4. **Client-Side Validation** - Better developer experience
5. **Performance Monitoring** - Production readiness
6. **Testing Utilities** - Improved testing workflow

### Phase 3 (High Impact, High Effort)
7. **Visual Studio Integration** - Premium developer experience
8. **Advanced Source Generators** - Comprehensive code generation
9. **Migration Tools** - Easier adoption

## 💡 Why These Enhancements Matter

These features address the core pain points that prevent developers from adopting new validation libraries:

- **Reduced Boilerplate**: Less manual code for schemas, client validation, testing
- **Better DX**: Enhanced IDE support, automatic generation, migration tools
- **Production Ready**: Monitoring, error handling, performance optimization
- **Ecosystem Integration**: Works seamlessly with modern .NET tooling and frameworks

Each enhancement builds on Sannr's core strength (source generation) while expanding its utility across the entire application development lifecycle.