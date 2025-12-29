# Advanced Validation Features

This document provides comprehensive documentation for Sannr's advanced validation capabilities, including source generator-based validation, sanitization, custom validators, and conditional logic.

## Overview

Sannr implements a source generator-based validation system that generates optimized validation code at compile-time. This approach provides AoT compliance and zero runtime reflection overhead.

## Source Generator Auto-Registration

Sannr's source generators automatically generate validators for types with validation attributes at compile-time:

```csharp
// In Program.cs
builder.Services.AddSannr(); // Uses pre-generated validators
```

The source generators:
1. Scan types with validation attributes during compilation
2. Generate optimized validation functions
3. Register validators in the dependency injection container
3. Registers validators in the global registry

## Core Validation Attributes

### Standard Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Required]` | Ensures field is not null/empty | `[Required(ErrorMessage = "Field is required")]` |
| `[StringLength]` | Validates string length constraints | `[StringLength(100, MinimumLength = 5)]` |
| `[Range]` | Numeric range validation | `[Range(0, 100)]` |
| `[EmailAddress]` | Email format validation | `[EmailAddress]` |
| `[Url]` | URL format validation | `[Url]` |
| `[Phone]` | Phone number format validation | `[Phone]` |
| `[CreditCard]` | Credit card number validation | `[CreditCard]` |
| `[FileExtensions]` | File extension validation | `[FileExtensions(Extensions = "pdf,docx,txt")]` |
| `[FutureDate]` | Ensures date is in the future | `[FutureDate]` |
| `[AllowedValues]` | Whitelist validation | `[AllowedValues("Option1", "Option2")]` |

### Conditional Attributes

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[RequiredIf]` | Required when another property has a specific value | `[RequiredIf("IsActive", true)]` |
| `[ConditionalRange]` | Range validation based on other properties | `[ConditionalRange("MinValue", "MaxValue")]` |

## Sanitization

Sannr provides automatic data sanitization to clean and transform input before validation:

```csharp
public class UserInput
{
    [Sanitize(Trim = true, ToUpper = true)]
    public string? Username { get; set; }
    
    [Sanitize(Trim = true, ToLower = true)]
    public string? Email { get; set; }
    
    [Sanitize(Trim = true)]
    public string? Description { get; set; }
}
```

Sanitization occurs before validation and modifies the object instance directly.

## Custom Validators

For complex business logic, implement custom validators:

```csharp
[CustomValidator(typeof(ProductValidator))]
public class Product
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
}

public class ProductValidator : SannrValidator<Product>
{
    public override async Task<ValidationResult> ValidateAsync(Product instance, ValidationContext context)
    {
        var result = ValidationResult.Success();
        
        // Async business logic
        if (instance.Category == "Electronics" && instance.Price < 10)
        {
            result.Errors.Add(new ValidationError("Price", "Electronics must cost at least $10"));
        }
        
        // Access services via DI
        var categoryService = context.ServiceProvider.GetRequiredService<ICategoryService>();
        if (!await categoryService.IsValidCategory(instance.Category))
        {
            result.Errors.Add(new ValidationError("Category", "Invalid category"));
        }
        
        return result;
    }
}
```

## Validation Groups

Control validation scope for different scenarios:

```csharp
public class RegistrationModel
{
    [Required]
    public string? Username { get; set; }
    
    [Required]
    public string? Email { get; set; }
    
    [Required(Group = "Complete")]
    public string? FullName { get; set; }
    
    [Required(Group = "Complete")]
    public DateTime? DateOfBirth { get; set; }
}

// Basic registration
var basicResult = await validator.ValidateAsync(model);

// Complete profile
var completeResult = await validator.ValidateAsync(model, group: "Complete");
```

## Error Messages and Localization

### Custom Error Messages

```csharp
[Required(ErrorMessage = "Please provide a username")]
[StringLength(50, ErrorMessage = "Username must be 50 characters or less")]
public string? Username { get; set; }
```

### Resource-Based Messages

```csharp
[Required(
    ErrorMessageResourceName = "RequiredField",
    ErrorMessageResourceType = typeof(ValidationResources)
)]
public string? Name { get; set; }
```

### Display Names

```csharp
[Display(Name = "User's Full Name")]
[Required]
public string? FullName { get; set; }

// Error: "User's Full Name is required"
```

## Severity Levels

Control error severity for different handling:

```csharp
public class LoginModel
{
    [Required(Severity = Severity.Error)]
    public string? Username { get; set; }
    
    [Required(Severity = Severity.Warning)]
    public string? PreferredLanguage { get; set; }
}
```

## Performance Characteristics

- **Auto-registration**: Compile-time code generation
- **Validation**: Direct compiled method calls
- **Memory**: Minimal footprint with shared validators
- **Threading**: Thread-safe validation execution

## Integration Examples

### ASP.NET Core MVC

```csharp
public class UserController : Controller
{
    private readonly SannrValidatorRegistry _validators;
    
    public UserController(SannrValidatorRegistry validators)
    {
        _validators = validators;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(User model)
    {
        var result = await _validators.ValidateAsync(model);
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.MemberName, error.Message);
            }
            return View(model);
        }
        
        // Process valid model
        return RedirectToAction("Index");
    }
}
```

### Minimal API

```csharp
app.MapPost("/api/users", async (Validated<User> user, SannrValidatorRegistry validators) =>
{
    // Validation already performed by Validated<T> wrapper
    if (!user.IsValid)
    {
        return Results.ValidationProblem(user.ValidationResult.ToModelStateDictionary());
    }
    
    // Process user
    return Results.Created($"/api/users/{user.Value.Id}", user.Value);
});
```

## Migration from Manual Validation

When migrating from manual validation or other libraries:

- Replace manual validation calls with `SannrValidatorRegistry.TryGetValidator(model.GetType(), out var validator)`
- Add `builder.Services.AddSannr()` to startup for automatic registration
- All validation attributes work the same way

## Troubleshooting

### Validators Not Found

Ensure `AddSannr()` is called in startup and assemblies contain attributed types.

### Performance Issues

- Auto-registration scans all types - consider limiting assemblies
- Custom validators should be async for I/O operations
- Use validation groups to reduce validation scope

### Common Issues

- Ensure properties are public instance properties
- Custom validators must inherit from `SannrValidator<T>`
- Service provider access requires proper DI registration</content>
<parameter name="filePath">/Users/digvijay/source/github/Sannr/docs/ADVANCED_VALIDATION_FEATURES.md