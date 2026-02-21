# Migration Guide

Sannr includes a powerful CLI tool to help migrate from other validation libraries like FluentValidation and DataAnnotations.

## Installation

The migration CLI is included when you install Sannr:

```bash
dotnet add package Sannr
```

## Analyze Your Project

First, analyze your existing validation code to understand migration complexity:

```bash
# Analyze a directory for validation patterns
dotnet run --project Sannr.Cli -- analyze --input ./MyProject/Models

# Output example:
  Analysis Results:
 Files scanned: 15
   Validation libraries detected: FluentValidation, DataAnnotations
  FluentValidation rules found: 23
  DataAnnotations found: 45
  Migration Complexity: Medium
  Recommendation: Moderate complexity - review generated code carefully
```

## Migrate from DataAnnotations

Convert DataAnnotations attributes to Sannr (automatic conversion):

```bash
# Migrate DataAnnotations in-place
dotnet run --project Sannr.Cli -- dataannotations --input ./Models --output ./Models

# Or migrate to a new directory
dotnet run --project Sannr.Cli -- dataannotations --input ./OldModels --output ./NewModels
```

**What gets converted:**
- `[Required]` -> `[Required]` (Sannr equivalent)
- `[EmailAddress]` -> `[Email]` (Sannr naming)
- `[StringLength(50)]` -> `[StringLength(50)]` (compatible)
- `[MaxLength(100)]` -> `[StringLength(100)]` (converted)
- Adds `using Sannr;` directive

## Migrate from FluentValidation

Get guidance for converting FluentValidation validators:

```bash
# Analyze FluentValidation code
dotnet run --project Sannr.Cli -- fluentvalidation --input ./Validators --dry-run
```

**Generated guidance includes:**
- Comments showing original FluentValidation rules
- TODO items for manual conversion steps
- Migration notes and best practices

### Migration Examples

**Before (FluentValidation):**
```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
```

**After (Sannr with guidance):**
```csharp
using Sannr;

// TODO: Convert to Sannr attributes on model properties
// Original FluentValidation: RuleFor(x => x.Name).NotEmpty().Length(2, 50);
/*
 * MIGRATION NOTES:
 * 1. Move validation rules from validator classes to model properties as attributes
 * 2. Remove this validator class after migrating all rules
 */
```

## CLI Options

```bash
# Analyze validation code
dotnet run --project Sannr.Cli -- analyze --input <path> [--type auto|fluentvalidation|dataannotations]

# Migrate DataAnnotations
dotnet run --project Sannr.Cli -- dataannotations --input <path> --output <path> [--overwrite] [--dry-run]
```
