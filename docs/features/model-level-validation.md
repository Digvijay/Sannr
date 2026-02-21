# Model-Level Validation

There are business rule invariants that cannot be defined effectively on individual properties. When a combination or calculation of state across multiple data members must be checked simultaneously, Sannr provides the explicit `IValidatableObject`.

Instead of inheriting the sluggish `System.ComponentModel.DataAnnotations.IValidatableObject` implementation, you adopt a zero-allocation Sannr contract.

## Interface Binding

Implement the `Sannr.IValidatableObject` interface to tap into the AOT pipeline.

```csharp
using Sannr;

public class EmploymentModel : Sannr.IValidatableObject
{
    [Required]
    public string Name { get; set; }
    
    public bool IsEmployed { get; set; }
    
    [Range(0, 1000000)]
    public decimal? Salary { get; set; }

    public IEnumerable<Sannr.ModelValidationResult> Validate(SannrValidationContext context)
    {
        if (IsEmployed && (!Salary.HasValue || Salary.Value <= 0))
        {
            // Specifically flag the error mapping dynamically to the "Salary" member state
            yield return new Sannr.ModelValidationResult
            {
                MemberName = nameof(Salary),
                Message = "Salary is strictly required when the flag indicates active employment."
            };
        }
    }
}
```

## AOT Generation

Unlike older interfaces which rely on dynamic type scanning at runtime the exact second a validator needs an instance check, Sannr Code Generators identify `Sannr.IValidatableObject` during the MSBuild cycle. It statically constructs a specific call hierarchy without any boxing overhead or runtime type switching.
