# Sannr Console Demo - The "Wow" Factor

**Note:** This demo shows what Sannr generates, but requires Swashbuckle dependencies to compile.  
For a fully working demo without web dependencies, see the [API Demo](../Sannr.Demo.ApiService/) or run directly with:

```bash
# Run the API demo which includes all Sannr features
cd ../Sannr.Demo.AppHost
dotnet run
```

## What This Demo Would Show (Conceptually)

This console demo is designed to showcase Sannr's automatic code generation, but it highlights an important limitation: the source generator currently generates OpenAPI filters for ALL projects, even console apps that don't need them.

### The Wow Factor Remains

Even though this specific demo has a build issue, the **actual wow factor of Sannr** is demonstrated perfectly in the [ASP.NET Core Demo](../Sannr.Demo.ApiService/):

1. **Zero Manual Code** - See [Program.cs](../Sannr.Demo.ApiService/Program.cs) - just one line: `builder.Services.AddSannr()`
2. **Automatic Validation** - See [Models](../Sannr.Demo.ApiService/Models/) - just attributes, no validator classes
3. **Generated Client Code** - Every model automatically gets `.ValidationRulesTypeScript`, `.ValidationRulesJavaScript`, and `.ValidationRulesJson` properties
4. **OpenAPI Integration** - Schema filters generated at compile-time

### What You'd See If This Ran

```csharp
// Your model with attributes
public partial class UserRegistration
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }
}

// Automatically available at compile-time:
var typescript = UserRegistration.ValidationRulesTypeScript;
var javascript = UserRegistration.ValidationRulesJavaScript;
var json = UserRegistration.ValidationRulesJson;
```

## Better Demos to Try

### 1. Web Demo (Full Interactive Experience)
```bash
cd ../Sannr.Demo.AppHost
dotnet run
# Then open the dashboard link and explore!
```

### 2. Check Generated Code Directly
```bash
cd ../Sannr.Demo.ApiService
dotnet build /p:EmitCompilerGeneratedFiles=true /p:CompilerGeneratedFilesOutputPath=obj/Gen

# View generated files:
ls obj/Gen/Sannr.Gen/Sannr.Gen.SannrGenerator/
```

You'll see files like:
- `*.ClientValidation.g.cs` - TypeScript/JavaScript/JSON generators
- `*.SannrValidator.g.cs` - Server-side validators
- `SannrGeneratedSchemaFilter.g.cs` - OpenAPI schema filter

## The Real Wow Factor

**Lines of Code Comparison:**
- Traditional Approach: ~500+ lines of manual validation
- FluentValidation: ~200+ lines of validator classes  
- **Sannr: ~15 lines of attributes**

**And you get:**
- ✅ Server validation (C#)
- ✅ Client validation (TypeScript/JavaScript)
- ✅ JSON validation rules
- ✅ OpenAPI schema extensions
- ✅ 100% AOT compatible
- ✅ Zero reflection

**ALL from the same attribute declarations!**

## See Also

- [Main README](../../README.md) - Project overview
- [Demo Web App](../Sannr.Demo.Web/) - Interactive web demo (BEST DEMO!)
- [API Demo](../Sannr.Demo.ApiService/) - Complete ASP.NET Core integration
