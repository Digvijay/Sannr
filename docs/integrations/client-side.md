# Client-Side Validation

Sannr provides automatic generation of client-side validation rules from your server-side validation attributes, ensuring consistency between client and server validation logic.

## Overview

The `[GenerateClientValidators]` attribute tells Sannr to generate JSON validation rules that can be consumed by client-side JavaScript/TypeScript validation libraries.

## Basic Usage

### 1. Mark Your Model

```csharp
using Sannr;

[GenerateClientValidators]
public class UserRegistrationForm
{
    [Required]
    public string? Username { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    [Range(13, 120)]
    public int Age { get; set; }

    [Url]
    public string? Website { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}
```

### 2. Access Generated Rules

After compilation, Sannr adds static properties to your partial class containing your validation rules in various formats:

```csharp
// Accessing rules from your C# code (e.g., to pass to a frontend)
string jsonRules = UserRegistrationForm.ValidationRulesJson;
string tsRules = UserRegistrationForm.ValidationRulesTypeScript;
string jsRules = UserRegistrationForm.ValidationRulesJavaScript;
```

These properties are automatically added to any `partial class` that has validation attributes.

### 3. Use in Client-Side Code

You can expose these rules via an API endpoint:

```csharp
app.MapGet("/api/rules/user-registration", () => 
    Results.Content(UserRegistrationForm.ValidationRulesJavaScript, "application/javascript"));
```

Then consume them in your JavaScript/TypeScript application:

```javascript
// In your HTML/JS
const response = await fetch('/api/rules/user-registration');
const scriptContent = await response.text();
// The script contains: const userRegistrationFormValidators = { ... };
```

## Attribute Mappings

| Sannr Attribute | JSON Rule | Example |
| :--- | :--- | :--- |
| `[Required]` | `"required": true` | `{ "required": true }` |
| `[EmailAddress]` | `"email": true` | `{ "email": true }` |
| `[StringLength(max, min)]` | `"minLength": min, "maxLength": max` | `{ "minLength": 8, "maxLength": 100 }` |
| `[Range(min, max)]` | `"min": min, "max": max` | `{ "min": 13, "max": 120 }` |
| `[Url]` | `"url": true` | `{ "url": true }` |
| `[Phone]` | `"phone": true` | `{ "phone": true }` |
| `[RequiredIf(other, val)]` | `"requiredIf": { "otherProperty": "other", "targetValue": val }` | `{ "requiredIf": { "otherProperty": "country", "targetValue": "USA" } }` |
| `[AllowedValues(vals)]` | `"allowedValues": [vals]` | `{ "allowedValues": ["USD", "EUR"] }` |

## Advanced Configuration

### Custom Namespace

```csharp
[GenerateClientValidators(Namespace = "MyApp.Validation")]
public class ProductForm
{
    // ...
}
```

### Validation Functions (Future)

```csharp
[GenerateClientValidators(GenerateValidationFunctions = true)]
public class ContactForm
{
    // This will generate validation functions in addition to rules
}
```

## Integration Examples

### With jQuery Validation

```javascript
$(document).ready(function() {
    const rules = JSON.parse(UserRegistrationFormValidators.ValidationRulesJson);

    $('#registrationForm').validate({
        rules: rules,
        messages: {
            username: "Please enter a username",
            email: "Please enter a valid email address"
        }
    });
});
```

### With React Hook Form

```typescript
import { useForm } from 'react-hook-form';
import { UserRegistrationFormValidators } from './generated-validators';

const validationRules = JSON.parse(UserRegistrationFormValidators.ValidationRulesJson);

function RegistrationForm() {
    const { register, handleSubmit, errors } = useForm({
        mode: 'onBlur',
        defaultValues: {},
        resolver: (data) => {
            // Apply validation rules
            const result = validateData(data, validationRules);
            return {
                values: result.isValid ? data : {},
                errors: result.errors
            };
        }
    });

    return (
        <form onSubmit={handleSubmit(onSubmit)}>
            <input {...register('username')} />
            {errors.username && <span>{errors.username.message}</span>}
            {/* ... other fields */}
        </form>
    );
}
```

### With Vue.js + Vuelidate

```javascript
import { validationMixin } from 'vuelidate';
import { UserRegistrationFormValidators } from './generated-validators';

const rules = JSON.parse(UserRegistrationFormValidators.ValidationRulesJson);

// Convert Sannr rules to Vuelidate format
const vuelidateRules = convertToVuelidate(rules);

export default {
    mixins: [validationMixin],
    validations: vuelidateRules,
    // ... component code
}
```

## Best Practices

### 1. Single Source of Truth

Keep validation logic in one place - your C# models. Client-side validation becomes a natural extension of your server-side rules.

### 2. Progressive Enhancement

Use client-side validation for better UX, but always validate on the server. Client-side validation can be bypassed.

### 3. Consistent Error Messages

Consider localizing your validation error messages on the client side, or use the same message keys from your server-side validation.

### 4. Performance

Generated JSON is lightweight and can be cached. For large applications, consider lazy-loading validation rules for specific forms.

## Limitations

- Complex validation logic (custom validators, conditional validation) cannot be automatically translated to client-side rules
- Model-level validation (`IValidatableObject`) is not supported on the client side
- Some attributes may require client-side libraries (e.g., credit card validation)

## Troubleshooting

### Generated Code Not Found

Ensure your project references the Sannr.Gen analyzer:

```xml
<ProjectReference Include="..\..\src\Sannr.Gen\Sannr.Gen.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

### Rules Not Generating

- Ensure the class has the `[GenerateClientValidators]` attribute
- Check that properties have Sannr validation attributes
- Rebuild the project to trigger source generation

### JSON Parse Errors

The generated JSON is properly escaped. If you encounter parsing issues, check for special characters in regex patterns or custom error messages.