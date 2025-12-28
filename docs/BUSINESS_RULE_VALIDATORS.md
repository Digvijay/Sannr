# Built-in Business Rule Validators

Sannr provides common business validation patterns as built-in attributes, eliminating the need for custom validators in most enterprise scenarios.

## Overview

Business rule validators address common validation patterns that appear across different domains:

- **Future Date Validation**: Ensuring dates are in the future (appointments, deadlines, etc.)
- **Allowed Values**: Restricting properties to predefined sets of values
- **Conditional Range**: Applying numeric ranges based on other property values

## FutureDate Attribute

The `[FutureDate]` attribute ensures that a `DateTime` or `DateTime?` property contains a date in the future.

### Usage

```csharp
public class Appointment
{
    [Required]
    public string CustomerName { get; set; }

    [FutureDate]
    public DateTime AppointmentDate { get; set; }
}

public class Order
{
    [FutureDate]
    public DateTime? DeliveryDate { get; set; } // Nullable also supported
}
```

### Client-Side Generation

```json
{
  "appointmentDate": { "futureDate": true },
  "deliveryDate": { "futureDate": true }
}
```

### Validation Logic

- For non-nullable `DateTime`: Must be > `DateTime.Now`
- For nullable `DateTime?`: If not null, must be > `DateTime.Now`
- Uses `DateTime.Now` for comparison (not `DateTime.UtcNow`)

## AllowedValues Attribute

The `[AllowedValues]` attribute restricts a property to a predefined set of string values.

### Usage

```csharp
public class Product
{
    [Required]
    public string Name { get; set; }

    [AllowedValues("active", "inactive", "discontinued", "pending")]
    public string Status { get; set; }

    [AllowedValues("USD", "EUR", "GBP", "JPY")]
    public string Currency { get; set; }
}
```

### Constructor

```csharp
public AllowedValuesAttribute(params string[] values)
```

### Properties

- `Values`: Array of allowed string values

### Client-Side Generation

```json
{
  "status": { "allowedValues": ["active", "inactive", "discontinued", "pending"] },
  "currency": { "allowedValues": ["USD", "EUR", "GBP", "JPY"] }
}
```

### Validation Logic

- Property value must match one of the allowed values (case-sensitive)
- Works with `string` and `string?` properties
- Null/empty values are allowed unless combined with `[Required]`

## ConditionalRange Attribute

The `[ConditionalRange]` attribute applies numeric range validation only when another property matches a specific value.

### Usage

```csharp
public class LoanApplication
{
    [Required]
    [AllowedValues("personal", "business", "mortgage")]
    public string LoanType { get; set; }

    [ConditionalRange("LoanType", "personal", 1000, 50000)]
    [ConditionalRange("LoanType", "business", 5000, 1000000)]
    [ConditionalRange("LoanType", "mortgage", 50000, 5000000)]
    public decimal Amount { get; set; }
}

public class UserProfile
{
    public bool IsPremium { get; set; }

    [ConditionalRange("IsPremium", true, 100, 1000)]
    [ConditionalRange("IsPremium", false, 10, 100)]
    public int Credits { get; set; }
}
```

### Constructor

```csharp
public ConditionalRangeAttribute(string otherProperty, object targetValue, double minimum, double maximum)
```

### Properties

- `OtherProperty`: Name of the property to check
- `TargetValue`: Value that the other property must have for validation to apply
- `Minimum`: Minimum allowable value
- `Maximum`: Maximum allowable value

### Client-Side Generation

```json
{
  "amount": {
    "minRange": 1000,
    "maxRange": 50000,
    "conditionProperty": "loanType",
    "conditionValue": "personal"
  },
  "credits": {
    "minRange": 100,
    "maxRange": 1000,
    "conditionProperty": "isPremium",
    "conditionValue": true
  }
}
```

### Validation Logic

- Range validation only applies when `OtherProperty == TargetValue`
- Works with numeric types: `int`, `long`, `float`, `double`, `decimal`
- Supports nullable numeric types
- When condition is not met, no range validation is performed

## Multiple Conditional Ranges

You can apply multiple `[ConditionalRange]` attributes to the same property for different conditions:

```csharp
public class ShippingOrder
{
    [AllowedValues("standard", "express", "overnight")]
    public string ShippingMethod { get; set; }

    [ConditionalRange("ShippingMethod", "standard", 1, 30)]    // 1-30 days
    [ConditionalRange("ShippingMethod", "express", 1, 7)]     // 1-7 days
    [ConditionalRange("ShippingMethod", "overnight", 1, 2)]   // 1-2 days
    public int DeliveryDays { get; set; }
}
```

## Integration with Other Attributes

Business rule validators work seamlessly with other Sannr attributes:

```csharp
public class AdvancedModel
{
    [Required]
    [AllowedValues("draft", "review", "approved", "published")]
    public string Status { get; set; }

    [Required]
    [FutureDate]
    public DateTime PublishDate { get; set; }

    [ConditionalRange("Status", "published", 0, 1000000)]
    [Range(0, 100)] // This range applies regardless of status
    public int ViewCount { get; set; }
}
```

## Error Messages

All business rule validators support custom error messages:

```csharp
public class CustomMessages
{
    [FutureDate(ErrorMessage = "Delivery date must be in the future")]
    public DateTime DeliveryDate { get; set; }

    [AllowedValues("active", "inactive", ErrorMessage = "Status must be either active or inactive")]
    public string Status { get; set; }

    [ConditionalRange("Category", "premium", 100, 1000, ErrorMessage = "Premium items must cost between $100 and $1000")]
    public decimal Price { get; set; }
}
```

## Performance Characteristics

- **Zero Runtime Overhead**: All validation logic is generated at compile-time
- **AOT Compatible**: No reflection or dynamic code generation
- **Memory Efficient**: No allocations for attribute metadata lookups
- **Fast Execution**: Direct static method calls

## Client-Side Integration

Business rule validators generate JSON rules compatible with popular JavaScript validation libraries:

```javascript
// jQuery Validation
$.validator.addMethod("futureDate", function(value, element) {
    return new Date(value) > new Date();
});

$.validator.addMethod("allowedValues", function(value, element, params) {
    return params.indexOf(value) !== -1;
});

// Example usage
const rules = JSON.parse(MyModelValidators.ValidationRulesJson);
$("#myForm").validate({ rules: rules });
```

## Migration from Custom Validators

If you're currently using custom validators for these patterns, migration is straightforward:

**Before:**
```csharp
[CustomValidator(typeof(MyValidators), nameof(MyValidators.ValidateFutureDate))]
public DateTime DeliveryDate { get; set; }

public static class MyValidators
{
    public static ValidationResult ValidateFutureDate(DateTime date)
    {
        return date > DateTime.Now
            ? ValidationResult.Success()
            : ValidationResult.Error("Date must be in the future");
    }
}
```

**After:**
```csharp
[FutureDate]
public DateTime DeliveryDate { get; set; }
```

## Best Practices

1. **Use AllowedValues for enums**: When you need string-based enums without the overhead of actual enums
2. **Combine with Required**: Use `[Required]` with `[AllowedValues]` to ensure a value is both present and valid
3. **Multiple ConditionalRanges**: Stack multiple attributes for complex business logic
4. **Client-Side Consistency**: Always use the generated JSON rules for client-side validation
5. **Error Messages**: Provide clear, business-specific error messages

## Limitations

- `ConditionalRange` only supports equality conditions (not greater than, less than, etc.)
- `AllowedValues` is case-sensitive (consider using `[Sanitize(ToLower = true)]` if needed)
- `FutureDate` uses local time (`DateTime.Now`) rather than UTC

## Roadmap

Future enhancements may include:
- Support for more complex conditional logic (>, <, !=, etc.)
- Case-insensitive allowed values
- UTC-based future date validation
- Date range validation (between two dates)
- Regular expression-based allowed patterns</content>
<parameter name="filePath">/Users/digvijay/source/github/Sannr/docs/BUSINESS_RULE_VALIDATORS.md