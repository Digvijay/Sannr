# Conditional Validation

Often, certain fields are only requiredor only subject to range constraintsif other fields contain specific values. Instead of writing complex `IValidatableObject` methods, Sannr natively handles conditional rule application entirely via Source Generation.

## RequiredIf

`RequiredIf` allows you to make a property dependent on the state of a sibling property. This condition evaluates quickly via AOT static lookups.

```csharp
public class UserProfile
{
    public string Country { get; set; }

    // ZipCode is only mandatory when Country == "USA"
    [RequiredIf(nameof(Country), "USA")]
    public string ZipCode { get; set; }
    
    public bool IsCompany { get; set; }
    
    // Tax ID is only required if the user registers as a company entity
    [RequiredIf(nameof(IsCompany), true)]
    public string TaxId { get; set; }
}
```

## Conditional Range

Numeric boundaries can shift based on enum or string states elsewhere in the model.

```csharp
public class LoanApplication
{
    [Required]
    public string LoanType { get; set; } // "personal", "business", "mortgage"

    [ConditionalRange("LoanType", "personal", 1000, 50000)]
    [ConditionalRange("LoanType", "business", 5000, 1000000)]
    [ConditionalRange("LoanType", "mortgage", 50000, 5000000)]
    public decimal Amount { get; set; }
}
```

## Client-Side Rule Generation

Because these conditions are strongly statically typed, Sannr will emit the equivalent `RequiredIf` Javascript/TypeScript configuration bounds without manual duplication.
