# Troubleshooting 

Source generators provide unbelievable execution speedsbut resolving integration errors can be tricky within visual code IDEs. Here are extremely common scenarios preventing native evaluations.

## 1. Missing `partial` Keywords

Sannr needs to rewrite parts of your class internally to generate evaluation trees.

**Incorrect:**
```csharp
public class UserDto { } //  Compiler error or no validation generated
```

**Correct:**
```csharp
public partial class UserDto { } // 
```

## 2. Namespace Collisions

Because Sannr emulates standard DataAnnotations for an easy migration path, standard naming collisions occur immediately.

```csharp
using Sannr;
// DO NOT ADD: using System.ComponentModel.DataAnnotations;
```
If you encounter ambiguous reference constraints, aggressively remove older namespaces across your domain models.

## 3. "Validation Not Asserting Properly"
If constraints bypass your evaluation completely:
- Double-check your model is `partial`.
- Ensure `builder.Services.AddSannr()` initializes fully in your `Program.cs`.
- Ensure routes explicitly state `.WithSannrValidation()` or you have hooked into standard ASP.NET filters correctly.

## 4. Unsupported Build Tooling
Legacy MSBuild networks running previous to .NET 8 will crash trying to evaluate AST trees instantly. Ensure your pipeline images update native SDK packages accurately. Visual Studio must exceed `17.8` bounds completely for reliable generation debugging workflows.
