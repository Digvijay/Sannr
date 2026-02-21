# Custom Validators

Beyond standard data annotations, complex business logic frequently requires robust rules tailored to your application's specific behavior. Sannr enables custom validations powered by AOT syntax instead of slower Runtime Reflection lookup logic.

## Declaring a Custom Validator

Use the `[CustomValidator]` attribute to wire complex logic dynamically generated to target your validation evaluation pipelines.

```csharp
using Sannr;

[CustomValidator(typeof(UserValidator))]
public class User
{
    public string Username { get; set; }
    public string Email { get; set; }
}
```

## Implementing the Logic

Implementing an inheriting `SannrValidator<T>` instance gives your AOT generators exactly the implementation structure needed to hook into your data objects safely.

```csharp
// Sannr discovers this instance via the [CustomValidator] attribute.
public class UserValidator : SannrValidator<User>
{
    // The synchronous implementation hook
    public override ValidationResult Validate(User instance, ValidationContext context)
    {
        var result = ValidationResult.Success();
        
        // Example logic
        if (instance.Username == instance.Email)
        {
            result.Errors.Add(new ValidationError("Username", "Username cannot be the email completely."));
        }
        
        return result;
    }
}
```

## Dependency Injection

Custom validator execution happens seamlessly within the context wrapper, which inherits scopes natively. Dependencies registered in ASP.NET can be retrieved from `ValidationContext.ServiceProvider` easily.
