# Minimal API Integration

Sannr provides seamless integration with ASP.NET Core Minimal APIs through the `Validated<T>` wrapper, which automatically handles validation and returns appropriate HTTP responses for invalid data.

## Overview

The `Validated<T>` class wraps your model types and provides:
- Automatic validation using Sannr attributes
- Safe access to validated data
- Convenient methods for returning HTTP error responses
- Implicit conversion to the underlying model type

## Setup

First, ensure Sannr is registered in your dependency injection container:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Sannr validation services
builder.Services.AddSannr();

var app = builder.Build();
```

## Basic Usage

Define your model with Sannr validation attributes:

```csharp
public class CreateUserRequest
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, StringLength(50, MinimumLength = 2)]
    public string? Name { get; set; }

    [Range(18, 120)]
    public int Age { get; set; }
}
```

Use `Validated<T>` in your Minimal API endpoints:

```csharp
app.MapPost("/users", async (Validated<CreateUserRequest> request) =>
{
    // Validation happens automatically
    if (!request.IsValid)
    {
        // Return 400 Bad Request with validation errors
        return request.ToBadRequestResult();
    }

    // Safe to access the validated data
    var user = request.Value;

    // Process the valid request...
    await CreateUserAsync(user);

    return Results.Created($"/users/{user.Id}", user);
});
```

## Advanced Usage

### Custom Error Messages

```csharp
app.MapPost("/users", async (Validated<CreateUserRequest> request) =>
{
    if (!request.IsValid)
    {
        return request.ToBadRequestResult("Invalid user data provided");
    }

    // Process valid request...
});
```

### Manual Validation

If you need more control over the validation process:

```csharp
app.MapPost("/users", async (CreateUserRequest request, IServiceProvider services) =>
{
    var validated = await Validated<CreateUserRequest>.CreateAsync(request, services);

    if (!validated.IsValid)
    {
        return validated.ToBadRequestResult();
    }

    // Use validated.Value...
});
```

### Accessing Validation Errors

```csharp
app.MapPost("/users", async (Validated<CreateUserRequest> request) =>
{
    if (!request.IsValid)
    {
        // Log validation errors
        foreach (var error in request.Errors)
        {
            logger.LogWarning("Validation error for {Property}: {Message}",
                error.MemberName, error.Message);
        }

        return request.ToBadRequestResult();
    }

    // Process valid request...
});
```

### TryGetValue Pattern

```csharp
app.MapPost("/users", (Validated<CreateUserRequest> request) =>
{
    if (request.TryGetValue(out var user))
    {
        // User is valid and not null
        return Results.Ok(user);
    }
    else
    {
        // Handle validation errors
        return request.ToBadRequestResult();
    }
});
```

## Error Response Format

When validation fails, `ToBadRequestResult()` returns a `400 Bad Request` response with a `ValidationProblemDetails` body:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["The Email field is not a valid e-mail address."],
    "Name": ["The Name field is required."]
  }
}
```

## Integration with OpenAPI/Swagger

When using `Validated<T>` with Swashbuckle.AspNetCore, the OpenAPI schema will automatically include validation constraints from your Sannr attributes. See the [OpenAPI Integration](OPENAPI_INTEGRATION.md) documentation for details.

## Complete Example

```csharp
using Sannr.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSannr();

var app = builder.Build();

public class UserRegistration
{
    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    [Required, Compare("Password")]
    public string? ConfirmPassword { get; set; }

    [Range(13, 120)]
    public int Age { get; set; }
}

app.MapPost("/register", async (Validated<UserRegistration> registration) =>
{
    if (!registration.IsValid)
    {
        return registration.ToBadRequestResult("Registration failed due to invalid data");
    }

    var user = registration.Value;

    // Create user account...
    var userId = await userService.CreateUserAsync(user);

    return Results.Created($"/users/{userId}", new { UserId = userId });
});

app.Run();
```

## Benefits

- **Clean Code**: Eliminates boilerplate validation logic in your API endpoints
- **Type Safety**: Strongly-typed access to validated data
- **Consistent Errors**: Standardized validation error responses
- **OpenAPI Integration**: Automatic schema generation with validation constraints
- **AOT Friendly**: Works with Native AOT compilation

## Migration from Manual Validation

### Before (Manual Validation)

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

### After (with Validated<T>)

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

## Troubleshooting

### Validator Not Found

If you get an error about no validator being registered, ensure:
1. Your model has Sannr validation attributes
2. The source generator has run (check for generated validator classes)
3. Sannr services are registered with `AddSannr()`

### Validation Not Working

Common issues:
- Missing `[Required]` attributes on non-nullable properties
- Using data annotation attributes instead of Sannr attributes
- Model not being processed by the source generator

### OpenAPI Schema Issues

If OpenAPI schemas don't show validation constraints:
- Ensure `AddSannrValidationSchemas()` is called in your Swagger configuration
- Check that the model is used in endpoints with `Validated<T>`

## See Also

- [OpenAPI Integration](OPENAPI_INTEGRATION.md) - Automatic schema generation
- [Validation Attributes](ATTRIBUTES.md) - Available validation attributes
- [Publishing Guide](PUBLISHING.md) - Deployment considerations