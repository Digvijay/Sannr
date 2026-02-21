# Architecture

Sannr's architecture is a fundamental shift in how .NET component validation is designed and executed. Instead of leaning heavily on `System.Reflection` at runtime, Sannr executes its validation discovery and rule construction process at compile-time.

## The Problem With Reflection

Standard .NET validation leverages `System.ComponentModel.DataAnnotations` or robust libraries like FluentValidation. To remain loosely coupled and dynamic, these tools wait until your application starts upor often, until the very first validation requestto begin inspecting your `[Required]` or `[MaxLength]` attributes.

This reflection-first architecture incurs serious penalties:
1. **Slow App Startup:** Hundreds of models undergo reflection inspection across multiple request threads.
2. **High Memory Overhead:** Extracted attributes, parsed rules, and compiled expressions stay resident in memory (high GEN2 heap usage).
3. **Execution Sluggishness:** Dynamic expression trees, boxed parameters, and `MethodInfo.Invoke` calls burn clock cycles during *every* HTTP request.
4. **Native AOT Roadblocks:** Reflection-heavy tools struggle with the IL Trimmer. They require special directives or `[RequiresUnreferencedCode]`, which bloats binary sizes and undermines Native AOT guarantees.

## The Solution: AOT Compilers as Validators

Sannr relies exclusively on **Roslyn Source Generators**. Because your C# code structure is totally known at compile-time, we shift the burden of validation discovery and construction entirely onto the MSBuild pipeline.

![Architecture Diagram](/icon.png)

When you run `dotnet build`:
1. The **Sannr Code Analyzer** intercepts the compilation graph.
2. It detects classes adorned with `[Required]`, `[Range]`, and custom validation traits.
3. Automatically generates an optimized, strongly-typed partial class to evaluate those properties and bounds entirely in clean static `C#` logic.
4. Directly binds the validation method as a `Task<ValidationResult>` executor.

### Sample Generated Code

```csharp
[SannrReflect]
public partial class RegisterDto 
{
    [Required, StringLength(10)]
    public string Name { get; set; }
}
```

Translates under-the-hood to hardcoded, zero-allocation condition trees:

```csharp
// System generated AOT-safe validation
public static ValidationResult Validate(RegisterDto model) 
{
    var errors = new ValidationResult();

    // No reflection, fast string lookups. No memory boxing.
    if (string.IsNullOrWhiteSpace(model.Name)) 
    {
        errors.Add("Name", "Name is required.");
    }
    else if (model.Name.Length > 10) 
    {
        errors.Add("Name", "Length cannot exceed 10 chars.");
    }
    
    return errors;
}
```

This ensures **zero allocations** for metadata lookups and guarantees maximum throughput for every single incoming web request.
