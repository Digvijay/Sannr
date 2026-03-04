# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- Performance monitoring capabilities</content>
<parameter name="filePath">/Users/digvijay/source/github/Sannr/CHANGELOG.md