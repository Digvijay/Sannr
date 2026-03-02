# Getting Started with Sannr

Sannr is a high-performance, compile-time validation and sanitization library for .NET. This guide will help you integrate Sannr into your projects, whether you're starting fresh or migrating an existing application.

## Prerequisites

- .NET 8.0 / 9.0 / 10.0 Runtime
- .NET 10.0 SDK (Recommended for the latest source generation features)
- **IDE Support**: Visual Studio 2022, JetBrains Rider, or VS Code with C# Dev Kit.

## 1. Installation

Add the Sannr NuGet package to your primary project (usually your Web API project):

```bash
dotnet add package Sannr
```

> [!IMPORTANT]
> Sannr leverages **Roslyn Source Generators**. Ensure your build environment and IDE are up to date to support real-time code generation.

---

## 2. Greenfield Projects (New Setup)

If you are starting a new project, follow these steps to enable Sannr's validation engine.

### Step 1: Register Sannr
In your `Program.cs`, call `AddSannr()` to register the validation infrastructure.

```csharp
using Sannr.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Register Sannr - this automatically finds and registers all generated validators
builder.Services.AddSannr(options => {
    options.EnableMetrics = true; // Optional: Enable performance metrics
});

var app = builder.Build();
```

### Step 2: Enable Validation for Routes
Sannr integrates seamlessly with Minimal APIs and Controllers. Use `.WithSannrValidation()` to protect your endpoints.

```csharp
var api = app.MapGroup("/api/v1").WithSannrValidation();

api.MapPost("/users", (UserDto user) => Results.Ok(user));
```

---

## 3. Existing Projects (Migration)

If you have an existing project using `DataAnnotations` or `FluentValidation`, Sannr makes it easy to migrate incrementally.

### Automatic Migration via CLI
Use the Sannr CLI to automatically convert your existing models:

```bash
# Install the tool
dotnet tool install --global Sannr.Cli

# Migrate models in a directory
sannr migrate --source ./Models --target ./Models
```


---

## 4. Define Your Models

To enable validation, your models must be `partial` and decorated with Sannr attributes.

```csharp
using Sannr;

namespace MyApp.Models;

public partial class UserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [Sanitize(Trim = true)] // Automatically cleans input data
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(18, 99)]
    public int Age { get; set; }
}
```

---

## 5. Performance Monitoring

Sannr provides built-in support for `System.Diagnostics.Metrics`. You can monitor validation performance in real-time.

```csharp
builder.Services.AddSannr(options => {
    options.EnableMetrics = true;
    options.MetricsPrefix = "my_api_validation";
});
```

---

## Next Steps

- **Reference**: [Available Attributes](./ATTRIBUTES.md)
- **Integration**: [OpenAPI / Swagger Setup](./OPENAPI_INTEGRATION.md)
- **Advanced**: [Minimal API Integration](./MINIMAL_API_INTEGRATION.md)
- **Security**: [PII & Shadow Types](./STATIC_REFLECTION.md)
