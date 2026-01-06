# Migration Tools

Sannr provides powerful CLI tools to help you migrate from other validation libraries to Sannr. The migration tools support both automated conversion (for DataAnnotations) and guided migration (for FluentValidation).

## Overview

The Migration Tools CLI is included with the Sannr package and provides three main commands:

- `analyze` - Analyze existing validation code to understand migration complexity
- `dataannotations` - Automatically migrate from DataAnnotations to Sannr
- `fluentvalidation` - Generate migration guidance for FluentValidation code

## Installation & Execution

The migration CLI is available as a dotnet global tool. Install it using:

```bash
dotnet tool install -g Sannr.Cli
```

Once installed, you can run it using the `sannr` command:

```bash
sannr [command] [options]
```

Alternatively, you can run it without installing from the repository root:

```bash
dotnet run --project src/Sannr.Cli -- [command] [options]
```

## Available Commands

### 1. `analyze`
Scans your codebase to estimate migration effort and detect which libraries you are currently using.

```bash
dotnet run --project src/Sannr.Cli -- analyze --input ./src/MyProject
```

**Options:**
- `--input`: Path to directory or file (Required)
- `--type`: Filter by library (`fluentvalidation`, `dataannotations`, or `auto`)

### 2. `fluentvalidation`
Migrates existing `FluentValidation` code to Sannr. You can choose between converting to **Sannr Attributes** (recommended for simplest models) or keeping the **Sannr Fluent API** structure.

```bash
# Migrate to Sannr Attributes (Adds attributes to your model properties)
dotnet run --project src/Sannr.Cli -- fluentvalidation --input ./Models --output ./Migrated --target attribute

# Migrate to Sannr Fluent Style (Keeps code structure similar to FluentValidation)
dotnet run --project src/Sannr.Cli -- fluentvalidation --input ./Models --output ./Migrated --target fluent
```

**Options:**
- `--input`: Path to source files (Required)
- `--output`: Path to save migrated files (Required)
- `--target`: `attribute` or `fluent` (Default: `fluent`)
- `--overwrite`: Overwrite files in output directory
- `--dry-run`: Preview changes without writing to disk

### 3. `dataannotations`
Automatically converts standard `System.ComponentModel.DataAnnotations` to high-performance `Sannr` equivalents.

```bash
dotnet run --project src/Sannr.Cli -- dataannotations --input ./Models --output ./Models
```

**Options:**
- `--input`: Path to directory or file to analyze (required)
- `--type`: Filter by validation library type (optional, defaults to auto-detect)

**Example Output:**
```
🔍 Analyzing validation code in: ./src/MyApp
🏷️  Library type: auto-detect

📊 Analysis Results:
📁 Files scanned: 42
🏷️  Validation libraries detected: FluentValidation, DataAnnotations
🔄 FluentValidation rules found: 27
   - RuleFor calls: 27
   - Must calls: 3
📝 DataAnnotations found: 58
   - [Required]: 23
   - [StringLength]: 15
   - [EmailAddress]: 8
🎯 Migration Complexity: High
💡 Recommendation: Complex migration - consider phased approach
```

**Complexity Levels:**
- **Low**: < 10 validation rules - Simple migration
- **Medium**: 10-50 validation rules - Review generated code carefully
- **High**: > 50 validation rules - Consider phased migration approach

### DataAnnotations Migration

The `dataannotations` command automatically converts DataAnnotations attributes to Sannr equivalents.

```bash
dotnet run --project Sannr.Cli -- dataannotations --input <path> --output <path> [--overwrite] [--dry-run]
```

**Options:**
- `--input`: Source directory containing DataAnnotations code (required)
- `--output`: Target directory for migrated code (required)
- `--overwrite`: Overwrite existing files in output directory
- `--dry-run`: Show what would be migrated without making changes

**Conversions Performed:**

| DataAnnotations | Sannr | Notes |
|----------------|-------|-------|
| `[Required]` | `[Required]` | Compatible |
| `[EmailAddress]` | `[EmailAddress]` | Direct equivalent |
| `[StringLength(n)]` | `[StringLength(n)]` | Compatible |
| `[MaxLength(n)]` | `[StringLength(n)]` | Converted |
| `[MinLength(n)]` | `[MinLength(n)]` | Compatible |
| `[Range(min, max)]` | `[Range(min, max)]` | Compatible |
| `[Url]` | `[Url]` | Compatible |
| `[Phone]` | `[Phone]` | Compatible |

**Additional Changes:**
- Adds `using Sannr;` directive
- Removes `using System.ComponentModel.DataAnnotations;` if no longer needed

**Example:**

**Before:**
```csharp
using System.ComponentModel.DataAnnotations;

public class User
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }
}
```

**After:**
```csharp
using Sannr;

public class User
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}
```

### FluentValidation Migration

The `fluentvalidation` command analyzes FluentValidation code and provides migration guidance.

```bash
dotnet run --project Sannr.Cli -- fluentvalidation --input <path> --output <path> [--overwrite] [--dry-run]
```

**Options:**
- `--input`: Source directory containing FluentValidation code (required)
- `--output`: Target directory for migrated code (required)
- `--overwrite`: Overwrite existing files in output directory
- `--dry-run`: Show what would be migrated without making changes

**What It Does:**
1. Scans for `AbstractValidator<T>` classes
2. Identifies `RuleFor()` calls and validation rules
3. Adds helpful comments and migration guidance
4. Preserves original code for reference

**Example Output:**

**Before:**
```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Age)
            .GreaterThan(0)
            .LessThan(150);
    }
}
```

**After (with guidance):**
```csharp
using Sannr;

// TODO: Convert to Sannr attributes on model properties
// Original FluentValidation: RuleFor(x => x.Name).NotEmpty().Length(2, 50);
// Original FluentValidation: RuleFor(x => x.Email).NotEmpty().EmailAddress();
// Original FluentValidation: RuleFor(x => x.Age).GreaterThan(0).LessThan(150);
// See migration guide: https://github.com/your-repo/sannr/migration

/*
 * MIGRATION NOTES:
 * 1. Move validation rules from validator classes to model properties as attributes
 * 2. Convert RuleFor(x => x.Name).NotEmpty() to [Required] on Name property
 * 3. Convert RuleFor(x => x.Name).Length(2, 50) to [StringLength(50)] on Name property
 * 4. Convert RuleFor(x => x.Email).EmailAddress() to [EmailAddress] on Email property
 * 5. Convert RuleFor(x => x.Age).GreaterThan(0) to [Range(1, int.MaxValue)] on Age property
 * 6. Remove this validator class after migrating all rules
 */
```

## Migration Strategies

### DataAnnotations Migration Strategy

1. **Backup your code**
2. **Run analysis**: `analyze --input ./Models`
3. **Test migration**: `dataannotations --input ./Models --output ./ModelsMigrated --dry-run`
4. **Apply migration**: `dataannotations --input ./Models --output ./Models --overwrite`
5. **Run tests** to ensure functionality is preserved

### FluentValidation Migration Strategy

1. **Backup your code**
2. **Run analysis**: `analyze --input ./Validators`
3. **Generate guidance**: `fluentvalidation --input ./Validators --output ./Validators --dry-run`
4. **Manual migration**: Follow the TODO comments to move validation rules to model properties
5. **Remove validator classes** after migration is complete
6. **Run tests** to ensure functionality is preserved

### Phased Migration Approach

For large codebases, consider migrating one module at a time:

```bash
# Phase 1: User management
sannr dataannotations --input ./UserModels --output ./UserModels
sannr fluentvalidation --input ./UserValidators --output ./UserValidators --target attribute

# Phase 2: Product management
sannr dataannotations --input ./ProductModels --output ./ProductModels
sannr fluentvalidation --input ./ProductValidators --output ./ProductValidators --target attribute

# Continue with other modules...
```

## Common Migration Patterns

### Converting RuleFor Chains

| FluentValidation | Sannr Attribute |
|------------------|----------------|
| `.NotEmpty()` | `[Required]` |
| `.NotNull()` | `[Required]` |
| `.Length(min, max)` | `[StringLength(max)]` |
| `.MaximumLength(n)` | `[StringLength(n)]` |
| `.MinimumLength(n)` | `[MinLength(n)]` |
| `.EmailAddress()` | `[EmailAddress]` |
| `.GreaterThan(n)` | `[Range(n + 1, int.MaxValue)]` |
| `.LessThan(n)` | `[Range(int.MinValue, n - 1)]` |

### Handling Complex Rules

For complex validation rules that don't have direct attribute equivalents:

```csharp
// Before: Complex FluentValidation rule
RuleFor(x => x.Password)
    .NotEmpty()
    .MinimumLength(8)
    .Matches(@"[A-Z]")
    .Matches(@"[a-z]")
    .Matches(@"[0-9]");

// After: Use CustomValidator
[Required]
[MinLength(8)]
[CustomValidator(typeof(PasswordRules), nameof(PasswordRules.ValidateComplexity))]
public string Password { get; set; }
```

## Troubleshooting

### Migration Tool Not Found

Ensure Sannr.Cli is properly referenced:

```bash
# Check if CLI project exists
ls src/Sannr.Cli/

# Run from correct directory
cd /path/to/your/solution
dotnet run --project src/Sannr.Cli/Sannr.Cli.csproj -- analyze --input ./Models
```

### Files Not Being Processed

- Ensure input path contains `.cs` files
- Check file permissions
- Verify paths are correct (use absolute paths if needed)

### Unexpected Migration Results

- Use `--dry-run` first to preview changes
- Check the analysis output for validation library detection
- Review generated code carefully before applying

## Integration with CI/CD

Add migration tools to your CI/CD pipeline for automated validation:

```yaml
# .github/workflows/migrate-validation.yml
name: Migrate Validation
on: [push]

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Analyze Validation
        run: dotnet run --project src/Sannr.Cli -- analyze --input ./src
      - name: Comment PR
        uses: actions/github-script@v6
        with:
          script: |
            // Post analysis results as PR comment
```

## Next Steps

After migration:

1. **Update using statements** throughout your codebase
2. **Register Sannr** in your DI container: `builder.Services.AddSannr()`
3. **Update tests** to use Sannr validation
4. **Remove old validation libraries** from your project
5. **Update documentation** to reflect the new validation approach

## Support

- **Documentation**: [Sannr Docs](../README.md)
- **Issues**: [GitHub Issues](https://github.com/microsoft/sannr/issues)
- **Discussions**: [GitHub Discussions](https://github.com/microsoft/sannr/discussions)

The migration tools are designed to make your transition to Sannr as smooth as possible while maintaining the high performance and AOT compatibility that Sannr provides.