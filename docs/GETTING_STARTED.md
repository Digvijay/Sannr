# Getting Started with Sannr

Sannr is a high-performance, compile-time validation and sanitization library for .NET. This guide will help you integrate Sannr into your own projects.

## Prerequisites

- .NET 8.0 SDK or later
- An ASP.NET Core project (Web API, Minimal API, etc.)

## 1. Installation

Add the Sannr NuGet package to your project:

```bash
dotnet add package Sannr
```

*Note: Sannr uses source generation. Ensure your IDE supports Roslyn source generators (Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit).*

## 2. Basic Configuration

In your `Program.cs`, register Sannr services and enable validation for your routes.

```csharp
using Sannr;
using Sannr.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Register Sannr - this finds and registers all generated validators
builder.Services.AddSannr();

var app = builder.Build();
// 2. Enable Sannr validation for your API group or specific endpoints
// This automatically validates and sanitizes incoming requests
var api = app.MapGroup("/api").WithSannrValidation();

api.MapPost("/users", (UserDto user) => 
{
    // If you reach here, 'user' is already validated and sanitized!
    return Results.Ok(user);
});

app.Run();
```

### Automatic Error Handling

When you use `.WithSannrValidation()`, Sannr injects an `IEndpointFilter` into your route pipeline. 
- **Validation Success**: Your handler is called with the `user` object already sanitized and ready for use.
- **Validation Failure**: Sannr intercepts the request **before** it reaches your handler. It automatically returns a `400 Bad Request` response with a standard `ValidationProblemDetails` body containing all validation errors grouped by property name.

*Note: You don't need to write manual `if (!ModelState.IsValid)` checks in your handlers.*

## 3. Define Your Models

Create your models and decorate them with Sannr validation and sanitization attributes. Models must be `partial` for source generation to work.

```csharp
using Sannr;

namespace MyApp.Models;

public partial class UserDto
{
    [Required(ErrorMessage = "Username is mandatory")]
    [StringLength(50, MinimumLength = 3)]
    [Sanitize(Trim = true)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Sanitize(ToLower = true)]
    public string Email { get; set; } = string.Empty;

    [Range(18, 99)]
    public int Age { get; set; }
}
```

## 4. Advanced Features

### Sanitization
Sannr can automatically clean your data before it reaches your handlers.
```csharp
[Sanitize(Trim = true, ToUpper = true)]
public string ProductCode { get; set; }
```

### Conditional Validation
Make fields required based on other fields.
```csharp
public bool HasCustomShipping { get; set; }

[RequiredIf(nameof(HasCustomShipping), true)]
public string? ShippingAddress { get; set; }
```

### Client-Side Validation
Generate validation rules for your frontend automatically.
```csharp
[GenerateClientValidators(Language = ClientValidationLanguage.TypeScript)]
public partial class LoginForm { ... }
```
Access the rules via `LoginForm.ValidationRulesTypeScript` or export them to a file.

## 5. Configuration Options

You can customize Sannr's behavior by passing an options action to `AddSannr()`.

```csharp
builder.Services.AddSannr(options => 
{
    // 1. Performance Monitoring
    options.EnableMetrics = true;           // Enables System.Diagnostics.Metrics
    options.MetricsPrefix = "myapp_sannr"; // Custom prefix for metric names

    // 2. Enhanced Error Responses
    // Returns detailed error objects including correlation IDs and timing metadata
    options.EnableEnhancedErrorResponses = true;
    
    // 3. Metadata Control
    options.IncludeValidationRuleMetadata = true; // Includes which rule failed in the response
    options.IncludeValidationDuration = true;     // Includes 'durationMs' in the response
});
```

### Options Reference

| Option | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `EnableMetrics` | `bool` | `false` | Enables collection of validation performance metrics. |
| `MetricsPrefix` | `string` | `"sannr_validation"` | Prefix for the reported metrics. |
| `EnableEnhancedErrorResponses` | `bool` | `false` | Enables rich ProblemDetails with correlation IDs and metadata. |
| `IncludeValidationRuleMetadata` | `bool` | `true` | When enhanced responses are on, includes rule details (e.g. min/max values). |
| `IncludeValidationDuration` | `bool` | `false` | Includes the validation time in milliseconds in the error response. |

## Next Steps

- Explore [available attributes](./ATTRIBUTES.md)
- Learn about [client-side validation](./CLIENT_SIDE_VALIDATION.md)
- Check out the [OpenAPI integration](./OPENAPI_INTEGRATION.md)
- See how to use [Fluent-style configuration](./BUSINESS_RULE_VALIDATORS.md)
