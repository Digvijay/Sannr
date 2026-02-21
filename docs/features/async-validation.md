# Asynchronous Validation

One key limitation of classic `System.ComponentModel.DataAnnotations` is its inherently synchronous lifecycle. Checking a database for a duplicate username, verifying a serial token via external API call, or validating unique IDs required either blocking threads or dropping out into custom validation contexts manually. 

Sannr integrates *first-class Async/Await* patterns into its generated validation trees.

## Performing Database Checks

You can provide full `Task<ValidationResult>` methods bound to your domain scopes out-of-the-box. The `[CustomValidator]` attribute accepts an `IsAsync = true` flag.

```csharp
public class UserRegistration
{
    // Bind to the exact method doing the asynchronous logic
    [CustomValidator(typeof(UserRules), nameof(UserRules.IsUniqueAsync), IsAsync = true)]
    public string Email { get; set; }
}
```

## Connecting Services via `IServiceProvider`

The validation pipeline provides the request's scoped `IServiceProvider`, allowing you to inject DbContexts or Network HttpClients seamlessly.

```csharp
using Sannr;
using Microsoft.Extensions.DependencyInjection; // For .GetRequiredService

public static class UserRules
{
    // Services are injected automatically from the HttpContext
    public static async Task<ValidationResult> IsUniqueAsync(string email, IServiceProvider sp)
    {
        // Obtain scoped services automatically!
        var db = sp.GetRequiredService<AppDbContext>();
        
        // Asynchronous query against entity framework.
        bool exists = await db.Users.AnyAsync(u => u.Email == email);
        
        return exists 
            ? ValidationResult.Error("Email is already taken.") 
            : ValidationResult.Success();
    }
}
```

The underlying pipeline processes these tasks without allocating vast proxy objects or wrapping dynamic trees, maximizing AOT-speed throughput across web API endpoints.
