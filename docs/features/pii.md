# PII Awareness

Privacy is critical in enterprise systems. Logging architectures or event streamers frequently need to identify Personally Identifiable Information (PII) to encrypt or mask data points without manual intervention or risky `GetCustomAttribute<PiiAttribute>()` reflection checks.

Sannr provides a lightweight tagging system mapping seamlessly into its Shadow Types structure.

## The `[Pii]` Attribute

Annotate fields with the Sannr `[Pii]` attribute directly on the properties of a `[SannrReflect]`-marked object.

```csharp
using Sannr;

[SannrReflect]
public class User
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Pii] // Explicitly state this data must be protected downstream
    [EmailAddress]
    public string Email { get; set; }
}
```

## Constant-Time Metadata Lookup

Query this state efficiently at scale with *zero* runtime overhead.

```csharp
// The generated static type evaluates instantly
bool shouldMask = UserShadow.IsEmailPii; // Returns True continuously from compiled constants.

if (shouldMask)
{
    Logger.Log("Masking email structure before emitting metric streams.");
}
```

## Generic Visitor Extraction

For dynamic iterationslike database insert mappers or log dump frameworksthe Shadow Type `Visit()` structure explicitly identifies PII automatically in zero allocations.

```csharp
UserShadow.Visit(user, (propertyName, value, isPii) => 
{
    if (isPii) {
        Console.WriteLine($"{propertyName}: [REDACTED]");
    } else {
        Console.WriteLine($"{propertyName}: {value}");
    }
});
```
