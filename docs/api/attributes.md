# Attributes Outline

Sannr translates standard C# class attributes into strict compile-time assertion statements natively compatible with trimming. This prevents runtime `MethodInfo.Invoke()` penalties.

## The Standard Property Constraints

These natively mirror their System component behaviors exactly with explicit string length validation, sequence lookups, and regex implementations designed with strict JIT considerations.

- `[Required]`
- `[StringLength(maximumLength, MinimumLength)]`
- `[Range(min, max)]`
- `[EmailAddress]`
- `[CreditCard]`
- `[Url]`
- `[Phone]`
- `[FileExtensions(Extensions = "pdf,docx")]`

## Expanded Enterprise Assertions

Advanced condition flags extend your declarative API capabilities instantly.

- `[RequiredIf(DependencyData, TargetValue)]`: Conditionally marks boundaries.
- `[FutureDate]`: Business rule dictating strict chronological bounds.
- `[AllowedValues("State1", "State2")]`: Generates direct `switch` logic statements avoiding massive array iterations for simple enumerations.
- `[ConditionalRange("Condition", "State", Min, Max)]`

## The `[Sanitize]` Mutation Attribute

This unique property alters state *before* logic executes, shifting `Trim`, culture casing, and whitespace processing directly into Sannr bounds.

```csharp
[Sanitize(Trim = true, ToUpper = true)]
public string Code { get; set; }
```

## `[CustomValidator(Type, Method)]`

Declares an explicit hook into custom, AOT-friendly code definitions containing `ValidationResult` objects with strict HTTP-scoped async evaluation capabilities natively provided.
