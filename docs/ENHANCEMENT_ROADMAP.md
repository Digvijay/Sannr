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

### 2. **Minimal API Integration** ✅ IMPLEMENTED
Enhanced support for .NET Minimal APIs with automatic validation and error handling through the `Validated<T>` wrapper.

```csharp
// Before: Manual validation boilerplate
app.MapPost("/users", async (CreateUserRequest request, SannrValidator<CreateUserRequest> validator) =>
{
    var result = await validator.ValidateAsync(request);
    if (!result.IsValid)
        return Results.ValidationProblem(result.ToProblemDetails());

    // Handle valid request
});

// After: Clean, automatic validation
app.MapPost("/users", async (Validated<CreateUserRequest> request) =>
{
    if (!request.IsValid)
    {
        return request.ToBadRequestResult(); // Automatic 400 with validation errors
    }

    // request.Value is guaranteed to be valid
    var user = request.Value;
    // Handle valid request...
});
```

**Benefits:**
- Eliminates validation boilerplate in Minimal API endpoints
- Strongly-typed access to validated data
- Consistent error responses across all endpoints
- Seamless integration with OpenAPI schema generation

### 3. **Client-Side Validation** ✅ IMPLEMENTED
Generate JSON validation rules from server-side attributes for seamless client-side validation.

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

// Generated C# class with JSON rules
public static class UserFormValidators
{
    public const string ValidationRulesJson = @"{
  ""name"": { ""required"": true, ""maxLength"": 100 },
  ""email"": { ""email"": true }
}";
}
```

**Benefits:**
- Single source of truth for client and server validation
- Framework-agnostic JSON format works with any JavaScript validation library
- Compile-time generation ensures type safety
- Zero runtime overhead for rule generation

### 4. **Built-in Business Rule Validators** ✅ IMPLEMENTED
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

**Features Implemented:**
- `[FutureDate]` - Ensures dates are in the future
- `[AllowedValues]` - Restricts properties to predefined string values
- `[ConditionalRange]` - Applies numeric ranges based on other property conditions
- Full client-side JSON rule generation
- Comprehensive test coverage (15+ test cases)
- Complete documentation and examples

**Benefits:**
- Covers 80% of common business validation scenarios
- Zero runtime overhead with AOT compilation
- Automatic client-side validation rule generation
- Type-safe attribute usage with compile-time validation

### 5. **Performance Monitoring & Diagnostics** ✅ IMPLEMENTED
Built-in metrics and performance monitoring for validation operations.

```csharp
// Automatic metrics collection
services.AddSannr(options =>
{
    options.EnableMetrics = true;
    options.MetricsPrefix = "myapp_validation";
});

// Metrics available:
// - {prefix}_validation_duration (histogram in ms)
// - {prefix}_validation_errors_total (counter)
```

**Features Implemented:**
- Automatic metrics collection using `System.Diagnostics.Metrics`
- Validation duration tracking with histograms
- Validation error counting with counters
- Model type tagging for detailed analysis
- Zero overhead when disabled
- Full AOT compatibility
- Integration with Prometheus, Application Insights, and custom monitoring systems
- Comprehensive performance characteristics documentation

**Benefits:**
- Production-ready observability with minimal configuration
- Performance bottleneck identification
- SLA monitoring and alerting capabilities
- Capacity planning insights
- Zero runtime overhead when metrics are disabled

### 6. **Visual Studio Integration**
Enhanced IDE experience with better IntelliSense and code analysis.

- **Code Snippets**: Quick insertion of common validation patterns
- **Live Analysis**: Real-time validation rule checking
- **Refactoring Support**: Safe renaming of validation properties
- **Code Generation**: Generate validators from existing models

### 7. **Advanced Error Handling** ✅ IMPLEMENTED
Structured error responses with problem details, correlation IDs, and validation rule metadata.

```csharp
// Enhanced error responses with correlation IDs and validation metadata
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "correlationId": "550e8400-e29b-41d4-a716-446655440000",
    "modelType": "CreateUserRequest",
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

**Configuration:**
```csharp
builder.Services.AddSannr(options =>
{
    options.EnableEnhancedErrorResponses = true;
    options.IncludeValidationRuleMetadata = true;
    options.IncludeValidationDuration = false; // Optional
});
```

**Features:**
- **Correlation IDs**: Automatic generation or extraction from `X-Correlation-ID` header
- **Validation Rule Metadata**: Structured information about applied validation rules
- **Model Type Information**: Identifies which model type was validated
- **Timestamps**: When validation occurred
- **Performance Metrics**: Optional validation duration tracking
- **Problem Details**: RFC 7807 compliant error responses

### 6. **Testing Utilities** ❌ NOT IMPLEMENTED
Comprehensive testing helpers for validation scenarios.

**Status**: Not implemented due to AOT compatibility requirements. Testing utilities would require reflection to dynamically generate test data, which violates Sannr's commitment to pure AOT compatibility.

**Alternative**: Use standard xUnit data attributes or manual test data generation for testing Sannr validation.

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

### Phase 1 (High Impact, Low Effort) ✅ COMPLETED
1. **OpenAPI/Swagger Integration** - Immediate value for API developers
2. **Minimal API Integration** - Essential for modern .NET apps
3. **Client-Side Validation** - Single source of truth for validation rules

### Phase 2 (Medium Impact, Medium Effort)
4. **Built-in Business Rule Validators** ✅ IMPLEMENTED - Addresses common validation needs
5. **Performance Monitoring & Diagnostics** ✅ IMPLEMENTED - Production readiness
6. **Testing Utilities** ❌ NOT IMPLEMENTED - Requires reflection, conflicts with AOT goals

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