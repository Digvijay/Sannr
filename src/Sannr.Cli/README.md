# Sannr CLI - Migration Tools

[![NuGet](https://img.shields.io/nuget/v/Sannr.Cli.svg)](https://www.nuget.org/packages/Sannr.Cli/)
[![Downloads](https://img.shields.io/nuget/dt/Sannr.Cli.svg)](https://www.nuget.org/packages/Sannr.Cli/)

The official migration tool for [Sannr](https://github.com/Digvijay/Sannr) - the AOT-first validation framework for .NET.

## 🚀 Quick Start

### Installation

Install as a global dotnet tool:

```bash
dotnet tool install -g Sannr.Cli
```

### Usage

```bash
# Analyze your existing validation code
sannr analyze --input ./src

# Migrate from DataAnnotations
sannr dataannotations --input ./Models --output ./Models

# Migrate from FluentValidation (to attributes)
sannr fluentvalidation --input ./Validators --output ./Models --target attribute

# Migrate from FluentValidation (to Sannr fluent API)
sannr fluentvalidation --input ./Validators --output ./Validators --target fluent
```

## 📋 Commands

### `analyze`
Scans your codebase to understand migration complexity and detect validation libraries.

```bash
sannr analyze --input <path> [--type auto|fluentvalidation|dataannotations]
```

**Example Output:**
```
📊 Analysis Results:
📁 Files scanned: 42
🏷️  Validation libraries detected: FluentValidation, DataAnnotations
🔄 FluentValidation rules found: 28
📝 DataAnnotations found: 156
🎯 Migration Complexity: Medium
💡 Recommendation: Moderate complexity - review generated code carefully
```

### `dataannotations`
Automatically migrates DataAnnotations to Sannr attributes.

```bash
sannr dataannotations --input <path> --output <path> [--overwrite] [--dry-run]
```

**Supported Conversions:**
- `[Required]` → `[Required]`
- `[StringLength]` → `[StringLength]`
- `[EmailAddress]` → `[EmailAddress]`
- `[Range]` → `[Range]`
- `[MaxLength]` → `[StringLength]`
- `[MinLength]` → `[MinLength]`

### `fluentvalidation`
Migrates FluentValidation code to Sannr with two output styles.

```bash
sannr fluentvalidation --input <path> --output <path> --target <attribute|fluent> [--overwrite] [--dry-run]
```

**Migration Targets:**
- **`attribute`**: Converts `RuleFor` chains into Sannr attributes on model properties (recommended for simple models)
- **`fluent`**: Maintains fluent API structure using Sannr's fluent validation (for complex scenarios)

## 🎯 Migration Strategies

### DataAnnotations → Sannr

**Before:**
```csharp
using System.ComponentModel.DataAnnotations;

public class User
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }
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

    [EmailAddress]
    public string Email { get; set; }
}
```

### FluentValidation → Sannr (Attribute Style)

**Before:**
```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

**After:**
```csharp
using Sannr;

public partial class User
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [EmailAddress]
    public string Email { get; set; }
}
```

### FluentValidation → Sannr (Fluent Style)

**Before:**
```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

**After:**
```csharp
using Sannr;

public partial class UserValidator : ValidatorConfig<User>
{
    public override void Configure()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Email).Email();
    }
}
```

## 🛠️ Options

### Common Options

- `--dry-run`: Preview changes without modifying files
- `--overwrite`: Overwrite existing files in the output directory
- `--help`: Show detailed help for any command

### Best Practices

1. **Always analyze first**: Run `sannr analyze` to understand the scope
2. **Use dry-run**: Test migrations with `--dry-run` before applying
3. **Backup your code**: Commit your changes before running migrations
4. **Incremental migration**: Migrate one module at a time for large codebases
5. **Review output**: Always review generated code before deploying

## 📖 Documentation

- **[Complete Migration Guide](https://github.com/Digvijay/Sannr/blob/main/docs/MIGRATION_TOOLS.md)**
- **[Sannr Documentation](https://github.com/Digvijay/Sannr)**
- **[Getting Started with Sannr](https://github.com/Digvijay/Sannr/blob/main/docs/GETTING_STARTED.md)**

## 🔄 Updating

Keep the tool up to date:

```bash
dotnet tool update -g Sannr.Cli
```

## 🤝 Support

- **Issues**: [GitHub Issues](https://github.com/Digvijay/Sannr/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Digvijay/Sannr/discussions)

## 📄 License

MIT License - see the [LICENSE](https://github.com/Digvijay/Sannr/blob/main/LICENSE) file for details.

---

**Made with ❤️ for the .NET community**
