# ASP.NET Core MVC & Controller Integration

If you aren't using Minimal APIs yet, Sannr still tightly integrates into standard `ControllerBase` architectures.

The Sannr setup automatically installs an `IObjectModelValidator` designed natively to override standard reflection behaviors completely, while correctly populating `ModelState` entries intuitively.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Replaces standard metadata constraints automatically
builder.Services.AddControllers();
builder.Services.AddSannr();

var app = builder.Build();
```

## Creating the Controller

Validation runs the exact same way you expect. Because Sannr's `IObjectModelValidator` intercepts the binder, your controllers gracefully receive pre-validated objects or explicitly bound `ModelState` hierarchies without custom action filters.

```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateUser([FromBody] UserRegistration model)
    {
        // Executes exactly like standard DataAnnotationsbut 20x faster.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Processing...
        return Ok(model);
    }
}
```

## AOT Compatibility Notes

Be aware that `AddControllers()` itself breaks Native AOT guidelines natively inside extensive metadata trees without aggressive explicit route configurations. If you demand absolute `.NET 8 Native AOT` capabilities, we strongly advise evaluating our [Minimal API](/integrations/minimal-apis) patterns instead.
