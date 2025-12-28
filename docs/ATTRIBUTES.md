# Supported Attributes Reference

Sannr provides a robust set of validation attributes designed for Native AOT scenarios. While many mirror the standard `System.ComponentModel.DataAnnotations`, Sannr includes additional "Power Attributes" for conditional logic and data sanitization.

All attributes are located in the `Sannr` namespace.

## Table of Contents
- [Standard Validation](#standard-validation)
  - [Required](#required)
  - [StringLength](#stringlength)
  - [Range](#range)
- [Format & Patterns](#format--patterns)
  - [EmailAddress](#emailaddress)
  - [CreditCard](#creditcard)
  - [Url](#url)
  - [Phone](#phone)
  - [FileExtensions](#fileextensions)
- [Advanced Logic](#advanced-logic)
  - [RequiredIf](#requiredif)
  - [CustomValidator](#customvalidator)
- [Data Sanitization](#data-sanitization)
  - [Sanitize](#sanitize)
- [Metadata](#metadata)
  - [Display](#display)

---

## Standard Validation

### `[Required]`
Specifies that a data field is required.

**Behavior:**
* Checks if the value is `null`.
* If the value is a string, it also checks if it is empty or consists only of whitespace.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ErrorMessage` | `string` | Custom error message. |
| `Group` | `string` | validation group (context) this rule applies to. |
| `Severity` | `Severity` | `Error` (Default), `Warning`, or `Info`. |

**Example:**
```csharp
[Required(ErrorMessage = "Username is mandatory.")]
public string Username { get; set; }
```

---

### `[StringLength]`
Specifies the minimum and maximum length of characters that are allowed in a data field.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `MaximumLength` | `int` | **(Constructor)** The maximum allowed length. |
| `MinimumLength` | `int` | The minimum required length (Optional). |

**Example:**
```csharp
[StringLength(50, MinimumLength = 3, ErrorMessage = "Must be between 3 and 50 chars.")]
public string DisplayName { get; set; }
```

---

### `[Range]`
Specifies the numeric range constraints for the value of a data field.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Minimum` | `double` \| `int` | **(Constructor)** The minimum allowed value. |
| `Maximum` | `double` \| `int` | **(Constructor)** The maximum allowed value. |

**Example:**
```csharp
[Range(18, 120, ErrorMessage = "Age must be valid.")]
public int Age { get; set; }

[Range(0.01, 1000.00)]
public double Price { get; set; }
```

---

## Format & Patterns



### `[EmailAddress]`
Validates that the property matches a valid email address format.

**Performance Note:** Sannr uses a pre-compiled, source-generated Regex for maximum throughput.

**Example:**
```csharp
[EmailAddress]
public string ContactEmail { get; set; }
```

---

### `[CreditCard]`
Validates that the property contains a valid credit card number structure (13-19 digits, allowing dashes/spaces).

**Example:**
```csharp
[CreditCard]
public string PaymentCard { get; set; }
```

---

### `[Url]`
Validates that the property is a well-formed URL (starting with `http://` or `https://`).

**Example:**
```csharp
[Url]
public string PortfolioUrl { get; set; }
```

---

### `[Phone]`
Validates that the property contains valid phone characters (digits, spaces, dashes, parentheses, `+`).

**Example:**
```csharp
[Phone]
public string PhoneNumber { get; set; }
```

---

### `[FileExtensions]`
Validates that a string (representing a file name) ends with one of the specified extensions.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Extensions` | `string` | Comma-separated list of extensions (default: "png,jpg,jpeg,gif"). |

**Example:**
```csharp
[FileExtensions(Extensions = "pdf,docx,txt")]
public string ResumeFileName { get; set; }
```

---

## Advanced Logic

### `[RequiredIf]`
Marks a property as required *only if* another property has a specific value.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `OtherProperty` | `string` | **(Constructor)** Name of the property to check against. |
| `TargetValue` | `object` | **(Constructor)** The value that triggers the requirement. |

**Example:**
```csharp
public string Country { get; set; }

// State is strictly required only when Country is "USA"
[RequiredIf(nameof(Country), "USA")]
public string State { get; set; }
```

---

### `[CustomValidator]`
Delegates validation to a custom static method. This is the primary way to implement complex business logic or database checks.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `ValidatorType` | `Type` | **(Constructor)** The class containing the static method. |
| `MethodName` | `string` | The name of the method (Default: "Check"). |
| `IsAsync` | `bool` | Set to `true` if the method returns `Task<ValidationResult>`. |

**Method Signature Requirements:**
* **Sync:** `public static ValidationResult Check(T value, IServiceProvider sp)`
* **Async:** `public static Task<ValidationResult> Check(T value, IServiceProvider sp)`

**Example:**
```csharp
[CustomValidator(typeof(UserRules), nameof(UserRules.IsUniqueAsync), IsAsync = true)]
public string Username { get; set; }

public static class UserRules
{
    public static async Task<ValidationResult> IsUniqueAsync(string username, IServiceProvider sp)
    {
        // ... database check logic ...
        return ValidationResult.Success();
    }
}
```

---

## Data Sanitization



### `[Sanitize]`
Automatically modifies the input data *before* validation rules are executed. This is useful for normalizing data (e.g., standardizing casing or removing accidental whitespace).

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Trim` | `bool` | If true, executes `.Trim()` on the string. |
| `ToUpper` | `bool` | If true, converts string to Uppercase. |
| `ToLower` | `bool` | If true, converts string to Lowercase. |

**Example:**
```csharp
// Input: "  john.doe  "
// Result: "JOHN.DOE"
[Sanitize(Trim = true, ToUpper = true)]
public string UserId { get; set; }
```

---

## Metadata

### `[Display]`
Provides a user-friendly name for the property, which is used in formatted error messages.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `Name` | `string` | The friendly display name. |

**Example:**
```csharp
[Display(Name = "User's Age")]
[Range(18, 99)]
public int Age { get; set; }
// Error: "The field User's Age must be between 18 and 99."
```