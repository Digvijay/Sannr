# Sannr API Overview

Sannr provides a fully automated, declarative API layer based on Microsoft's Source Generators. Rather than initializing complex fluent builder trees manually, your entire validation suite is orchestrated solely by class attributes, static context parameters, and native DI.

## Core Namespaces

Sannr resides simply under:
- `using Sannr;`
- `using Sannr.AspNetCore;`

## Global Initialization

When integrating your Web API or HTTP lifecycle pipeline, one line coordinates all static validators detected by the Source Generator:

```csharp
var builder = WebApplication.CreateBuilder(args);

// The Source Generator creates an extension for zero-allocation injection mapping.
builder.Services.AddSannr(options => 
{
    // Configure rich error representations and metric profiling globally
    options.EnableMetrics = true; 
}); 
```

## AOT Validation Entrypoints

Normally handled automatically by `.WithSannrValidation()` endpoint mappers (or ASP.NET `IObjectModelValidator` intercepts), you can also invoke a compiled validator directly for background jobs and background services.

```csharp
var employee = new EmployeeDto();

// Bypasses reflection and evaluates directly against the hardcoded Sannr validator logic.
ValidationResult result = await SannrValidatorRegistry.GetValidator<EmployeeDto>().ValidateAsync(employee);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"[Error in {error.MemberName}]: {error.Message}");
    }
}
```

## Exploring Further

To dive into the API structure completely:
- Learn about the [Configuration object](/api/configuration)
- Review the [Attributes Guide](/api/attributes)
- Check how to execute the [Migration CLI](/api/cli)
- Address any integration issues with [Troubleshooting](/api/troubleshooting)
