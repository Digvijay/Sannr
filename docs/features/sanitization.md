# Sanitization

Sannr can automatically clean and transform your data *before* it gets validated and proceeds to your business logic. 

Traditional validation frameworks only report whether a field is "valid" or not. Fixing typos, trimming bounding whitespace, or normalizing casing is often relegated to boilerplate code scattered across controllers or business layer components.

Sannr's `[Sanitize]` attribute executes transformations instantly via AOT logic.

## Supported Sanitizers

You can stack properties within the single generic `[Sanitize]` attribute.

```csharp
public class UserProfile
{
    // Auto-Sanitization: Trims whitespace and Uppercases input before validation
    [Sanitize(Trim = true, ToUpper = true)]
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    [Sanitize(ToLower = true)]
    public string Email { get; set; }
}
```

### Trim

Automatically maps to `string.Trim()` to remove leading and trailing whitespace.

### ToUpper / ToLower

Maps to culture-invariant casing. Excellent for deterministic emails, database matching, or standardizing serial codes.

## Execution Order

Sanitization *always* happens before attribute-level validations string formats. 

1. Sannr receives the DTO.
2. The AOT engine applies modifications (`Trim`, `ToUpper`, etc).
3. Evaluates `[Required]`, `[EmailAddress]`, bounds.
4. Returns the valid and cleansed DTO cleanly to ASP.NET controllers.
