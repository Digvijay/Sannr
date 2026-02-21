# Sannr Developer CLI

The Sannr CLI provides deep source evaluation to immediately highlight legacy constraints holding back your application performance, and systematically alters `FluentValidation` or `DataAnnotations` implementations into AOT-native syntax instantly.

## Initiating Migrations

Ensure Sannr is installed via NuGet across your project scope, then invoke `dotnet run` mapped strictly into the library's `Sannr.Cli` executable target.

## `analyze` Command

Scans and discovers reflection boundaries preventing comprehensive trimming.

```bash
dotnet run --project Sannr.Cli -- analyze --input ./MyProject/Models
```

## `dataannotations` Extractor

Directly scans C# files and replaces `System.ComponentModel.DataAnnotations` constructs directly with `Sannr` natively compiled directives instantly.

```bash
dotnet run --project Sannr.Cli -- dataannotations --input ./Models --output ./Models
```

## `fluentvalidation` Analyzer

Generates a robust migration map directly interpreting abstract classes and `RuleFor().NotNull()` pipelines back into declarative class targets safely.

```bash
dotnet run --project Sannr.Cli -- fluentvalidation --input ./Validators --dry-run
```

**Flags**:
- `--dry-run`: Maps changes into terminal standard output explicitly avoiding file overwrites safely.
- `--overwrite`: Injects mapping definitions aggressively via AST tree structures.
