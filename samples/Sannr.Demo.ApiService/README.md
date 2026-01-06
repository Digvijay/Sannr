# Sannr Demo API Service

This project demonstrates the core capabilities of the **Sannr** validation framework in an ASP.NET Core Minimal API environment.

## 🚀 Key Features Demonstrated

1.  **Automatic Validation**: Using `AddSannr()` and `WithSannrValidation()`, models are automatically validated and sanitized before reaching your endpoint handlers.
2.  **Zero Reflection**: All validators are source-generated at compile-time, making this API 100% Native AOT compatible.
3.  **Automatic Sanitization**: Attributes like `[Sanitize(Trim = true, ToLower = true)]` automatically clean input data.
4.  **Enhanced Error Responses**: Custom problem details with correlation IDs and validation rule metadata.
5.  **Performance Metrics**: Integrated tracking of validation duration and error counts.

## 📂 Project Structure

- `Program.cs`: Setup and endpoint definitions.
- `Models/`: Data models decorated with Sannr attributes.
  - `UserModels.cs`: Basic registration and profile models.
  - `ValidationTestModels.cs`: Comprehensive test model covering all attribute types.
  - `BusinessModels.cs`: Complex business rule examples.

## 🛠️ Integration Example

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Sannr
builder.Services.AddSannr(options => {
    options.EnableMetrics = true;
    options.EnableEnhancedErrorResponses = true;
});

var app = builder.Build();

// Apply validation to a group of endpoints
var api = app.MapGroup("/api").WithSannrValidation();

api.MapPost("/users", (UserModel user) => Results.Ok(user));
```

## 🧪 Testing

You can test these endpoints directly via the `Sannr.Demo.Web` interface or using tools like `curl` or Postman.

```bash
curl -X POST http://localhost:59808/api/test/validation \
     -H "Content-Type: application/json" \
     -d '{"username": "  TestUser  ", "age": 25}'
```
