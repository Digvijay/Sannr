# Limitations & Future Roadmap

## ⚠️ Important Transparency Notice

Sannr is designed as an **AOT-first validation library** using Roslyn source generators. While this provides exceptional performance and Native AOT compatibility, it comes with specific limitations that users must understand from day one. This document outlines current limitations and our roadmap for addressing them.

## Current Limitations

### Build-Time & Development Environment Issues

#### 1. **Unsupported Build Environments**
- **CI/CD Systems**: Azure DevOps, GitHub Actions, or Jenkins with .NET SDK <8.0
- **Legacy Build Servers**: Windows Server 2016/2019 with outdated MSBuild
- **Containerized Builds**: Docker images using .NET 7 or earlier base images

#### 2. **Source Generator Execution Issues**
- **IDE Limitations**: Visual Studio 2022 versions before 17.8
- **Memory-Constrained Builds**: Systems with <8GB RAM
- **Large Codebases**: Projects with >1000 model classes causing generator timeout

### Runtime & Application Architecture Limitations

#### 3. **Dynamic Type Validation**
❌ **Cannot validate** `dynamic`, `ExpandoObject`, or runtime-generated types
❌ **No support for** types created via `System.Reflection.Emit`
❌ **Limited support for** JSON schema validation with dynamic schemas

#### 4. **Complex Inheritance Scenarios**
❌ **Multiple inheritance** with conflicting validation rules
❌ **Complex generic constraints** that generators can't resolve
❌ **Interface-based validation** patterns

#### 5. **Reflection-Dependent Integrations**
❌ **OR/M integration** with Entity Framework reflection-based validation
❌ **Serialization frameworks** expecting reflection metadata
❌ **DI containers** that scan for validation attributes

### Framework-Specific Limitations

#### 6. **ASP.NET Core Edge Cases**
❌ **Runtime controller discovery** scenarios
❌ **Complex model binding** with custom `IModelBinder` implementations
❌ **Tag helpers** relying on validation metadata

#### 7. **Blazor WebAssembly**
❌ **Client-side code generation** (generators don't run in WASM build)
❌ **AOT publishing issues** in complex applications
❌ **JavaScript interop** validation rules

#### 8. **Azure Functions**
❌ **Consumption plan cold starts** with compilation overhead
❌ **Rapid scaling scenarios** where compilation becomes bottleneck
❌ **Complex orchestration patterns**

#### 9. **Microservices Architecture**
❌ **Service mesh integration** expecting standard validation metadata
❌ **API gateway validation** with reflection-based approaches
❌ **Event-driven validation** with dynamic schemas

#### 10. **Container & Orchestration**
❌ **Kubernetes init containers** with resource constraints
❌ **Service discovery** integrations requiring reflection
❌ **Container health checks** depending on validation metadata

### Data & Business Logic Limitations

#### 11. **Complex Business Rules**
❌ **Cross-entity validation** spanning multiple entities
❌ **Temporal validation** rules that change over time
❌ **External data dependencies** requiring database/API lookups

#### 12. **Legacy System Integration**
❌ **Incomplete migration** from existing DataAnnotations
❌ **Third-party libraries** expecting `ValidationAttribute` inheritance
❌ **WCF services** with built-in validation expectations

### Performance & Scale Limitations

#### 13. **High-Throughput Scenarios**
❌ **Sub-millisecond requirements** where any overhead matters
❌ **Large dataset batch processing** with memory constraints
❌ **Streaming validation** requiring state management

#### 14. **Memory-Constrained Environments**
❌ **IoT devices** with <100MB RAM
❌ **Serverless functions** with 128MB memory limits
❌ **Mobile applications** with strict memory constraints

### Development & Maintenance Issues

#### 15. **Debugging Challenges**
❌ **Source generator debugging** difficulties
❌ **Hot reload** development scenarios
❌ **Limited IDE support** for generated code

#### 16. **Version Compatibility**
❌ **.NET Framework** 4.8 or earlier
❌ **Mono/Xamarin** platforms
❌ **Unity game engine** environments

### Security & Compliance Limitations

#### 17. **Runtime Security Scanning**
❌ **SAST tools** unable to analyze generated code
❌ **Runtime instrumentation** expecting reflection metadata
❌ **Compliance auditing** requiring visible validation logic

## Future Roadmap

**All roadmap items maintain 100% AOT compatibility and the source generator approach. No reflection-based fallbacks or runtime code generation will be added.**

### Phase 1: Q1 2026 - Enhanced Compatibility
- [ ] **Improved Build Support**: Better error messages and diagnostics for build failures
- [ ] **Legacy .NET Support**: .NET 6+ compatibility layer (maintaining AOT principles)
- [ ] **Memory Optimization**: Reduced memory usage for large codebases
- [ ] **Source Generator Enhancements**: Improved incremental generation and performance

### Phase 2: Q2 2026 - Framework Integration
- [ ] **Blazor WASM Support**: Client-side validation generation (AOT-compatible)
- [ ] **Azure Functions Optimization**: Cold start optimization for consumption plans
- [ ] **Microservices Patterns**: Service mesh and API gateway integration (metadata-only)
- [ ] **Container Optimization**: Kubernetes-native deployment patterns

### Phase 3: Q3 2026 - Advanced Features
- [ ] **Cross-Entity Validation**: Multi-entity validation patterns with source generation
- [ ] **Temporal Rules**: Time-based validation logic (compile-time configurable)
- [ ] **Conditional Validation Chains**: Complex validation workflows via attributes
- [ ] **Validation Groups**: Enhanced grouping and conditional execution

### Phase 4: Q4 2026 - Enterprise Features
- [ ] **Distributed Validation**: Multi-service validation coordination (AOT-compatible protocols)
- [ ] **Advanced Monitoring**: Performance monitoring and alerting (static instrumentation)
- [ ] **Compliance Tools**: Audit trails and compliance reporting (generated metadata)
- [ ] **Migration Tools**: Enhanced migration from other validation libraries

### Phase 5: 2027 - Ecosystem Expansion
- [ ] **Multi-Platform Support**: Mobile and IoT platforms (AOT-compatible runtimes)
- [ ] **Cloud-Native Patterns**: Serverless and edge computing optimization
- [ ] **Industry-Specific**: Healthcare, finance, and regulated industry features
- [ ] **Global Scale**: High-throughput enterprise scenarios

## Workarounds & Best Practices

### For Dynamic Scenarios
**Important**: Sannr maintains 100% AOT compatibility. For truly dynamic scenarios, consider alternative validation approaches or pre-compile all possible validation paths.

```csharp
// AOT-compatible approach: Pre-compile all known validation paths
public ValidationResult ValidateKnownTypes(object instance)
{
    // Use explicit type checking for all known types
    return instance switch
    {
        UserModel user => SannrValidatorRegistry.GetValidator<UserModel>().Validate(user),
        ProductModel product => SannrValidatorRegistry.GetValidator<ProductModel>().Validate(product),
        OrderModel order => SannrValidatorRegistry.GetValidator<OrderModel>().Validate(order),
        _ => throw new NotSupportedException("Unknown type for validation")
    };
}
```

### For Complex Inheritance
```csharp
// Manual composition pattern
public class ComplexModelValidator
{
    public ValidationResult Validate(ComplexModel model)
    {
        var result = new ValidationResult();

        // Compose multiple validators manually
        result.Merge(BaseValidator.Validate(model));
        result.Merge(InterfaceValidator.Validate(model));
        result.Merge(CustomBusinessRules.Validate(model));

        return result;
    }
}
```

### For Large Codebases
- Split validation into multiple assemblies
- Use incremental generation strategies
- Implement validation grouping to reduce initial load

## Contributing to the Roadmap

We welcome community input on prioritization. Please:

1. **File Issues**: Report specific limitation scenarios you encounter
2. **Propose Solutions**: Suggest architectural approaches for addressing limitations
3. **Share Use Cases**: Describe your application patterns and requirements
4. **Contribute Code**: Help implement workarounds or new features

## Migration Considerations

### From DataAnnotations
- **Supported**: Most attribute-based validation
- **Limited**: Custom `ValidationAttribute` implementations
- **Workaround**: Implement custom validators manually

### From FluentValidation
- **Supported**: Rule-based validation patterns
- **Limited**: Runtime rule modification
- **Workaround**: Pre-compile all validation rules

### From Other Libraries
- **Supported**: Static validation patterns
- **Limited**: Dynamic or reflection-based approaches
- **Workaround**: Use Sannr for static validation, maintain separate validation for dynamic scenarios

## Support & Community

- **Documentation**: [Full Documentation](./README.md)
- **Issues**: [GitHub Issues](https://github.com/Digvijay/Sannr/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Digvijay/Sannr/discussions)

---

*This document is maintained with transparency in mind. We believe users deserve to know limitations upfront rather than discovering them during implementation.*