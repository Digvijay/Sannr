# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Refreshed Dependabot-managed dependencies: OpenTelemetry.Api and MessagePack central package pins, and the docs Rollup lockfile. The SDK pin stays on the lowest 10.0 feature band so contributors with older 10.0 SDK installs can still roll forward, while preserving the preview SDK roll-forward needed by the opt-in net11.0 validation leg.

## [1.7.0] - 2026-09-25

### Fixed — found by running CI on GitHub-hosted x64 runners for the first time
- **The AOT validation workflow had never run to completion.** It passed `-p:PublishAot=true` on
  the command line, which creates a *global* property that MSBuild propagates into every
  `ProjectReference` — including the `netstandard2.0` generator, which cannot be AOT-compiled
  (`NETSDK1207`). The flag was also redundant: the target project already declares `PublishAot`.
  Removed; AOT stays configured in the project file, where it does not flow across references.
- **The trim and AOT warnings-as-errors list was never enforced.** It was passed as
  `-p:WarningsAsErrors=IL2026,IL2046,...`, and the dotnet CLI splits `-p:` values on commas, so
  every code after the first was parsed as a separate switch and the run failed with
  `MSB1006: Property is not valid. Switch: IL2046` before compiling anything. The codes are now
  joined with `%3B`, the escaped semicolon.

### Fixed
- **Validation filter silently passed invalid payloads (fail-open).** Two different classes were
  named `SannrValidatorRegistry`: `Sannr.SannrValidatorRegistry` (written to by the generator via
  `[ModuleInitializer]`) and `Sannr.AspNetCore.SannrValidatorRegistry` (never written to). C#
  binds an unqualified name to the one in the containing namespace, so every lookup inside
  `Sannr.AspNetCore` resolved to the permanently empty registry, the filter found no validator,
  and invalid requests returned `200 OK`. Registry lookups are now fully qualified, the duplicate
  filter and extension classes are removed, and the shadow registry is an `[Obsolete]` forwarder.
- **Fail-closed by default.** `WithSannrValidation()` now verifies at startup that a generated
  validator exists for every parameter it is asked to guard, and throws instead of silently
  skipping validation. Opt out with `SannrValidationOptions.RequireValidator = false`.
- **`global.json` prevented the repository from building on any current SDK** (`latestPatch`
  combined with `allowPrerelease: false` pinned an SDK that is no longer installed).
- **A leftover non-incremental `TestGenerator` shipped in the package** and injected a dead
  generated file into every consuming compilation. Removed.
- **The source generator targeted `netstandard2.1`**, which Roslyn does not support for compiler
  extensions (RS1041). Retargeted to `netstandard2.0`.
- **Fluent validators were silently not generated** unless the project also set
  `EnableSannrSchemaGen` or called `AddSannr()`. The fluent test project could not compile for
  that reason and was not in the solution, so nothing noticed. Output is now unconditional.
- **Generator output was not reproducible.** A static set leaked across compilations (validators
  vanished on the second build in the IDE or compiler server), hint names used `Guid.NewGuid()`,
  and templates stamped `DateTime.Now`. All three removed.
- **Debug scaffolding was still injected into every consumer** after the `TestGenerator` removal
  above: `GeneratorInitDebug.g.cs`, `TestGenerator.g.cs`, `ValidationTargetsDebug.g.cs`,
  `FluentValidatorsDebug.g.cs` and `// DEBUG` comments. Removed with an unused syntax parser.
- `dotnet pack --no-build` failed with `NETSDK1085`; `GeneratePackageOnBuild` is removed.
- Removed `System.Net.Http` and `System.Text.RegularExpressions` pins that the SDK already prunes
  (`NU1510` on SDK 11).

### Changed
- Multi-targets `net8.0` (LTS) and `net10.0` (current). `net11.0` builds are validated in CI
  behind an opt-in switch but are not shipped in the released package yet.
- Dependency floors raised to clear advisories: `Microsoft.OpenApi` to 2.12.2
  (GHSA-v5pm-xwqc-g5wc, High) and all `OpenTelemetry` packages to 1.18.0
  (GHSA-g94r-2vxg-569j, Moderate). These floors are security-relevant and must not be lowered.

### Added
- HTTP-level integration tests that assert a `400` is actually returned for an invalid payload.
  The previous unit tests exercised the registry directly and so could not observe the fail-open
  behaviour at all.

## [1.6.0] - 2026-03-04

### Added
- **Universal Class Library Support**: Sannr now works natively in Class Libraries without any ASP.NET Core or OpenAPI dependencies. The source generator automatically detects the environment and only emits Web-related registration code when appropriate.
- **SANN005 Analyzer (Version Safety)**: New warning when a project's `SannrOpenApiVersion` MSBuild property doesn't match the actual `Microsoft.OpenApi` references in the compilation.
- **Improved DI Registration**: Better handling of internal validator registration via `[ModuleInitializer]` that works across assembly boundaries.

### Fixed
- **Source Generator Mismatch**: Resolved an issue where models in Class Libraries were missing their generated validators but still had registration entries in the global initializer.
- **Diagnostic Precision**: SANN004 (missing partial) now provides more accurate source locations and error messages.
- **Generated File Visibility**: Configured `EmitCompilerGeneratedFiles` globally to assist in debugging and verifying generated code.

### Technical Deep Dive
- **Intelligent Environment Detection**: The generator now performs real-time analysis of compilation assembly references. If `Sannr.AspNetCore` symbols are not found, the generator automatically skips emitting Web-specific boilerplate (like `SannrInitializer.g.cs`), making it safe for pure library projects.
- **OpenAPI Version Guard**: A new diagnostic engine examines the `Microsoft.OpenApi.OpenApiSchema` type definition in the compilation. If it detects a mismatch between the linked library version (v1.x vs v2.x) and the provided `SannrOpenApiVersion` MSBuild property, it emits **SANN005** to prevent runtime schema generation failures.
- **Zero-Reflection Registry**: Validators in Class Libraries are now registered using `[ModuleInitializer]`. This ensures that when a library is referenced by a Web project, its validators are automatically registered in the global `SannrValidatorRegistry` before the application starts, maintaining full Native AOT compatibility.

## [1.5.0] - 2026-03-03

### Added
- **Zero-Config Generator Activation**: Sannr's source generator now automatically activates if it detects `AddSannr()` or `AddSannrValidators()` in your code. No more manual MSBuild property configuration required.
- **Combined AOT Initializer**: Consolidated validator registration and OpenAPI schema appliers into a single, high-performance `[ModuleInitializer]`.
- **Automatic Registration**: Removed the need for manual `RegisterGeneratedValidators` partial method implementation. Everything works "out of the box" while maintaining 100% Native AOT compatibility.

### Fixed
- **IDE0055 Warnings**: Resolved all trailing whitespace and formatting warnings in generated code.
- **OpenAPI v2.x Range Mapping**: Correctly handles string-based `Minimum`/`Maximum` for Swashbuckle 10+ (Microsoft.OpenApi v2.x).

## [1.4.0] - 2026-03-02

### Added
- **SANN004 Analyzer**: New build error when a class has Sannr validation attributes but is missing the `partial` keyword.
- **Swashbuckle 10.x Compatibility**: Initial support for `Microsoft.OpenApi` v2.0.
- **Phone, CreditCard, FileExtensions schemas**: Added mappings for specialized formats.

### Migration from v1.3
If you previously used:
```xml
<EnableSannrSchemaGen>true</EnableSannrSchemaGen>
```
You can **remove** this property. The schema filter works automatically.

Change any `options.AddSannrValidationSchemas()` calls to:
```csharp
options.SchemaFilter<SannrGeneratedSchemaFilter>();
```

## [1.3.0] - 2026-01-11

### Added
- **Static Reflection**: Introduced "Shadow Types" (`[SannrReflect]`) for zero-allocation, AOT-compatible inspection and manipulation of models.
- **Deep Cloning**: Generated `DeepClone()` methods for robust, reflection-free object copying using Shadow Types.
- **PII Awareness**: New `[Pii]` attribute and generated `IsPii` metadata for privacy-aware data handling.
- **Visitor Pattern**: Zero-allocation `Visit` method on Shadow Types for efficient property iteration.
- **Documentation**: Comprehensive guides for Static Reflection and updated README with performance comparisons.

## [1.2.0] - 2026-01-06

### Added
- **Security Hardening**: Automated SBOM (Software Bill of Materials) generation in release pipeline using CycloneDX
- **Documentation**: New "Common Pitfalls & Troubleshooting" section in README to assist with Source Generator adoption
- **Feature Verification**: Independent verification of 100% Native AOT compatibility
- **Aspire Integration**: Verified metrics and diagnostics integration with Aspire Dashboard

### Fixed
- **AOT Compatibility**: Removed `dynamic` keyword usage in Source Generator (`Generator.cs`) to ensure strict Native AOT compatibility (`IL3053` resolved)
- **Generated Code**: Fixed `CS0108` (member hiding) and `CS1998` (async/await) warnings in generated validators
- **Code Quality**: Resolved numerous CodeQL warnings including `CA1861` (prefer static readonly), `CA1860` (prefer Length > 0), and `CA13xx` (culture-insensitive string operations)
- **Developer Experience**: Addressed namespace collisions and partial class requirements in documentation

## [1.1.0] - 2025-12-31

### Added
- **Source-Generated Automatic Validator Registration**: `services.AddSannrValidators()` now automatically registers all validators at compile-time, maintaining AOT compatibility
- **Enhanced OpenAPI Integration**: Complete OpenAPI schema generation for all Sannr validation attributes with proper format, minLength, maxLength, minimum, and maximum constraints
- **Comprehensive Migration Tools**: Improved CLI tools for migrating from DataAnnotations and FluentValidation with better attribute parameter handling
- **Observability & Metrics**: Built-in metrics collection for validation performance monitoring and enterprise observability
- **Enhanced Dependency Injection**: Idempotent service registration patterns and improved DI integration
- **Client-Side Validation Generation**: Source-generated JavaScript validators for seamless client-side validation
- **Advanced Error Handling**: Enhanced problem details with validation rule extraction and improved error responses
- **Repository Hygiene & Security**:
  - Added Central Package Management (CPM) via `Directory.Packages.props`
  - Added centralized build configuration via `Directory.Build.props`
  - Implemented GitHub CodeQL static analysis workflow
  - Added `.editorconfig` for project-wide coding standards
  - Improved CI pipeline with automated formatting checks and code coverage collection
  - Added community health files: `CODE_OF_CONDUCT.md`, PR templates, and issue templates
  - Locked .NET SDK version via `global.json`

### Changed
- **AOT Compatibility**: Full Native AOT support with zero reflection in production code paths
- **Performance**: 15-20x performance improvement through source generation and compile-time optimizations
- **Validator Registration**: Moved from runtime reflection to compile-time source generation for automatic registration

### Fixed
- **Documentation**: Corrected migration examples to properly handle MinimumLength parameters
- **OpenAPI Schema Generation**: Fixed attribute casting and decimal handling for Range attributes
- **Build Warnings**: Resolved all compilation warnings and IL trimming issues

### Technical Enhancements
- **Source Generators**: Incremental generators for validators, OpenAPI schemas, and service registration
- **Enterprise Patterns**: Async validation, validation groups, conditional validation, and data sanitization
- **Migration CLI**: Comprehensive tools for converting existing validation code
- **Testing**: 165 comprehensive tests covering all validation scenarios

## [1.0.0] - 2025-12-01

### Added
- Initial release of Sannr validation framework
- Core validation attributes: `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, etc.
- Basic dependency injection integration
- Fundamental OpenAPI schema generation
- Migration tools for DataAnnotations and FluentValidation
- Comprehensive test suite

### Features
- AOT-first validation engine for .NET
- Enterprise-grade validation with async support
- Custom validation rules and business logic validation
- Internationalization support
- Performance monitoring capabilities
