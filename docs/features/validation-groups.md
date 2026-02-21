# Validation Groups (Scenarios)

Validation Groups allow you to reuse a single Data Transfer Object (DTO) or Entity model across multiple business scenarios. Rather than duplicating a `CreateUserDto` and an `UpdateUserDto`, Sannr groups govern which checks fire contextually.

## Defining Group Tags

By default, attributes fire under the `null` or "default" group. You can assign them specifically to tagged validation runs by supplying the `Group` property.

```csharp
public class Product
{
    [Required] // Always evaluates
    public string Name { get; set; }

    // Only evaluates during the "Creation" execution path
    [Required(Group = "Creation")]
    public string InitialStock { get; set; }
    
    // Only evaluates when editing
    [Required(Group = "Update")]
    public string ChangeReasonId { get; set; }
}
```

## Triggering Specific Groups

When executing validation, provide the explicitly named group string to the Context or Validator executor.

```csharp
// Evaluate only the "Creation" rules AND default rules
var result = validator.ValidateAsync(model, group: "Creation");

// Integration with Minimal APIs Wrapper:
app.MapPost("/product", async (Validated<Product> request) =>
{
    // Evaluates with full group scoping automatically defined by your extension setup
});
```
