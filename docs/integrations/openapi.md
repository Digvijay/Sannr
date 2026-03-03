# OpenAPI/Swagger Integration

Sannr provides seamless integration with Swashbuckle.AspNetCore, automatically converting your validation attributes into OpenAPI schema constraints at compile-time. This ensures your API documentation stays synchronized with your validation rules without any runtime reflection.

## Setup

### 1. Install Dependencies

Ensure you have Swashbuckle.AspNetCore installed:

```bash
dotnet add package Swashbuckle.AspNetCore
```

### 2. Configure SwaggerGen

In your `Program.cs`, register the Sannr schema filter:

```csharp
using Sannr.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
{
    // Register the compile-time-generated schema filter
    options.SchemaFilter<SannrGeneratedSchemaFilter>();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
```

### 3. Define Your Models

Models must be `partial` for the Sannr source generator to work. Forgetting `partial` will produce a build error (SANN004):

```csharp
using Sannr;

namespace MyApp.Models;

public partial class UserRegistrationRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Range(13, 120)]
    public int Age { get; set; }

    [Url]
    public string? Website { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [CreditCard]
    public string? CreditCardNumber { get; set; }

    [FileExtensions]
    public string? ProfileImagePath { get; set; }
}
```

That is all that is needed. The source generator automatically registers schema constraints at startup.

## How It Works

Sannr uses a **compile-time registration pattern** rather than runtime reflection:

1. **Build-time**: The Sannr source generator inspects your model classes and emits a static initializer (`[ModuleInitializer]`) that registers per-type schema appliers into `SannrGeneratedSchemaFilter`.
2. **App startup**: The `ModuleInitializer` runs automatically, populating the schema applier dictionary.
3. **Request time**: When Swashbuckle generates your OpenAPI spec, `SannrGeneratedSchemaFilter.Apply()` looks up the registered applier for the model type and applies the OpenAPI constraints.

This means:
- Zero runtime reflection
- Full AOT compatibility
- `SannrGeneratedSchemaFilter` is always present in the assembly, regardless of whether any models have validation attributes

## Supported Attributes

| Sannr Attribute | OpenAPI Constraint | Example |
| :--- | :--- | :--- |
| `[EmailAddress]` | `format: "email"` | `"email": {"type": "string", "format": "email"}` |
| `[Url]` | `format: "uri"` | `"website": {"type": "string", "format": "uri"}` |
| `[Range(min, max)]` | `minimum`, `maximum` | `"age": {"type": "integer", "minimum": 18, "maximum": 120}` |
| `[StringLength(max, min)]` | `minLength`, `maxLength` | `"name": {"type": "string", "minLength": 2, "maxLength": 100}` |
| `[Phone]` | `format: "tel"` | `"phone": {"type": "string", "format": "tel"}` |
| `[CreditCard]` | `format: "credit-card"` | `"card": {"type": "string", "format": "credit-card"}` |
| `[FileExtensions]` | `format: "file"` | `"avatar": {"type": "string", "format": "file"}` |

## Generated OpenAPI Schema Example

Given the model above, Sannr produces:

```json
{
  "UserRegistrationRequest": {
    "type": "object",
    "properties": {
      "email": { "type": "string", "format": "email" },
      "firstName": { "type": "string", "minLength": 2, "maxLength": 100 },
      "age": { "type": "integer", "minimum": 13, "maximum": 120 },
      "website": { "type": "string", "format": "uri" },
      "phoneNumber": { "type": "string", "format": "tel" },
      "creditCardNumber": { "type": "string", "format": "credit-card" },
      "profileImagePath": { "type": "string", "format": "file" }
    }
  }
}
```

## Benefits

### Single Source of Truth
Validation rules and API documentation are always synchronized. When you update a validation attribute, the OpenAPI schema automatically reflects the change at the next build.

### AOT Compatible
The schema filter uses no reflection at runtime. It is safe for use in Native AOT-published applications.

### Reduced Boilerplate
No need for manual `MapType<>` calls or custom schema classes per model.

## Limitations

### Required Properties
The `[Required]` attribute does not currently set the `required` array in OpenAPI schemas. That is handled by Swashbuckle's base functionality.

### Complex and Conditional Validations
Attributes like `[RequiredIf]` and custom validators do not affect OpenAPI schemas - they represent runtime conditional logic.

## Troubleshooting

### Schema Not Generated
- Ensure `options.SchemaFilter<SannrGeneratedSchemaFilter>()` is called in `AddSwaggerGen`
- Verify the `Sannr` NuGet package is referenced
- Confirm model properties have Sannr validation attributes

### SANN004 Build Error: Class Must Be Partial
If you see `error SANN004: Class 'X' has Sannr validation attributes but is not declared as 'partial'`, add the `partial` keyword to your model class. This is required for the source generator.

### Incorrect Constraints
- Verify attribute parameters match expected types (e.g. `[StringLength(100, MinimumLength = 2)]`)
- Ensure attributes are from the `Sannr` namespace, not `System.ComponentModel.DataAnnotations`

### Build Errors
- Confirm Swashbuckle.AspNetCore is properly referenced
- Check for conflicting schema filter registrations

## Advanced Usage

### Combining with Custom Filters

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<SannrGeneratedSchemaFilter>();

    // Add your own custom filters alongside Sannr
    options.SchemaFilter<MyCustomSchemaFilter>();
});
```

### Integration with API Versioning

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<SannrGeneratedSchemaFilter>();
    options.DocInclusionPredicate((name, api) => true);
});
```

## Migration from Manual Schema Definitions

### Before (Manual)
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<UserRequest>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["email"] = new() { Type = "string", Format = "email" },
            ["age"] = new() { Type = "integer", Minimum = 18, Maximum = 120 }
        }
    });
});
```

### After (Sannr v1.5)
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<SannrGeneratedSchemaFilter>(); // Single line
});

// Model drives the schema automatically
public partial class UserRequest
{
    [EmailAddress] public string Email { get; set; } = string.Empty;
    [Range(18, 120)] public int Age { get; set; }
}
```

## Testing

Run the included OpenAPI integration tests with:
```bash
dotnet test --filter OpenApiIntegrationTests
```