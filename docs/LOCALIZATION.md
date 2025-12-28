# Sannr Localization Guide

Sannr provides comprehensive support for localized validation error messages through .NET resource files (.resx). This allows you to provide culture-specific error messages for your validation attributes.

## How Sannr Localization Works

Sannr's source generator automatically handles localization by checking for `ErrorMessageResourceName` and `ErrorMessageResourceType` properties on validation attributes. When these are specified, the generated validation code will use the resource manager to retrieve localized messages based on the current culture.

## Setting Up Localization

### 1. Create Resource Files

Create .resx files in your project for different cultures:

```
Resources/
├── ValidationMessages.resx          (Default/English)
├── ValidationMessages.es.resx       (Spanish)
├── ValidationMessages.fr.resx       (French)
├── ValidationMessages.de.resx       (German)
└── ValidationMessages.Designer.cs   (Auto-generated)
```

### 2. Define Resource Messages

In your default `ValidationMessages.resx` file, add entries like:

| Name | Value |
|------|-------|
| RequiredFieldRequired | The {0} field is required. |
| EmailFieldInvalid | The {0} field is not a valid email address. |
| AgeFieldOutOfRange | The {0} field must be between {1} and {2}. |

### 3. Create Localized Versions

For Spanish (`ValidationMessages.es.resx`):

| Name | Value |
|------|-------|
| RequiredFieldRequired | El campo {0} es obligatorio. |
| EmailFieldInvalid | El campo {0} no es una dirección de correo electrónico válida. |
| AgeFieldOutOfRange | El campo {0} debe estar entre {1} y {2}. |

### 4. Use in Model Classes

```csharp
using YourNamespace.Resources;

public class UserModel
{
    [Required(
        ErrorMessageResourceName = nameof(ValidationMessages.RequiredFieldRequired),
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? Name { get; set; }

    [EmailAddress(
        ErrorMessageResourceName = nameof(ValidationMessages.EmailFieldInvalid),
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public string? Email { get; set; }

    [Range(18, 120,
        ErrorMessageResourceName = nameof(ValidationMessages.AgeFieldOutOfRange),
        ErrorMessageResourceType = typeof(ValidationMessages))]
    public int Age { get; set; }
}
```

### 5. Runtime Culture Switching

The validation messages will automatically adapt to the current culture:

```csharp
// Set Spanish culture
Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

// Validation will now use Spanish messages
var result = await validator.Validate(model);
```

## Benefits of Sannr Localization

1. **Compile-time Safety**: Resource names are checked at compile time
2. **Performance**: No runtime reflection or string parsing
3. **Type Safety**: Strongly-typed resource access
4. **Standard .NET**: Uses familiar .resx files and ResourceManager
5. **Fallback Support**: Automatically falls back to default messages if localized versions are missing

## Comparison with Other Libraries

| Feature | Sannr | FluentValidation | DataAnnotations |
|---------|-------|------------------|-----------------|
| Resource Files | ✅ .resx | ✅ .resx | ✅ .resx |
| Compile-time Safety | ✅ Full | ✅ Partial | ⚠️ Partial |
| Performance | ✅ Generated | ✅ Cached | ✅ Cached |
| Culture Fallback | ✅ Automatic | ✅ Automatic | ✅ Automatic |
| Type Safety | ✅ Strong | ✅ Strong | ✅ Strong |

### Detailed Comparison

**Resource Files:**
- **Sannr**: Full .resx support with source-generated access
- **FluentValidation**: Native .resx support with strongly-typed wrappers
- **DataAnnotations**: .resx support through IStringLocalizer and resource types

**Compile-time Safety:**
- **Sannr**: ✅ Full - Source generator validates resource names and types at compile time
- **FluentValidation**: ✅ Partial - Strongly-typed wrappers provide compile-time safety, but string keys can still be runtime errors
- **DataAnnotations**: ⚠️ Partial - Resource types are validated at compile time, but property names are runtime-checked

**Performance:**
- **Sannr**: ✅ Generated - Zero runtime overhead, all code generated at compile time
- **FluentValidation**: ✅ Cached - Uses ConcurrentDictionary caching for translations
- **DataAnnotations**: ✅ Cached - ResourceManager caching with delegate-based access

**Culture Fallback:**
- **Sannr**: ✅ Automatic - ResourceManager handles culture hierarchy fallback
- **FluentValidation**: ✅ Automatic - ResourceManager fallback from specific to neutral to English
- **DataAnnotations**: ✅ Automatic - Standard .NET ResourceManager culture fallback

**Type Safety:**
- **Sannr**: ✅ Strong - Generated code uses strongly-typed resource properties
- **FluentValidation**: ✅ Strong - Strongly-typed resource wrappers prevent invalid access
- **DataAnnotations**: ✅ Strong - Type-safe resource access through generic IStringLocalizer<T>

## Advanced Usage

### Custom Resource Classes

You can create custom resource classes for different domains:

```csharp
// BusinessRules.resx
public class BusinessRules
{
    public static string InsufficientFunds => ResourceManager.GetString("InsufficientFunds");
    public static string AccountLocked => ResourceManager.GetString("AccountLocked");
}
```

### Culture-Specific Validation

Combine with custom validators for culture-aware validation:

```csharp
[CustomValidator(typeof(CultureAwareValidator))]
[ErrorMessageResourceName = nameof(ValidationMessages.InvalidFormat),
 ErrorMessageResourceType = typeof(ValidationMessages)]
public string? LocalizedValue { get; set; }
```

## Best Practices

1. **Use Descriptive Names**: Make resource names clear and descriptive
2. **Include Placeholders**: Use {0}, {1}, etc. for dynamic values
3. **Test Multiple Cultures**: Ensure your UI works with different languages
4. **Provide Fallbacks**: Always include default (English) messages
5. **Organize by Feature**: Group related messages in feature-specific resource files

## Example Implementation

See `tests/Sannr.Tests/Resources/` for a complete working example with English and Spanish translations.

## Conclusion

Sannr provides robust localization support that integrates seamlessly with .NET's standard resource management system. While all major .NET validation libraries offer good localization capabilities, Sannr's source-generation approach provides unique advantages in performance and compile-time safety.

The choice between validation libraries depends on your project's specific requirements. Sannr is particularly well-suited for applications that prioritize:
- Compile-time validation of localization setup
- Zero runtime overhead for message retrieval
- Strong typing throughout the localization pipeline

For applications already using FluentValidation or DataAnnotations, those libraries also provide excellent localization support with their respective strengths in flexibility and ASP.NET Core integration.

Regardless of the validation library chosen, using .resx resource files ensures consistency with .NET localization patterns and provides a solid foundation for internationalizing your applications.