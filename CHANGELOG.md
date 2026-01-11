# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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